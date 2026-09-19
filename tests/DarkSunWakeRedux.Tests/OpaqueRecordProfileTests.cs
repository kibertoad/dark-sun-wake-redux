using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class OpaqueRecordProfileTests
{
    [Fact]
    public void ReportsPerColumnStatisticsWithoutRetainingTheInputPayload()
    {
        var profile = OpaqueRecordProfile.Create(new byte[] { 0, 5, 4, 0, 9, 4 }, 2, "synthetic");

        Assert.Equal((6, 2, 3),
            (profile.ResourceByteLength, profile.RecordWidth, profile.RecordCount));
        Assert.Equal(
        [
            new OpaqueRecordColumnProfile(0, 3, 1, 0, 9),
            new OpaqueRecordColumnProfile(1, 3, 1, 0, 5)
        ], profile.Columns);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4097)]
    public void RejectsUnsafeCandidateWidths(int width) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OpaqueRecordProfile.Create(new byte[] { 1 }, width));

    [Fact]
    public void RejectsEmptyOrNonDivisiblePayloads()
    {
        Assert.Throws<InvalidDataException>(() => OpaqueRecordProfile.Create(Array.Empty<byte>(), 1));
        Assert.Throws<InvalidDataException>(() =>
            OpaqueRecordProfile.Create(new byte[] { 1, 2, 3 }, 2));
    }
}
