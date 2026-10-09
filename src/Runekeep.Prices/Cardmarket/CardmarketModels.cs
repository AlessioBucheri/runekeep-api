using System.Text.Json.Serialization;

namespace Runekeep.Prices.Cardmarket;

/// <summary>price_guide_{game}.json: one entry per product (singles and sealed).</summary>
public sealed record PriceGuideFile(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("priceGuides")] IReadOnlyList<PriceGuideEntry> PriceGuides);

/// <summary>
/// Prices in euro. Any value can be missing (null) when the product had no sales or offers.
/// </summary>
public sealed record PriceGuideEntry(
    [property: JsonPropertyName("idProduct")] long IdProduct,
    [property: JsonPropertyName("idCategory")] int IdCategory,
    [property: JsonPropertyName("avg")] decimal? Avg,
    [property: JsonPropertyName("low")] decimal? Low,
    [property: JsonPropertyName("trend")] decimal? Trend,
    [property: JsonPropertyName("avg1")] decimal? Avg1,
    [property: JsonPropertyName("avg7")] decimal? Avg7,
    [property: JsonPropertyName("avg30")] decimal? Avg30,
    [property: JsonPropertyName("avg-foil")] decimal? AvgFoil,
    [property: JsonPropertyName("low-foil")] decimal? LowFoil,
    [property: JsonPropertyName("trend-foil")] decimal? TrendFoil,
    [property: JsonPropertyName("avg30-foil")] decimal? Avg30Foil);

/// <summary>products_singles_{game}.json: the single cards of the game.</summary>
public sealed record ProductListFile(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("products")] IReadOnlyList<Product> Products);

public sealed record Product(
    [property: JsonPropertyName("idProduct")] long IdProduct,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("idCategory")] int IdCategory,
    [property: JsonPropertyName("categoryName")] string CategoryName,
    [property: JsonPropertyName("idExpansion")] int IdExpansion);
