using System.Text.Json;
using System.Text.RegularExpressions;

namespace Runekeep.Meta.Riot;

/// <summary>What the job needs from a playriftbound.com news article.</summary>
public sealed partial record ArticlePage(string Title, DateTimeOffset? PublishedAt, string BodyHtml)
{
    /// <summary>
    /// The site is a Next.js app: the article (title, date, rich-text body) is in the
    /// <c>__NEXT_DATA__</c> JSON, as "articleMasthead" and "articleRichText" blades.
    /// </summary>
    public static ArticlePage Parse(string html)
    {
        var match = NextData().Match(html);
        if (!match.Success) throw new FormatException("Article page without __NEXT_DATA__.");

        using var doc = JsonDocument.Parse(match.Groups["json"].Value);
        var blades = doc.RootElement
            .GetProperty("props")
            .GetProperty("pageProps")
            .GetProperty("page")
            .GetProperty("blades")
            .EnumerateArray()
            .ToList();

        var masthead = blades.FirstOrDefault(b => TypeOf(b) == "articleMasthead");
        var title = masthead.ValueKind == JsonValueKind.Object && masthead.TryGetProperty("title", out var t)
            ? t.GetString() ?? string.Empty
            : string.Empty;
        DateTimeOffset? published = masthead.ValueKind == JsonValueKind.Object
            && masthead.TryGetProperty("publishDate", out var d)
            && d.TryGetDateTimeOffset(out var date)
                ? date
                : null;
        var body = string.Concat(
            blades
                .Where(b => TypeOf(b) == "articleRichText")
                .Select(b => b.GetProperty("richText").GetProperty("body").GetString()));

        return new ArticlePage(title, published, body);
    }

    private static string? TypeOf(JsonElement blade) =>
        blade.TryGetProperty("type", out var type) ? type.GetString() : null;

    [GeneratedRegex("""<script id="__NEXT_DATA__"[^>]*>(?<json>.*?)</script>""", RegexOptions.Singleline)]
    private static partial Regex NextData();
}
