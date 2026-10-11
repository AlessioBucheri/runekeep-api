using Runekeep.Meta.Decklists;
using Runekeep.Meta.Riot;

namespace Runekeep.Meta.Snapshot;

public static class MetaSnapshotBuilder
{
    /// <summary>One event per article that has decklists, newest first.</summary>
    public static MetaSnapshot Build(IEnumerable<(Uri Url, ArticlePage Page)> articles, DateTimeOffset now)
    {
        var events = articles
            .Select(a => new MetaEvent(
                TopDecksArticles.IdOf(a.Url),
                TopDecksArticles.EventName(a.Page.Title),
                a.Page.Title.Trim(),
                a.Page.PublishedAt,
                a.Url.ToString(),
                DecklistParser.Parse(a.Page.BodyHtml),
                AttendanceParser.Parse(a.Page.BodyHtml)))
            .Where(e => e.Decks.Count > 0)
            .OrderByDescending(e => e.Date)
            .ToList();
        return new MetaSnapshot(MetaSnapshot.CurrentVersion, "playriftbound.com", now, events);
    }
}
