using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbsynthiumCountryFlags;

internal static partial class CountryFlagMap
{
    public static IReadOnlyDictionary<string, int> Load(IReadOnlyDictionary<string, int> countryFlags)
    {
        var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var countryFlag in countryFlags)
        {
            if (countryFlag.Value > 0)
            {
                levels[countryFlag.Key.ToUpperInvariant()] = countryFlag.Value;
            }
        }

        return levels;
    }

    public static IReadOnlyDictionary<string, int> Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Country flag config file was not found.", path);
        }

        var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        if (!document.RootElement.TryGetProperty("CountryFlags", out var countryFlags)
            || countryFlags.ValueKind != JsonValueKind.Object)
        {
            return levels;
        }

        foreach (var country in countryFlags.EnumerateObject())
        {
            if (TryReadIndex(country.Value, out var index))
            {
                levels[country.Name.ToUpperInvariant()] = index;
            }
        }

        return levels;
    }

    private static bool TryReadIndex(JsonElement value, out int index)
    {
        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.TryGetInt32(out index);
        }

        if (value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("index", out var nestedIndex)
            && nestedIndex.ValueKind == JsonValueKind.Number)
        {
            return nestedIndex.TryGetInt32(out index);
        }

        index = 0;
        return false;
    }
}
