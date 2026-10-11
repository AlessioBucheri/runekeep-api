using System.Text.Json.Serialization;

namespace Runekeep.Meta.Decklists;

/// <summary>One line of a decklist: how many copies of which card, by its printed English name.</summary>
public sealed record CardCount(
    [property: JsonPropertyName("n")] int Count,
    [property: JsonPropertyName("name")] string Name);

/// <summary>
/// A decklist as Riot published it. Card names are kept as printed: the app matches them to its catalog.
/// <see cref="Legend"/> is null in the oldest articles, which only name the champion.
/// </summary>
public sealed record MetaDeck(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("section")] string? Section,
    [property: JsonPropertyName("overallRank")] int? OverallRank,
    [property: JsonPropertyName("legendRank")] int? LegendRank,
    [property: JsonPropertyName("legendPlayers")] int? LegendPlayers,
    [property: JsonPropertyName("legend")] string? Legend,
    [property: JsonPropertyName("champion")] string? Champion,
    [property: JsonPropertyName("main")] IReadOnlyList<CardCount> Main,
    [property: JsonPropertyName("battlefields")] IReadOnlyList<CardCount> Battlefields,
    [property: JsonPropertyName("runes")] IReadOnlyList<CardCount> Runes,
    [property: JsonPropertyName("sideboard")] IReadOnlyList<CardCount> Sideboard);

/// <summary>Players of one legend at the event: on Day 1 and still in on Day 2. The name is as printed ("Master Yi, WB").</summary>
public sealed record LegendAttendance(
    [property: JsonPropertyName("legend")] string Legend,
    [property: JsonPropertyName("day1")] int Day1,
    [property: JsonPropertyName("day2")] int Day2);
