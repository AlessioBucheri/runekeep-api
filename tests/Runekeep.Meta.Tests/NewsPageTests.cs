using Runekeep.Meta.News;

namespace Runekeep.Meta.Tests;

public class NewsPageTests
{
    private const string Html = """
        <html><script id="__NEXT_DATA__" type="application/json">
        {"props":{"pageProps":{"page":{"blades":[
          {"type":"riotbar"},
          {"type":"articleCardGrid","items":[
            {"title":"Los Angeles’ Top Decks","action":{"payload":{"url":"/en-us/news/organizedplay/los-angeles-top-decks"}},
             "publishedAt":"2026-09-30T16:00:00.000Z","category":{"machineName":"organized_play","title":"Organized Play"},
             "description":{"body":"<p>The Top 8</p>"},"analytics":{"contentId":"a1"},"media":{"url":"https://img/x.jpg"}},
            {"title":"Radiance trailer","action":{"payload":{"url":"https://youtu.be/abc"}},
             "publishedAt":"2026-10-09T20:00:00.000Z","category":{"machineName":"announcements","title":"Announcements"}},
            {"title":"No date","action":{"payload":{"url":"/x"}}}
          ]}
        ]}}}}
        </script></html>
        """;

    [Fact]
    public void Reads_the_article_cards_newest_first_with_absolute_links()
    {
        var items = NewsPage.Parse(Html);

        Assert.Equal(2, items.Count);
        Assert.Equal("Radiance trailer", items[0].Title);
        Assert.Equal("https://youtu.be/abc", items[0].Url);
        var la = items[1];
        Assert.Equal("a1", la.Id);
        Assert.Equal("Los Angeles' Top Decks", la.Title);
        Assert.Equal("https://playriftbound.com/en-us/news/organizedplay/los-angeles-top-decks", la.Url);
        Assert.Equal("organized_play", la.Category);
        Assert.Equal("The Top 8", la.Summary);
        Assert.Equal("https://img/x.jpg", la.Image);
    }
}
