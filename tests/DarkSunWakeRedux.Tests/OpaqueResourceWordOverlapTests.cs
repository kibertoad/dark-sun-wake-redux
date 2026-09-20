using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class OpaqueResourceWordOverlapTests
{
    [Fact]
    public void ReportsOnlyOffsetsWhoseLittleEndianWindowsMatchTheTargetNamespace()
    {
        var overlap = OpaqueResourceWordOverlap.FindLittleEndianMatches(
            new byte[] { 9, 42, 0, 7, 0 }, new HashSet<uint> { 42, 7 });

        Assert.Equal(5, overlap.SourceByteLength);
        Assert.Equal(4, overlap.CandidateWordCount);
        Assert.Equal([1, 3], overlap.MatchingOffsets);
    }

    [Fact]
    public void HandlesAOneBytePayloadWithoutInventingAWord()
    {
        var overlap = OpaqueResourceWordOverlap.FindLittleEndianMatches(
            new byte[] { 42 }, new HashSet<uint> { 42 });

        Assert.Equal(0, overlap.CandidateWordCount);
        Assert.Empty(overlap.MatchingOffsets);
    }

    [Fact]
    public void RejectsAnEmptyTargetNamespace()
    {
        Assert.Throws<InvalidDataException>(() =>
            OpaqueResourceWordOverlap.FindLittleEndianMatches(
                new byte[] { 1, 0 }, new HashSet<uint>()));
    }
}
