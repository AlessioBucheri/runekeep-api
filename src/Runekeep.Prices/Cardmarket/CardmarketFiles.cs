namespace Runekeep.Prices.Cardmarket;

/// <summary>
/// Public, daily-updated files that Cardmarket publishes for every game.
/// See https://news.cardmarket.com/en/Magic/were-making-the-price-guide-and-product-catalogue-available-for-download
/// </summary>
public static class CardmarketFiles
{
    /// <summary>Cardmarket's id for Riftbound.</summary>
    public const int RiftboundGameId = 22;

    private const string BaseUrl = "https://downloads.s3.cardmarket.com/productCatalog";

    public static Uri PriceGuide(int gameId = RiftboundGameId) =>
        new($"{BaseUrl}/priceGuide/price_guide_{gameId}.json");

    public static Uri Singles(int gameId = RiftboundGameId) =>
        new($"{BaseUrl}/productList/products_singles_{gameId}.json");
}
