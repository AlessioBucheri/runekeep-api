using System.Text.Json;
using System.Text.RegularExpressions;

namespace Runekeep.Meta.Riot;

/// <summary>
/// playriftbound.com is a Next.js site: every page carries its content as JSON in a
/// <c>__NEXT_DATA__</c> script, organised in "blades" (masthead, rich text, card grid…).
/// </summary>
internal static partial class NextData
{
    public static string Json(string html)
    {
        var match = Script().Match(html);
        return match.Success ? match.Groups["json"].Value : throw new FormatException("Page without __NEXT_DATA__.");
    }

    public static IEnumerable<JsonElement> Blades(JsonElement root) =>
        root.GetProperty("props").GetProperty("pageProps").GetProperty("page").GetProperty("blades").EnumerateArray();

    [GeneratedRegex("""<script id="__NEXT_DATA__"[^>]*>(?<json>.*?)</script>""", RegexOptions.Singleline)]
    private static partial Regex Script();
}
