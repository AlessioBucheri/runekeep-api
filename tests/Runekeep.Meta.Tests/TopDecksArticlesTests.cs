using Runekeep.Meta.Riot;

namespace Runekeep.Meta.Tests;

public class TopDecksArticlesTests
{
    [Fact]
    public void Picks_the_english_decklist_articles_from_the_sitemap()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml">
              <url><loc>https://playriftbound.com/en-us/news/organizedplay/los-angeles-top-decks/</loc>
                <xhtml:link rel="alternate" hreflang="en-gb" href="https://playriftbound.com/en-gb/news/organizedplay/los-angeles-top-decks/"/></url>
              <url><loc>https://playriftbound.com/en-us/news/organizedplay/the-best-decks-out-of-bologna/</loc></url>
              <url><loc>https://playriftbound.com/en-us/news/announcements/vi--vex-champion-decks/</loc></url>
              <url><loc>https://playriftbound.com/en-us/news/rules-and-releases/deckbuilding-primer/</loc></url>
              <url><loc>https://playriftbound.com/it-it/news/organizedplay/los-angeles-top-decks/</loc></url>
            </urlset>
            """;

        var urls = TopDecksArticles.FromSitemap(xml).Select(TopDecksArticles.IdOf);

        Assert.Equal(new[] { "los-angeles-top-decks", "the-best-decks-out-of-bologna" }, urls);
    }

    [Theory]
    [InlineData("Los Angeles’ Top Decks", "Los Angeles")]
    [InlineData("Singapore’s Top Decks", "Singapore")]
    [InlineData("From Garen to Annie, RQ Houston’s Top Decks", "Houston")]
    [InlineData("The Best Decks out of Bologna", "Bologna")]
    [InlineData("Something else", "Something else")]
    public void Names_the_event(string title, string expected) =>
        Assert.Equal(expected, TopDecksArticles.EventName(title));
}
