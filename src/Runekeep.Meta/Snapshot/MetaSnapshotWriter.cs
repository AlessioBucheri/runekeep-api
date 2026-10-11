using System.Text.Json;
using System.Text.Json.Serialization;

namespace Runekeep.Meta.Snapshot;

public static class MetaSnapshotWriter
{
    public const string FileName = "meta-decks.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(MetaSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Options);

    /// <summary>Writes meta-decks.json into <paramref name="directory"/> and returns its full path.</summary>
    public static async Task<string> WriteAsync(MetaSnapshot snapshot, string directory, CancellationToken ct = default)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, FileName);
        await File.WriteAllTextAsync(path, Serialize(snapshot), ct).ConfigureAwait(false);
        return path;
    }
}
