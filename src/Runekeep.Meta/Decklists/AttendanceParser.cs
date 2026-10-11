using System.Globalization;
using System.Text.RegularExpressions;

namespace Runekeep.Meta.Decklists;

/// <summary>
/// Reads the attendance table of a "Top Decks" article: one row per legend with how many players
/// brought it on Day 1 and how many reached Day 2. Columns vary between articles
/// ("# Day 1 | # Day 2 | % …" or "# Played | % of Field" per day): the first two whole numbers
/// of a row are Day 1 and Day 2, percentages are ignored.
/// </summary>
public static partial class AttendanceParser
{
    public static IReadOnlyList<LegendAttendance> Parse(string bodyHtml)
    {
        foreach (Match table in Table().Matches(bodyHtml))
        {
            var rows = Row()
                .Matches(table.Value)
                .Select(row => Cell().Matches(row.Value).Select(c => HtmlText.Plain(c.Groups["cell"].Value)).ToList())
                .ToList();
            if (rows.Any(cells => cells.Any(c => c.StartsWith("Main Deck", StringComparison.OrdinalIgnoreCase)))) continue;

            var attendance = rows.Select(ToAttendance).OfType<LegendAttendance>().ToList();
            if (attendance.Count > 0) return attendance;
        }
        return [];
    }

    private static LegendAttendance? ToAttendance(List<string> cells)
    {
        if (cells.Count < 3 || cells[0].Length == 0 || cells[0].Equals("Legend", StringComparison.OrdinalIgnoreCase))
            return null;
        var counts = cells
            .Skip(1)
            .Where(c => WholeNumber().IsMatch(c))
            .Select(c => int.Parse(c, CultureInfo.InvariantCulture))
            .ToList();
        return counts.Count >= 2 ? new LegendAttendance(cells[0], counts[0], counts[1]) : null;
    }

    [GeneratedRegex("<table.*?</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex Table();

    [GeneratedRegex("<tr.*?</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex Row();

    [GeneratedRegex("<t[dh][^>]*>(?<cell>.*?)</t[dh]>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex Cell();

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex WholeNumber();
}
