using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

// Golden vectors for RULE-RNG-001, worked out from the spec rather than recorded from the original.
public sealed class NativeRandomTests
{
    [Fact]
    public void AdvancesTheNative32BitLinearCongruentialSequence()
    {
        var random = new NativeRandom(1);

        Assert.Equal((ushort)346, random.Next());
        Assert.Equal(0x015A4E36U, random.State);
        Assert.Equal((ushort)130, random.Next());
        Assert.Equal(0x8082A52FU, random.State);
        Assert.Equal((ushort)10982, random.Next());
        Assert.Equal(0xAAE684BCU, random.State);
    }

    [Fact]
    public void ZeroSeedAdvancesRatherThanUsingAnImplicitDefault()
    {
        var random = new NativeRandom(0);

        Assert.Equal((ushort)0, random.Next());
        Assert.Equal(1U, random.State);
        Assert.Equal((ushort)346, random.Next());
    }

    [Fact]
    public void ZeroModuloDoesNotConsumeTheStream()
    {
        var random = new NativeRandom(1);

        Assert.Equal((ushort)0, random.NextModulo(0));
        Assert.Equal(1U, random.State);
        Assert.Equal((ushort)346, random.Next());
    }

    [Fact]
    public void NonzeroModuloConsumesOneNativeResult()
    {
        var random = new NativeRandom(1);

        Assert.Equal((ushort)3, random.NextModulo(7));
        Assert.Equal(0x015A4E36U, random.State);
        Assert.Equal((ushort)4, random.NextModulo(7));
        Assert.Equal(0x8082A52FU, random.State);
    }

    [Fact]
    public void InclusiveRangeUsesTheNativeScaledResult()
    {
        var random = new NativeRandom(1);

        Assert.Equal((short)1, random.NextInclusive(1, 20));
        Assert.Equal(0x015A4E36U, random.State);
        Assert.Equal((short)4, random.NextInclusive(4, 6));
        Assert.Equal(0x8082A52FU, random.State);
    }

    [Fact]
    public void EqualOrReversedRangeDoesNotConsumeTheStream()
    {
        var random = new NativeRandom(1);

        Assert.Equal((short)5, random.NextInclusive(5, 5));
        Assert.Equal((short)6, random.NextInclusive(6, 4));
        Assert.Equal(1U, random.State);
    }

    [Fact]
    public void RepeatedScaledRollConsumesOncePerPositiveIteration()
    {
        var random = new NativeRandom(1);

        Assert.Equal(2, random.NextScaledSum(2, 6));
        Assert.Equal(0x8082A52FU, random.State);
        Assert.Equal(3, random.NextScaledSum(1, 6));
        Assert.Equal(0xAAE684BCU, random.State);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    public void NonPositiveRepeatedRollDoesNotConsumeTheStream(short count)
    {
        var random = new NativeRandom(1);

        Assert.Equal(0, random.NextScaledSum(count, 6));
        Assert.Equal(1U, random.State);
    }
}
