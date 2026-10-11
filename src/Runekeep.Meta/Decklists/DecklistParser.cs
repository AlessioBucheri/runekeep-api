using System.Globalization;
using System.Text.RegularExpressions;

namespace Runekeep.Meta.Decklists;

/// <summary>
/// Reads the decklists of a "Top Decks" article body. Every deck is an HTML table: player name,
/// "Legend Rank" and "Overall Ranking", then the sections (Legend, Champion, Main Deck, Battlefields,
/// Rune Pool, Sideboard). Older articles have no Legend or Rune Pool and put the runes in the main deck,
/// and may list battlefields without a count: both shapes are accepted.
/// Tables without a "Main Deck" section (the attendance tables) are skipped.
/// </summary>
public static partial class DecklistParser
{
    private enum Part { Header, Legend, Champion, Main, Battlefields, Runes, Sideboard }

    private static readonly Dictionary<string, Part> Headings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Legend"] = Part.Legend,
        ["Champion"] = Part.Champion,
        ["Main Deck"] = Part.Main,
        ["Main"] = Part.Main,
        ["Battlefields"] = Part.Battlefields,
        ["Battlefield"] = Part.Battlefields,
        ["Rune Pool"] = Part.Runes,
        ["Runes"] = Part.Runes,
        ["Sideboard"] = Part.Sideboard,
        ["Side Deck"] = Part.Sideboard,
    };

    public static IReadOnlyList<MetaDeck> Parse(string bodyHtml)
    {
        var decks = new List<MetaDeck>();
        string? section = null;
        foreach (Match block in HeadingOrTable().Matches(bodyHtml))
        {
            if (block.Groups["heading"].Success)
            {
                section = HtmlText.Plain(block.Groups["heading"].Value);
                continue;
            }
            var deck = ParseTable(HtmlText.Lines(block.Value).SelectMany(SplitGlued).ToList(), section);
            if (deck is not null) decks.Add(deck);
        }
        return decks;
    }

    internal static MetaDeck? ParseTable(IReadOnlyList<string> lines, string? section)
    {
        var part = Part.Header;
        var header = new List<string>();
        string? legend = null, champion = null;
        var main = new List<CardCount>();
        var battlefields = new List<CardCount>();
        var runes = new List<CardCount>();
        var sideboard = new List<CardCount>();
        var hasMain = false;

        foreach (var line in lines)
        {
            if (TryHeading(line, out var next, out var inline))
            {
                part = next;
                hasMain |= next == Part.Main;
                if (inline is null) continue;
                AddTo(inline);
                continue;
            }
            AddTo(line);
        }

        if (!hasMain || main.Count == 0) return null;
        var (overall, legendRank, legendPlayers) = Ranks(header);
        return new MetaDeck(
            header.FirstOrDefault(l => !l.Contains(':', StringComparison.Ordinal)) ?? "?",
            section,
            overall,
            legendRank,
            legendPlayers,
            legend,
            champion,
            main,
            battlefields,
            runes,
            sideboard);

        void AddTo(string line)
        {
            if (part == Part.Header)
            {
                header.Add(line);
                return;
            }
            var card = ParseCard(line);
            switch (part)
            {
                case Part.Legend:
                    legend ??= card.Name;
                    break;
                case Part.Champion:
                    champion ??= card.Name;
                    break;
                // Old articles list the runes with the main deck.
                case Part.Main when IsRune(card.Name):
                case Part.Runes:
                    runes.Add(card);
                    break;
                case Part.Main:
                    main.Add(card);
                    break;
                case Part.Battlefields:
                    battlefields.Add(card);
                    break;
                case Part.Sideboard:
                    sideboard.Add(card);
                    break;
            }
        }
    }

    private static bool TryHeading(string line, out Part part, out string? inline)
    {
        var match = Heading().Match(line);
        inline = null;
        part = default;
        if (!match.Success || !Headings.TryGetValue(match.Groups["name"].Value, out part)) return false;
        var rest = match.Groups["rest"].Value.Trim();
        inline = rest.Length > 0 ? rest : null;
        return true;
    }

    /// <summary>A missing line break in the source glues two cards: "Wages of Pain3 Xin Zhao, Vigilant".</summary>
    internal static IEnumerable<string> SplitGlued(string line) =>
        GluedCount().Split(line).Select(part => part.Trim()).Where(part => part.Length > 0);

    internal static CardCount ParseCard(string line)
    {
        var match = CountedCard().Match(line);
        return match.Success
            ? new CardCount(int.Parse(match.Groups["n"].Value, CultureInfo.InvariantCulture), match.Groups["name"].Value)
            : new CardCount(1, line);
    }

    private static bool IsRune(string name) => name.EndsWith(" Rune", StringComparison.OrdinalIgnoreCase);

    /// <summary>"Overall Ranking: #12" and "Legend Rank: #1/22" (or "1 / 22 players").</summary>
    private static (int? Overall, int? LegendRank, int? LegendPlayers) Ranks(IEnumerable<string> header)
    {
        int? overall = null, rank = null, players = null;
        foreach (var line in header)
        {
            var o = OverallRanking().Match(line);
            if (o.Success) overall = int.Parse(o.Groups["n"].Value, CultureInfo.InvariantCulture);
            var l = LegendRanking().Match(line);
            if (l.Success)
            {
                rank = int.Parse(l.Groups["n"].Value, CultureInfo.InvariantCulture);
                players = int.Parse(l.Groups["of"].Value, CultureInfo.InvariantCulture);
            }
        }
        return (overall, rank, players);
    }

    [GeneratedRegex(@"<h[1-2][^>]*>(?<heading>.*?)</h[1-2]>|<table.*?</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex HeadingOrTable();

    /// <summary>A section name alone, or followed by a colon and (in old articles) the card.</summary>
    [GeneratedRegex(@"^(?<name>[A-Za-z ]+?)\s*(?::(?<rest>.*))?$")]
    private static partial Regex Heading();

    [GeneratedRegex(@"(?<=[a-z'])(?=\d{1,2}\s+[A-Z])")]
    private static partial Regex GluedCount();

    [GeneratedRegex(@"^(?<n>\d{1,2})\s*x?\s+(?<name>.+)$")]
    private static partial Regex CountedCard();

    [GeneratedRegex(@"Overall Rank\w*\s*:\s*#?\s*(?<n>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex OverallRanking();

    [GeneratedRegex(@"Legend Rank\w*\s*:\s*#?\s*(?<n>\d+)\s*/\s*(?<of>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex LegendRanking();
}
