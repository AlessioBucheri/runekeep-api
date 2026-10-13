namespace Runekeep.Meta.Riot;

/// <summary>Downloads the news list, the sitemap and the articles of the official Riftbound site.</summary>
public sealed class RiotNewsClient(HttpClient http)
{
    public async Task<IReadOnlyList<Uri>> GetTopDecksArticlesAsync(CancellationToken ct = default) =>
        TopDecksArticles.FromSitemap(await http.GetStringAsync(TopDecksArticles.Sitemap, ct).ConfigureAwait(false));

    public async Task<IReadOnlyList<News.NewsItem>> GetNewsAsync(CancellationToken ct = default) =>
        News.NewsPage.Parse(await http.GetStringAsync(News.NewsPage.Url, ct).ConfigureAwait(false));

    public async Task<ArticlePage> GetArticleAsync(Uri url, CancellationToken ct = default) =>
        ArticlePage.Parse(await http.GetStringAsync(url, ct).ConfigureAwait(false));
}
