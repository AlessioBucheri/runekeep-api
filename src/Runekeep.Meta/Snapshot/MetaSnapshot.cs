using System.Text.Json.Serialization;
using Runekeep.Meta.Decklists;

namespace Runekeep.Meta.Snapshot;

/// <summary>The file the app downloads (meta-decks.json): every event with its decklists, newest first.</summary>
public sealed record MetaSnapshot(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("generatedAt")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("events")] IReadOnlyList<MetaEvent> Events)
{
    public const int CurrentVersion = 1;
}

/// <summary>One "Top Decks" article.</summary>
public sealed record MetaEvent(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("date")] DateTimeOffset? Date,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("decks")] IReadOnlyList<MetaDeck> Decks);
