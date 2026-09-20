using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class OpaqueLaneWordNamespaceProfileTests
{
    [Fact]
    public void ReportsAlignedLaneWordMembershipWithoutExposingValues()
    {
        var profile = OpaqueLaneWordNamespaceProfiler.Create(
            new byte[]
            {
                3, 0, 10, 0, 4, 0, 20, 0,
                5, 0, 10, 0, 6, 0, 10, 0
            }, recordWidth: 8, laneWidth: 4, wordOffset: 2,
            new HashSet<uint> { 10, 20 });

        Assert.Equal((16, 8, 2, 4, 2),
            (profile.ResourceByteLength, profile.RecordWidth, profile.RecordCount,
                profile.LaneWidth, profile.WordOffset));
        Assert.Equal((4, 4, 2, 2, 1),
            (profile.CandidateWordCount, profile.NamespaceMemberCount,
                profile.DistinctCandidateValueCount, profile.DistinctNamespaceMemberCount,
                profile.RecordsWithRepeatedCandidateValues));
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(4, 3)]
    public void RejectsLanesWithoutACompleteSelectedWord(int laneWidth, int wordOffset) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => OpaqueLaneWordNamespaceProfiler.Create(
            new byte[] { 1, 0, 2, 0 }, 4, laneWidth, wordOffset,
            new HashSet<uint> { 1 }));

    [Fact]
    public void RejectsEmptyTargetNamespace() =>
        Assert.Throws<InvalidDataException>(() => OpaqueLaneWordNamespaceProfiler.Create(
            new byte[] { 1, 0, 2, 0 }, 4, 2, 0, new HashSet<uint>()));
}
