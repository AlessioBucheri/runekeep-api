using System.Net;
using System.Text.RegularExpressions;

namespace Runekeep.Meta.Decklists;

/// <summary>Turns the small HTML fragments of an article into trimmed text lines.</summary>
internal static partial class HtmlText
{
    public static IReadOnlyList<string> Lines(string html)
    {
        var text = LineBreak().Replace(html, "\n");
        text = BlockEnd().Replace(text, "\n");
        text = Tag().Replace(text, string.Empty);
        text = WebUtility.HtmlDecode(text).Replace(' ', ' ');
        return text
            .Split('\n')
            .Select(Normalize)
            .Where(line => line.Length > 0)
            .ToList();
    }

    public static string Plain(string html) => string.Join(' ', Lines(html));

    /// <summary>Single spaces and straight apostrophes, as in the app's catalog.</summary>
    public static string Normalize(string text) =>
        Spaces().Replace(text.Replace('’', '\'').Replace('‘', '\''), " ").Trim();

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreak();

    [GeneratedRegex(@"</(p|h\d|td|th|li|tr|div)>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockEnd();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tag();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();
}
