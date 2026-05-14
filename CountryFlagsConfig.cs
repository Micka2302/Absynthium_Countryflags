using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace AbsynthiumCountryFlags;

public sealed class CountryFlagsConfig : BasePluginConfig
{
    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;

    [JsonPropertyName("CountryFlags")]
    public Dictionary<string, int> CountryFlags { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("GeoLiteCountryDatabasePath")]
    public string GeoLiteCountryDatabasePath { get; set; } = "GeoLite2-Country.mmdb";
}
