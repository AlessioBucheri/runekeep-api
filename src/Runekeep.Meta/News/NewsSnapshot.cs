using System.Text.Json.Serialization;

namespace Runekeep.Meta.News;

/// <summary>
/// One article of the official Riftbound news, as listed on playriftbound.com/en-us/news/.
/// <see cref="Category"/> is Riot's id ("announcements", "organizedplay", "rules-and-releases"…).
/// </summary>
public sealed record NewsItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("date")] DateTimeOffset Date,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("categoryTitle")] string CategoryTitle,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("image")] string? Image);

/// <summary>The file the app downloads (news.json): the latest articles, newest first.</summary>
public sealed record NewsSnapshot(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("generatedAt")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("items")] IReadOnlyList<NewsItem> Items)
{
    public const int CurrentVersion = 1;

    /// <summary>Enough for notifications and a news list; older articles stay on the site.</summary>
    public const int MaxItems = 40;
}

public static class NewsSnapshotWriter
{
    public const string FileName = "news.json";

    private static readonly System.Text.Json.JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(NewsSnapshot snapshot) => System.Text.Json.JsonSerializer.Serialize(snapshot, Options);

    /// <summary>Writes news.json into <paramref name="directory"/> and returns its full path.</summary>
    public static async Task<string> WriteAsync(NewsSnapshot snapshot, string directory, CancellationToken ct = default)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, FileName);
        await File.WriteAllTextAsync(path, Serialize(snapshot), ct).ConfigureAwait(false);
        return path;
    }
}
