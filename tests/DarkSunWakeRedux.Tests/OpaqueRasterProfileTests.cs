using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class OpaqueRasterProfileTests
{
    [Fact]
    public void ReportsAggregateCandidateRasterStatisticsWithoutRetainingPayload()
    {
        var profile = OpaqueRasterProfile.Create(new byte[] { 0, 0, 2, 0, 3, 2 }, 3, "synthetic");

        Assert.Equal((6, 3, 2, 3, 3), (profile.ResourceByteLength,
            profile.CandidateWidth, profile.CandidateHeight,
            profile.DistinctValueCount, profile.ZeroValueCount));
        Assert.Equal((4, 1, 3, 2), (profile.HorizontalAdjacentPairCount,
            profile.HorizontalEqualPairCount, profile.VerticalAdjacentPairCount,
            profile.VerticalEqualPairCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4097)]
    public void RejectsUnsafeCandidateWidths(int width) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OpaqueRasterProfile.Create(new byte[] { 1 }, width));

    [Fact]
    public void RejectsEmptyOrNonDivisiblePayloads()
    {
        Assert.Throws<InvalidDataException>(() => OpaqueRasterProfile.Create(Array.Empty<byte>(), 1));
        Assert.Throws<InvalidDataException>(() =>
            OpaqueRasterProfile.Create(new byte[] { 1, 2, 3 }, 2));
    }
}
