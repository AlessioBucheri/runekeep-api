using Runekeep.Prices.Snapshot;

namespace Runekeep.Prices.Tests;

public class PriceSnapshotBuilderTests
{
    private static readonly DateTimeOffset GeneratedAt = new(2026, 10, 9, 3, 30, 0, TimeSpan.Zero);

    private static PriceSnapshot Build() => PriceSnapshotBuilder.Build(Fixtures.PriceGuide(), Fixtures.Singles(), GeneratedAt);

    [Fact]
    public void Keeps_only_single_cards()
    {
        var snapshot = Build();

        Assert.Equal(3, snapshot.Count);
        Assert.Equal(["845712", "847156", "898120"], snapshot.Prices.Keys);
        Assert.DoesNotContain("845721", snapshot.Prices.Keys); // sealed product
    }

    [Fact]
    public void Uses_the_30_day_average_and_rounds_to_cents()
    {
        var kaisa = Build().Prices["847156"];

        Assert.Equal(45.77m, kaisa.Avg);
        Assert.Equal(46.41m, kaisa.Trend);
        Assert.Equal(36.99m, kaisa.Low);
        Assert.Equal(0m, kaisa.Foil);
    }

    [Fact]
    public void Falls_back_when_30_day_values_are_missing()
    {
        var newCard = Build().Prices["898120"];

        Assert.Equal(0.12m, newCard.Avg);
        Assert.Equal(0.5m, newCard.Foil);
    }

    [Fact]
    public void Reads_the_foil_30_day_average()
    {
        Assert.Equal(0.17m, Build().Prices["845712"].Foil);
    }

    [Fact]
    public void Carries_the_cardmarket_date_and_generation_time()
    {
        var snapshot = Build();

        Assert.Equal(new DateTimeOffset(2026, 10, 9, 2, 45, 56, TimeSpan.FromHours(2)), snapshot.PriceDate);
        Assert.Equal(GeneratedAt, snapshot.GeneratedAt);
        Assert.Equal(PriceSnapshot.CurrentVersion, snapshot.Version);
        Assert.Equal("cardmarket", snapshot.Source);
    }

    [Theory]
    [InlineData("2026-10-09T02:45:56+0200", 2)]
    [InlineData("2026-01-15T23:00:00-0500", -5)]
    [InlineData("2026-10-09T02:45:56+02:00", 2)]
    public void Parses_cardmarket_offsets(string value, int offsetHours)
    {
        Assert.Equal(TimeSpan.FromHours(offsetHours), PriceSnapshotBuilder.ParseCardmarketDate(value).Offset);
    }

    [Fact]
    public void Round_trips_through_json()
    {
        var snapshot = Build();

        var copy = PriceSnapshotWriter.Deserialize(PriceSnapshotWriter.Serialize(snapshot));

        Assert.NotNull(copy);
        Assert.Equal(snapshot.Count, copy.Count);
        Assert.Equal(snapshot.Prices["847156"], copy.Prices["847156"]);
        Assert.Equal(snapshot.PriceDate, copy.PriceDate);
    }

    [Fact]
    public void Writes_the_shape_the_app_reads()
    {
        var json = PriceSnapshotWriter.Serialize(Build());

        Assert.Contains("\"priceDate\":", json, StringComparison.Ordinal);
        Assert.Contains("\"845712\":{\"avg\":0.04,\"trend\":0.04,\"low\":0.02,\"foil\":0.17}", json, StringComparison.Ordinal);
    }
}
