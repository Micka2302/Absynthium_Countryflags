using System.Net;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Events;
using MaxMind.Db;
using Microsoft.Extensions.Logging;

namespace AbsynthiumCountryFlags;

[MinimumApiVersion(80)]
public sealed class CountryFlagsPlugin : BasePlugin, IPluginConfig<CountryFlagsConfig>
{
    private const int BadgeRefreshIntervalTicks = 32;
    private const string UnknownCountryCode = "UNKNOWN";

    private readonly Dictionary<int, string> _connectedIps = new();
    private readonly Dictionary<int, int> _wantedLevels = new();

    private IReadOnlyDictionary<string, int> _countryLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    private Reader? _geoIpReader;
    private int _tickCounter;

    public override string ModuleName => "Absynthium_Countryflags";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Absynthium";
    public override string ModuleDescription => "Displays country flag XP icons on the CS2 scoreboard.";

    public CountryFlagsConfig Config { get; set; } = new();

    public void OnConfigParsed(CountryFlagsConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        LoadFiles();

        RegisterListener<Listeners.OnClientConnect>((slot, _, ipAddress) =>
        {
            _connectedIps[slot] = NormalizeIpAddress(ipAddress) ?? ipAddress;
        });

        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);

        RegisterListener<Listeners.OnClientPutInServer>(slot =>
        {
            var player = Utilities.GetPlayerFromSlot(slot);
            if (player is null || !player.IsValid || player.IsBot)
            {
                return;
            }

            ApplyPlayerFlag(player);
        });

        RegisterListener<Listeners.OnClientDisconnect>(slot =>
        {
            _connectedIps.Remove(slot);
            _wantedLevels.Remove(slot);
        });

        RegisterListener<Listeners.OnTick>(RefreshLevels);

        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot))
            {
                ApplyPlayerFlag(player);
            }
        }
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var slot = @event.Userid?.Slot;
        AddTimer(0.2f, () => ApplyPlayerFlagFromSlot(slot));

        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var slot = @event.Userid?.Slot;
        AddTimer(0.2f, () => ApplyPlayerFlagFromSlot(slot));

        return HookResult.Continue;
    }

    private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        var slot = @event.Userid?.Slot;
        AddTimer(0.2f, () =>
        {
            ApplyPlayerFlagFromSlot(slot);
        });

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers().Where(IsUsablePlayer))
        {
            ApplyPlayerFlag(player);
        }

        return HookResult.Continue;
    }

    private void ApplyPlayerFlagFromSlot(int? slot)
    {
        if (slot is null)
        {
            return;
        }

        var player = Utilities.GetPlayerFromSlot(slot.Value);
        if (IsUsablePlayer(player))
        {
            ApplyPlayerFlag(player);
        }
    }

    public override void Unload(bool hotReload)
    {
        _geoIpReader?.Dispose();
    }

    private void LoadFiles()
    {
        _countryLevels = CountryFlagMap.Load(Config.CountryFlags);
        Logger.LogInformation("Loaded {Count} country flag mappings from plugin config.", _countryLevels.Count);

        _geoIpReader?.Dispose();
        _geoIpReader = null;

        var geoPath = ResolvePluginPath(Config.GeoLiteCountryDatabasePath);
        if (File.Exists(geoPath))
        {
            _geoIpReader = new Reader(geoPath);
            Logger.LogInformation("Loaded GeoIP database from {Path}.", geoPath);
        }
        else
        {
            Logger.LogWarning("GeoIP database not found at {Path}. Players will use UNKNOWN until the database is installed.", geoPath);
        }
    }

    private void ApplyPlayerFlag(CCSPlayerController? player)
    {
        if (!IsUsablePlayer(player))
        {
            return;
        }

        var validPlayer = player!;
        var context = ResolvePlayerCountry(validPlayer);
        var level = GetLevelForCountry(context.CountryCode);

        if (level < 0)
        {
            _wantedLevels.Remove(validPlayer.Slot);
            return;
        }

        _wantedLevels[validPlayer.Slot] = level;
        SetCountryFlagBadge(validPlayer, level);
    }

    private CountryResolveContext ResolvePlayerCountry(CCSPlayerController player)
    {
        var originalIp = GetPlayerIp(player);
        if (originalIp is null)
        {
            return new CountryResolveContext(null);
        }

        var lookupIp = originalIp;

        var countryCode = GetCountryCode(lookupIp);
        if (countryCode is null)
        {
            return new CountryResolveContext(null);
        }

        if (!_countryLevels.ContainsKey(countryCode))
        {
            return new CountryResolveContext(countryCode);
        }

        return new CountryResolveContext(countryCode);
    }

    private int GetLevelForCountry(string? countryCode)
    {
        if (countryCode is not null && _countryLevels.TryGetValue(countryCode, out var level))
        {
            return level;
        }

        return GetUnknownLevel();
    }

    private string? GetPlayerIp(CCSPlayerController player)
    {
        if (_connectedIps.TryGetValue(player.Slot, out var connectedIp))
        {
            return NormalizeIpAddress(connectedIp);
        }

        return NormalizeIpAddress(player.IpAddress ?? "");
    }

    private string? GetCountryCode(string ipAddress)
    {
        if (_geoIpReader is null)
        {
            return null;
        }

        try
        {
            var data = _geoIpReader.Find<Dictionary<string, object>>(IPAddress.Parse(ipAddress));
            return TryGetIsoCode(data, "country") ?? TryGetIsoCode(data, "registered_country");
        }
        catch (InvalidDatabaseException exception)
        {
            Logger.LogWarning(exception, "GeoIP database lookup failed for {IpAddress}.", ipAddress);
            return null;
        }
        catch (DeserializationException exception)
        {
            Logger.LogWarning(exception, "GeoIP lookup failed for {IpAddress}.", ipAddress);
            return null;
        }
    }

    private static string? TryGetIsoCode(IReadOnlyDictionary<string, object>? data, string key)
    {
        if (data is null || !data.TryGetValue(key, out var value))
        {
            return null;
        }

        if (value is IReadOnlyDictionary<string, object> country
            && country.TryGetValue("iso_code", out var isoCode)
            && isoCode is string code
            && code.Length == 2)
        {
            return code;
        }

        return null;
    }

    private int GetUnknownLevel()
    {
        return _countryLevels.TryGetValue(UnknownCountryCode, out var level) ? level : -1;
    }

    private void RefreshLevels()
    {
        if (++_tickCounter < BadgeRefreshIntervalTicks)
        {
            return;
        }

        _tickCounter = 0;

        foreach (var player in Utilities.GetPlayers())
        {
            if (!IsUsablePlayer(player))
            {
                continue;
            }

            if (_wantedLevels.TryGetValue(player.Slot, out var wantedLevel))
            {
                SetCountryFlagBadge(player, wantedLevel);
            }
            else
            {
                ApplyPlayerFlag(player);
            }
        }
    }

    private void SetCountryFlagBadge(CCSPlayerController player, int badgeId)
    {
        if (!IsUsablePlayer(player) || player.InventoryServices is null)
        {
            return;
        }

        try
        {
            player.InventoryServices.Rank[5] = (MedalRank_t)badgeId;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInventoryServices");
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Failed to apply country flag badge {BadgeId} to {PlayerName} slot {Slot}.", badgeId, player.PlayerName, player.Slot);
        }
    }

    private static bool IsUsablePlayer(CCSPlayerController? player)
    {
        return player is not null && player.IsValid && !player.IsBot && player.Connected == PlayerConnectedState.Connected && player.SteamID != 0;
    }

    private string ResolvePluginPath(string path)
    {
        return Path.IsPathRooted(path) ? path : Path.Combine(ModuleDirectory, path);
    }

    private static string? NormalizeIpAddress(string rawAddress)
    {
        if (string.IsNullOrWhiteSpace(rawAddress))
        {
            return null;
        }

        var address = rawAddress.Trim();
        if (address.StartsWith('['))
        {
            var closingBracket = address.IndexOf(']', StringComparison.Ordinal);
            if (closingBracket > 0)
            {
                address = address[1..closingBracket];
            }
        }
        else
        {
            var lastColon = address.LastIndexOf(':');
            if (lastColon > 0 && address.Count(character => character == ':') == 1)
            {
                address = address[..lastColon];
            }
        }

        return IPAddress.TryParse(address, out _) ? address : null;
    }

    private sealed record CountryResolveContext(string? CountryCode);
}
