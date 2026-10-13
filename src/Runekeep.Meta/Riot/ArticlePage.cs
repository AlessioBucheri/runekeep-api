using System.Text.Json;

namespace Runekeep.Meta.Riot;

/// <summary>What the job needs from a playriftbound.com news article.</summary>
public sealed record ArticlePage(string Title, DateTimeOffset? PublishedAt, string BodyHtml)
{
    /// <summary>
    /// The site is a Next.js app: the article (title, date, rich-text body) is in the
    /// <c>__NEXT_DATA__</c> JSON, as "articleMasthead" and "articleRichText" blades.
    /// </summary>
    public static ArticlePage Parse(string html)
    {
        using var doc = JsonDocument.Parse(NextData.Json(html));
        var blades = NextData.Blades(doc.RootElement).ToList();

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
}
