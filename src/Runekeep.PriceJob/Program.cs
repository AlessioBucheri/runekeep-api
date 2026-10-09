using Runekeep.Prices.Cardmarket;
using Runekeep.Prices.Snapshot;

// Usage: dotnet run --project src/Runekeep.PriceJob -- [output directory, default "site"]
var outputDirectory = args.Length > 0 ? args[0] : "site";

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("Runekeep-PriceJob/1.0 (+https://github.com/AlessioBucheri/runekeep-api)");
var client = new CardmarketClient(http);

try
{
    var guideTask = client.GetPriceGuideAsync();
    var singlesTask = client.GetSinglesAsync();
    await Task.WhenAll(guideTask, singlesTask);

    var snapshot = PriceSnapshotBuilder.Build(guideTask.Result, singlesTask.Result, DateTimeOffset.UtcNow);
    if (snapshot.Count == 0)
        throw new CardmarketException("No single card prices found: refusing to publish an empty snapshot.");

    var path = await PriceSnapshotWriter.WriteAsync(snapshot, outputDirectory);
    Console.WriteLine($"Wrote {snapshot.Count} prices (Cardmarket {snapshot.PriceDate:yyyy-MM-dd HH:mm zzz}) to {path}");
    return 0;
}
catch (Exception ex) when (ex is CardmarketException or HttpRequestException or TaskCanceledException)
{
    // A failed run keeps yesterday's file online: the workflow stops before publishing.
    Console.Error.WriteLine($"Price job failed: {ex.Message}");
    return 1;
}
