using System.Text.Json;

namespace Runekeep.Prices.Snapshot;

public static class PriceSnapshotWriter
{
    public const string FileName = "prices.json";

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    public static string Serialize(PriceSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Options);

    public static PriceSnapshot? Deserialize(string json) => JsonSerializer.Deserialize<PriceSnapshot>(json, Options);

    /// <summary>Writes prices.json into <paramref name="directory"/> and returns its full path.</summary>
    public static async Task<string> WriteAsync(PriceSnapshot snapshot, string directory, CancellationToken ct = default)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, FileName);
        await File.WriteAllTextAsync(path, Serialize(snapshot), ct).ConfigureAwait(false);
        return path;
    }
}
