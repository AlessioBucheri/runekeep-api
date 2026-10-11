using Runekeep.Meta.Riot;
using Runekeep.Meta.Snapshot;

// Usage: dotnet run --project src/Runekeep.MetaJob -- [output directory, default "site"]
var outputDirectory = args.Length > 0 ? args[0] : "site";

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(1) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("Runekeep-MetaJob/1.0 (+https://github.com/AlessioBucheri/runekeep-api)");
var client = new RiotNewsClient(http);

try
{
    var urls = await client.GetTopDecksArticlesAsync();
    var articles = new List<(Uri, ArticlePage)>();
    foreach (var url in urls)
    {
        // One article at a time, with a pause: a handful of pages a day, gentle on Riot's site.
        articles.Add((url, await client.GetArticleAsync(url)));
        await Task.Delay(TimeSpan.FromMilliseconds(500));
    }

    var snapshot = MetaSnapshotBuilder.Build(articles, DateTimeOffset.UtcNow);
    var decks = snapshot.Events.Sum(e => e.Decks.Count);
    if (decks == 0) throw new FormatException("No decklists found: refusing to publish an empty file.");

    var path = await MetaSnapshotWriter.WriteAsync(snapshot, outputDirectory);
    Console.WriteLine($"Wrote {decks} decklists from {snapshot.Events.Count} events to {path}");
    foreach (var e in snapshot.Events) Console.WriteLine($"  {e.Date:yyyy-MM-dd} {e.Name}: {e.Decks.Count} decks, {e.Field.Count} legends in the field");
    return 0;
}
catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or FormatException or System.Text.Json.JsonException or System.Xml.XmlException)
{
    // The workflow publishes the prices anyway; the app keeps the decklists it already has.
    Console.Error.WriteLine($"Meta job failed: {ex.Message}");
    return 1;
}
