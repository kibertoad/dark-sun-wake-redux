using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ObjectResourcePatternOverlapTests
{
    [Fact]
    public void ComparesAllFourNeutralWordPositionsWithoutExposingTheirValues()
    {
        IReadOnlySet<uint> patternResources = new HashSet<uint> { 7, 9 };

        var matches = ObjectResourcePatternOverlap.Match(7, 8, 10, 9,
            patternResources);

        Assert.Equal(
        [
            new ObjectRawWordPatternMatch(0, true),
            new ObjectRawWordPatternMatch(6, false),
            new ObjectRawWordPatternMatch(8, false),
            new ObjectRawWordPatternMatch(10, true)
        ], matches);
    }

    [Fact]
    public void EmptySubsetLeavesEveryPositionUnmatched()
    {
        var matches = ObjectResourcePatternOverlap.Match(1, 2, 3, 4,
            new HashSet<uint>());

        Assert.All(matches, match => Assert.False(match.MatchesPatternResource));
    }
}
