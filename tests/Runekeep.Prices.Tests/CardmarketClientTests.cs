using System.Net;
using System.Text;
using Runekeep.Prices.Cardmarket;

namespace Runekeep.Prices.Tests;

public class CardmarketClientTests
{
    /// <summary>Answers each known URL with a fixture, anything else with 404.</summary>
    private sealed class FakeCardmarket(HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public List<Uri> Requested { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!;
            Requested.Add(url);
            var file = url == CardmarketFiles.PriceGuide() ? "price_guide_22.json"
                : url == CardmarketFiles.Singles() ? "products_singles_22.json"
                : null;

            var response = file is null || status != HttpStatusCode.OK
                ? new HttpResponseMessage(file is null ? HttpStatusCode.NotFound : status)
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(Fixtures.Read(file), Encoding.UTF8, "application/json"),
                };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task Downloads_the_riftbound_files()
    {
        var handler = new FakeCardmarket();
        using var http = new HttpClient(handler);
        var client = new CardmarketClient(http);

        var guide = await client.GetPriceGuideAsync();
        var singles = await client.GetSinglesAsync();

        Assert.Equal(4, guide.PriceGuides.Count);
        Assert.Equal(3, singles.Products.Count);
        Assert.Contains(handler.Requested, u => u.AbsolutePath.EndsWith("price_guide_22.json", StringComparison.Ordinal));
        Assert.Contains(handler.Requested, u => u.AbsolutePath.EndsWith("products_singles_22.json", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Reports_http_errors_clearly()
    {
        using var http = new HttpClient(new FakeCardmarket(HttpStatusCode.Forbidden));
        var client = new CardmarketClient(http);

        var error = await Assert.ThrowsAsync<CardmarketException>(() => client.GetPriceGuideAsync());

        Assert.Contains("403", error.Message, StringComparison.Ordinal);
    }
}
