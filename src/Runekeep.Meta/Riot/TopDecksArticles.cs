using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Runekeep.Meta.Riot;

/// <summary>Finds the "Top Decks" articles and names their event.</summary>
public static partial class TopDecksArticles
{
    /// <summary>English sitemap of the official site (it lists the playriftbound.com news pages).</summary>
    public static readonly Uri Sitemap = new("https://riftbound.leagueoflegends.com/en-us/sitemap_loc.xml");

    /// <summary>
    /// Tournament decklist articles are named "…-top-decks" (e.g. "los-angeles-top-decks",
    /// "from-garen-to-annie-rq-houstons-top-decks") or "the-best-decks-out-of-bologna".
    /// </summary>
    public static IReadOnlyList<Uri> FromSitemap(string xml)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        return XDocument.Parse(xml)
            .Descendants(ns + "loc")
            .Select(loc => loc.Value.Trim())
            .Where(url => url.Contains("/en-us/news/", StringComparison.Ordinal) && DeckArticle().IsMatch(url))
            .Distinct(StringComparer.Ordinal)
            .Select(url => new Uri(url))
            .ToList();
    }

    /// <summary>Stable id of an article: the last segment of its address.</summary>
    public static string IdOf(Uri url) => url.AbsolutePath.TrimEnd('/').Split('/')[^1];

    /// <summary>"Los Angeles’ Top Decks" → "Los Angeles"; "The Best Decks out of Bologna" → "Bologna".</summary>
    public static string EventName(string title)
    {
        var clean = title.Replace('’', '\'').Trim();
        var bestOf = BestDecksOutOf().Match(clean);
        if (bestOf.Success) return bestOf.Groups["city"].Value.Trim();
        var topDecks = TopDecksTitle().Match(clean);
        if (!topDecks.Success) return title.Trim();
        var name = topDecks.Groups["city"].Value;
        name = name[(name.LastIndexOf(',') + 1)..].Trim();
        return RegionalQualifier().Replace(name, string.Empty).Trim();
    }

    [GeneratedRegex(@"/(?:[a-z0-9-]*-top-decks|the-best-decks-out-of-[a-z0-9-]+)/?$")]
    private static partial Regex DeckArticle();

    [GeneratedRegex(@"^The Best Decks out of (?<city>.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex BestDecksOutOf();

    [GeneratedRegex(@"^(?<city>.+?)'s?\s+Top Decks$", RegexOptions.IgnoreCase)]
    private static partial Regex TopDecksTitle();

    [GeneratedRegex(@"^RQ\s+")]
    private static partial Regex RegionalQualifier();
}
