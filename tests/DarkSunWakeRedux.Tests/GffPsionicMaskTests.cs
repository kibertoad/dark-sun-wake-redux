using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffPsionicMaskTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void ReadsObservedMaskValues(byte mask)
    {
        var selection = GffPsionicMask.Read(new[] { mask }, "synthetic:PSIN#40");

        Assert.Equal(mask, selection.RawMask);
    }

    [Fact]
    public void AcceptsUnobservedCombinationWithinStructuralEnvelope()
    {
        Assert.Equal(3, GffPsionicMask.Read(new byte[] { 3 }, "synthetic").RawMask);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(255)]
    public void RejectsMaskOutsideEnvelope(byte mask)
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            GffPsionicMask.Read(new[] { mask }, "synthetic"));

        Assert.Contains("synthetic", exception.Message);
        Assert.Contains("mask", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void RejectsWrongResourceLength(int length)
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            GffPsionicMask.Read(new byte[length], "synthetic"));

        Assert.Contains("exactly one byte", exception.Message);
    }
}
