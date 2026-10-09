using System.Net.Http.Json;

namespace Runekeep.Prices.Cardmarket;

/// <summary>Reads Cardmarket's public files. The HttpClient is injected so tests can fake it.</summary>
public sealed class CardmarketClient(HttpClient http)
{
    public Task<PriceGuideFile> GetPriceGuideAsync(CancellationToken ct = default) =>
        GetAsync<PriceGuideFile>(CardmarketFiles.PriceGuide(), ct);

    public Task<ProductListFile> GetSinglesAsync(CancellationToken ct = default) =>
        GetAsync<ProductListFile>(CardmarketFiles.Singles(), ct);

    private async Task<T> GetAsync<T>(Uri url, CancellationToken ct)
    {
        using var response = await http.GetAsync(url, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new CardmarketException($"{url} answered {(int)response.StatusCode} {response.ReasonPhrase}.");

        return await response.Content.ReadFromJsonAsync<T>(ct).ConfigureAwait(false)
               ?? throw new CardmarketException($"{url} returned an empty document.");
    }
}

public sealed class CardmarketException : Exception
{
    public CardmarketException() { }
    public CardmarketException(string message) : base(message) { }
    public CardmarketException(string message, Exception inner) : base(message, inner) { }
}
