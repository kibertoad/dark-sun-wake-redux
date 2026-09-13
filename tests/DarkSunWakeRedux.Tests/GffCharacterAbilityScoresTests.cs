using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffCharacterAbilityScoresTests
{
    [Fact]
    public void ReadsSixScoresInManualOrder()
    {
        var data = Resource(18, 21, 17, 16, 19, 15);

        var scores = GffCharacterAbilityScores.Read(data, "synthetic:CHAR#7");

        Assert.Equal(new GffCharacterAbilityScores(18, 21, 17, 16, 19, 15), scores);
        Assert.Equal(
            ["Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma"],
            scores.All().Select(item => item.Name));
    }

    [Theory]
    [InlineData(GffCharacterAbilityScores.MinimumScore)]
    [InlineData(GffCharacterAbilityScores.MaximumScore)]
    public void AcceptsInclusiveManualBounds(byte score)
    {
        var scores = GffCharacterAbilityScores.Read(
            Resource(score, score, score, score, score, score), "synthetic");

        Assert.All(scores.All(), item => Assert.Equal(score, item.Value));
    }

    [Theory]
    [InlineData(8, "Strength")]
    [InlineData(25, "Strength")]
    public void RejectsScoreOutsideManualBounds(byte score, string field)
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterAbilityScores.Read(Resource(score, 15, 15, 15, 15, 15), "synthetic"));

        Assert.Contains("synthetic", exception.Message);
        Assert.Contains(field, exception.Message);
    }

    private static byte[] Resource(params byte[] scores)
    {
        Assert.Equal(GffCharacterAbilityScores.ScoreCount, scores.Length);
        var data = new byte[GffCharacterRecordEnvelope.FixedHeaderSize];
        data[0] = GffCharacterRecordEnvelope.SupportedVersion;
        scores.CopyTo(data, GffCharacterAbilityScores.ScoresOffset);
        return data;
    }
}
