using System.Text.Json.Serialization;

namespace Runekeep.Prices.Snapshot;

/// <summary>
/// The file the app downloads (prices.json). Keys of <see cref="Prices"/> are Cardmarket
/// product ids as strings, which are also the card ids in the app's catalog.
/// </summary>
public sealed record PriceSnapshot(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("priceDate")] DateTimeOffset PriceDate,
    [property: JsonPropertyName("generatedAt")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("prices")] IReadOnlyDictionary<string, CardPrice> Prices)
{
    public const int CurrentVersion = 1;
}

/// <summary>Prices in euro, as the app shows them. 0 means "no data".</summary>
public sealed record CardPrice(
    [property: JsonPropertyName("avg")] decimal Avg,
    [property: JsonPropertyName("trend")] decimal Trend,
    [property: JsonPropertyName("low")] decimal Low,
    [property: JsonPropertyName("foil")] decimal Foil);
