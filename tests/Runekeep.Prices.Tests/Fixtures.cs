using System.Text.Json;
using Runekeep.Prices.Cardmarket;

namespace Runekeep.Prices.Tests;

/// <summary>Trimmed copies of the real Cardmarket files (same shape, a few entries).</summary>
internal static class Fixtures
{
    public static string Read(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    public static PriceGuideFile PriceGuide() =>
        JsonSerializer.Deserialize<PriceGuideFile>(Read("price_guide_22.json"))!;

    public static ProductListFile Singles() =>
        JsonSerializer.Deserialize<ProductListFile>(Read("products_singles_22.json"))!;
}
