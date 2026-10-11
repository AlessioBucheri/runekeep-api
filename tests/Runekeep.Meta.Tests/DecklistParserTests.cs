using Runekeep.Meta.Decklists;

namespace Runekeep.Meta.Tests;

public class DecklistParserTests
{
    /// <summary>Shape of the 2026 articles (Atlanta onwards), trimmed.</summary>
    private const string CurrentArticle = """
        <p>Intro with an attendance table.</p>
        <div><figure class="table"><table><tbody><tr><td>Legend</td><td># Played</td></tr><tr><td>Irelia</td><td>193</td></tr></tbody></table></figure></div>
        <h1>Top 8 Decks</h1>
        <div><figure class="table"><table><thead><tr><th colspan="2"><img src="x.jpg"></th></tr></thead><tbody>
        <tr><td colspan="2"><h3>DSG Prismaticism</h3><p><strong>Legend Rank:</strong> #1/92&nbsp;<br><strong>Overall Ranking:</strong> #1</p></td></tr>
        <tr><td><p><strong>Legend:</strong>&nbsp;<br>1 Rengar, Pridestalker</p><p><strong>Champion:</strong>&nbsp;<br>1 Rengar, Trophy Hunter</p>
        <p><strong>Main Deck:</strong>&nbsp;<br>3 Inferna&nbsp;<br>3 Kai’Sa, Survivor&nbsp;<br>2 Thrill of the Hunt</p></td>
        <td><p><strong>Battlefields:</strong>&nbsp;<br>1 Emperor's Dais&nbsp;<br>1 Seat of Power&nbsp;<br>1 Star Spring</p>
        <p><strong>Rune Pool:</strong>&nbsp;<br>8 Body Rune&nbsp;<br>4 Fury Rune</p>
        <p><strong>Sideboard:</strong>&nbsp;<br>2 Brittle Steel</p></td></tr></tbody></table></figure></div>
        """;

    /// <summary>Shape of the first article (RQ Houston, December 2025): no legend, runes in the main deck.</summary>
    private const string OldArticle = """
        <h2>Best-of Decks</h2>
        <table><tbody><tr><td><h3>Frodan</h3><p>Legend Rank: 1 / 31 players<br>Overall Ranking: #234</p></td></tr>
        <tr><td><p><strong>Champion: Leona, Determined</strong></p><p><strong>Main Deck</strong><br>3 Call to Glory<br>2 Zhonya's Hourglass<br>6 Calm Rune<br>6 Order Rune</p>
        <p><strong>Battlefields</strong><br>Grove of the God-Willow<br>Monastery of Hirana</p><p><strong>Sideboard</strong><br>2 Hidden Blade</p></td></tr></tbody></table>
        """;

    [Fact]
    public void Reads_current_decklists_and_skips_other_tables()
    {
        var deck = Assert.Single(DecklistParser.Parse(CurrentArticle));

        Assert.Equal("DSG Prismaticism", deck.Player);
        Assert.Equal("Top 8 Decks", deck.Section);
        Assert.Equal(1, deck.OverallRank);
        Assert.Equal(1, deck.LegendRank);
        Assert.Equal(92, deck.LegendPlayers);
        Assert.Equal("Rengar, Pridestalker", deck.Legend);
        Assert.Equal("Rengar, Trophy Hunter", deck.Champion);
        Assert.Equal(new CardCount[] { new CardCount(3, "Inferna"), new CardCount(3, "Kai'Sa, Survivor"), new CardCount(2, "Thrill of the Hunt") },
            deck.Main);
        Assert.Equal(3, deck.Battlefields.Count);
        Assert.Equal(new CardCount[] { new CardCount(8, "Body Rune"), new CardCount(4, "Fury Rune") }, deck.Runes);
        Assert.Equal(new CardCount[] { new CardCount(2, "Brittle Steel") }, deck.Sideboard);
    }

    [Fact]
    public void Reads_the_oldest_format()
    {
        var deck = Assert.Single(DecklistParser.Parse(OldArticle));

        Assert.Equal("Frodan", deck.Player);
        Assert.Equal("Best-of Decks", deck.Section);
        Assert.Equal(234, deck.OverallRank);
        Assert.Equal(31, deck.LegendPlayers);
        Assert.Null(deck.Legend);
        Assert.Equal("Leona, Determined", deck.Champion);
        Assert.Equal(new CardCount[] { new CardCount(3, "Call to Glory"), new CardCount(2, "Zhonya's Hourglass") }, deck.Main);
        Assert.Equal(new CardCount[] { new CardCount(6, "Calm Rune"), new CardCount(6, "Order Rune") }, deck.Runes);
        Assert.Equal(new CardCount[] { new CardCount(1, "Grove of the God-Willow"), new CardCount(1, "Monastery of Hirana") }, deck.Battlefields);
    }

    [Fact]
    public void Splits_cards_glued_by_a_missing_line_break()
    {
        Assert.Equal(new[] { "2 Wages of Pain", "3 Xin Zhao, Vigilant" }, DecklistParser.SplitGlued("2 Wages of Pain3 Xin Zhao, Vigilant"));
        Assert.Equal(new[] { "Legend Rank: 1 / 31 players" }, DecklistParser.SplitGlued("Legend Rank: 1 / 31 players"));
    }
}
