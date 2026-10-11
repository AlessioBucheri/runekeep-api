using Runekeep.Meta.Decklists;

namespace Runekeep.Meta.Tests;

public class AttendanceParserTests
{
    [Fact]
    public void Reads_day_one_and_day_two_whatever_the_column_order()
    {
        // Barcelona/LA layout: # and % per day; Houston/Atlanta layout: both # first, then both %.
        const string html = """
            <table><tr><td></td><td>Day 1</td><td>Day 2</td></tr>
            <tr><td>Legend</td><td># Played</td><td>% of Field</td><td># Played</td><td>% of Field</td></tr>
            <tr><td>Kennen</td><td>270</td><td>12.7%</td><td>81</td><td>21.4%</td></tr>
            <tr><td>Master Yi, WB</td><td>199</td><td>9.3%</td><td>47</td><td>12.4%</td></tr></table>
            <table><tr><td>Legend:</td></tr><tr><td>Main Deck: 3 Inferna</td></tr></table>
            """;

        var field = AttendanceParser.Parse(html);

        Assert.Equal(new[] { new LegendAttendance("Kennen", 270, 81), new LegendAttendance("Master Yi, WB", 199, 47) }, field);
    }

    [Fact]
    public void Reads_the_other_layout_and_normalizes_apostrophes()
    {
        const string html = """
            <table><tr><th>Legend</th><th>Day 1</th><th>Day 2</th><th>Day 1</th><th>Day 2</th></tr>
            <tr><td>Kai’sa, Daughter of the Void</td><td>372</td><td>27</td><td>33.1%</td><td>42.2%</td></tr>
            <tr><td>Garen</td><td>18</td><td>0</td><td>1.2%</td><td></td></tr></table>
            """;

        Assert.Equal(
            new[] { new LegendAttendance("Kai'sa, Daughter of the Void", 372, 27), new LegendAttendance("Garen", 18, 0) },
            AttendanceParser.Parse(html));
    }

    [Fact]
    public void Returns_nothing_without_an_attendance_table() =>
        Assert.Empty(AttendanceParser.Parse("<p>No tables</p>"));
}
