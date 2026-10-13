using System.Text.Json;
using Runekeep.Meta.Decklists;
using Runekeep.Meta.Riot;

namespace Runekeep.Meta.News;

/// <summary>Reads the article cards of the news page (the "articleCardGrid" blade of its page data).</summary>
public static class NewsPage
{
    public static readonly Uri Url = new("https://playriftbound.com/en-us/news/");

    public static IReadOnlyList<NewsItem> Parse(string html)
    {
        using var doc = JsonDocument.Parse(NextData.Json(html));
        var items = new List<NewsItem>();
        foreach (var blade in NextData.Blades(doc.RootElement))
        {
            if (!blade.TryGetProperty("items", out var cards) || cards.ValueKind != JsonValueKind.Array) continue;
            foreach (var card in cards.EnumerateArray())
            {
                var item = ToItem(card);
                if (item is not null) items.Add(item);
            }
        }
        return items
            .DistinctBy(i => i.Url)
            .OrderByDescending(i => i.Date)
            .Take(NewsSnapshot.MaxItems)
            .ToList();
    }

    private static NewsItem? ToItem(JsonElement card)
    {
        var title = Text(card, "title");
        var url = card.TryGetProperty("action", out var action)
            && action.TryGetProperty("payload", out var payload)
                ? Text(payload, "url")
                : null;
        if (title is null || url is null) return null;
        if (!card.TryGetProperty("publishedAt", out var published) || !published.TryGetDateTimeOffset(out var date))
            return null;

        var category = card.TryGetProperty("category", out var c) ? c : default;
        var summary = card.TryGetProperty("description", out var d) ? Text(d, "body") : null;
        var image = card.TryGetProperty("media", out var m) ? Text(m, "url") : null;
        var id = card.TryGetProperty("analytics", out var a) ? Text(a, "contentId") : null;
        // Riot's own articles are listed with a relative path ("/en-us/news/…"), videos with a full link.
        var link = new Uri(Url, url).ToString();
        return new NewsItem(
            id ?? link,
            HtmlText.Normalize(title),
            link,
            date,
            category.ValueKind == JsonValueKind.Object ? Text(category, "machineName") ?? "news" : "news",
            category.ValueKind == JsonValueKind.Object ? Text(category, "title") ?? "News" : "News",
            summary is null ? null : HtmlText.Plain(summary),
            image);
    }

    private static string? Text(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value)
        && value.ValueKind == JsonValueKind.String
        && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
}
