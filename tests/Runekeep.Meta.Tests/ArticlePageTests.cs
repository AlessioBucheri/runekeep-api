using Runekeep.Meta.Riot;

namespace Runekeep.Meta.Tests;

public class ArticlePageTests
{
    [Fact]
    public void Reads_title_date_and_body_from_next_data()
    {
        const string html = """
            <html><body><script id="__NEXT_DATA__" type="application/json">
            {"props":{"pageProps":{"page":{"blades":[
              {"type":"riotbar"},
              {"type":"articleMasthead","title":"Vegas’ Top Decks","publishDate":"2026-03-04T17:00:00.000Z"},
              {"type":"articleRichText","richText":{"body":"<p>One</p>"}},
              {"type":"articleRichText","richText":{"body":"<p>Two</p>"}}
            ]}}}}
            </script></body></html>
            """;

        var page = ArticlePage.Parse(html);

        Assert.Equal("Vegas’ Top Decks", page.Title);
        Assert.Equal(new DateTimeOffset(2026, 3, 4, 17, 0, 0, TimeSpan.Zero), page.PublishedAt);
        Assert.Equal("<p>One</p><p>Two</p>", page.BodyHtml);
    }

    [Fact]
    public void Rejects_a_page_without_article_data() =>
        Assert.Throws<FormatException>(() => ArticlePage.Parse("<html></html>"));
}
