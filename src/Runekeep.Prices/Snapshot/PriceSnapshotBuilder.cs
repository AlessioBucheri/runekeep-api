using System.Globalization;
using System.Text.RegularExpressions;
using Runekeep.Prices.Cardmarket;

namespace Runekeep.Prices.Snapshot;

/// <summary>Pure transformation: Cardmarket files in, app snapshot out. No I/O.</summary>
public static partial class PriceSnapshotBuilder
{
    public static PriceSnapshot Build(PriceGuideFile guide, ProductListFile singles, DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(guide);
        ArgumentNullException.ThrowIfNull(singles);

        // The price guide also lists sealed products: keep singles only.
        var singleIds = singles.Products.Select(p => p.IdProduct).ToHashSet();

        var prices = guide.PriceGuides
            .Where(e => singleIds.Contains(e.IdProduct))
            .OrderBy(e => e.IdProduct)
            .ToDictionary(
                e => e.IdProduct.ToString(CultureInfo.InvariantCulture),
                ToCardPrice);

        return new PriceSnapshot(
            PriceSnapshot.CurrentVersion,
            "cardmarket",
            ParseCardmarketDate(guide.CreatedAt),
            generatedAt,
            prices.Count,
            prices);
    }

    /// <summary>
    /// "Medio" in the app is the 30-day average, like the prototype; when a card has
    /// no 30-day history it falls back to the overall average.
    /// </summary>
    internal static CardPrice ToCardPrice(PriceGuideEntry e) => new(
        Avg: Round(e.Avg30 ?? e.Avg),
        Trend: Round(e.Trend),
        Low: Round(e.Low),
        Foil: Round(e.Avg30Foil ?? e.AvgFoil ?? e.TrendFoil));

    private static decimal Round(decimal? value) =>
        value is > 0 ? Math.Round(value.Value, 2, MidpointRounding.AwayFromZero) : 0m;

    /// <summary>Cardmarket writes offsets without a colon ("+0200"); ISO 8601 parsers want "+02:00".</summary>
    internal static DateTimeOffset ParseCardmarketDate(string value)
    {
        var iso = OffsetWithoutColon().Replace(value.Trim(), "$1:$2");
        return DateTimeOffset.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    [GeneratedRegex(@"([+-]\d{2})(\d{2})$")]
    private static partial Regex OffsetWithoutColon();
}
