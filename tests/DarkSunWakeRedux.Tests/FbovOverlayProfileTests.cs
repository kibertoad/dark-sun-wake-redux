using System.Buffers.Binary;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class FbovOverlayProfileTests
{
    [Fact]
    public void ReportsOnlyFbovEnvelopeAndDescriptorAggregates()
    {
        using var executable = new MemoryStream(CreateExecutable());

        var profile = FbovOverlayProfileReader.Read(executable, "synthetic");

        Assert.Equal((64L, 32L, 32L, 16u, 8u, 2, 24L, 1, 1, (ushort)3, (ushort)9),
            (profile.FileByteLength, profile.MzFileByteLength, profile.OverlayStart,
             profile.DeclaredPayloadByteLength, profile.SegmentTableFileOffset,
             profile.SegmentDescriptorCount, profile.SegmentTableEnd,
             profile.AscendingDescriptorRangeCount, profile.DescendingDescriptorRangeCount,
             profile.MinimumSegment, profile.MaximumSegment));
        Assert.Equal([new FbovOverlayFlagProfile(1, 1), new FbovOverlayFlagProfile(4, 1)],
            profile.Flags);
    }

    [Fact]
    public void RejectsAnEnvelopeWhosePayloadDoesNotMatchThePhysicalOverlay()
    {
        var bytes = CreateExecutable();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(36, 4), 15);

        Assert.Throws<InvalidDataException>(() =>
            FbovOverlayProfileReader.Read(new MemoryStream(bytes), "synthetic"));
    }

    [Fact]
    public void RejectsDescriptorTableOutsideTheMZFile()
    {
        var bytes = CreateExecutable();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(40, 4), 25);

        Assert.Throws<InvalidDataException>(() =>
            FbovOverlayProfileReader.Read(new MemoryStream(bytes), "synthetic"));
    }

    private static byte[] CreateExecutable()
    {
        var bytes = new byte[64];
        bytes[0] = (byte)'M';
        bytes[1] = (byte)'Z';
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2, 2), 32);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4, 2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8, 2), 3);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(10, 2), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(12, 2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(14, 2), 5);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(16, 2), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(18, 2), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(20, 2), 4);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(22, 2), 8);
        bytes[32] = (byte)'F';
        bytes[33] = (byte)'B';
        bytes[34] = (byte)'O';
        bytes[35] = (byte)'V';
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(36, 4), 16);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(40, 4), 8);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(44, 4), 2);
        return bytes;
    }
}
