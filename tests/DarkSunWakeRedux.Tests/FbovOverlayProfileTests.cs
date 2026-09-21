using System.Buffers.Binary;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class FbovOverlayProfileTests
{
    [Fact]
    public void ReportsFbovEnvelopeDescriptorsAndBoundedOverlayHeaderAggregates()
    {
        using var executable = new MemoryStream(CreateExecutable());

        var profile = FbovOverlayProfileReader.Read(executable, "synthetic");

        Assert.Equal((160L, 128L, 128L, 16u, 32u, 2, 48L, 1, 1, (ushort)1, (ushort)3,
                1, 1, 1, 4L, 2L),
            (profile.FileByteLength, profile.MzFileByteLength, profile.OverlayStart,
             profile.DeclaredPayloadByteLength, profile.SegmentTableFileOffset,
             profile.SegmentDescriptorCount, profile.SegmentTableEnd,
             profile.AscendingDescriptorRangeCount, profile.DescendingDescriptorRangeCount,
             profile.MinimumSegment, profile.MaximumSegment,
             profile.OverlayHeaderDescriptorCount, profile.OverlayHeaderTrapStubCount,
             profile.OverlayHeaderPayloadRangeCount, profile.TotalOverlayCodeByteLength,
             profile.TotalOverlayFixupByteLength));
        Assert.Equal([new FbovOverlayFlagProfile(3, 1), new FbovOverlayFlagProfile(4, 1)],
            profile.Flags);
    }

    [Fact]
    public void RejectsAnEnvelopeWhosePayloadDoesNotMatchThePhysicalOverlay()
    {
        var bytes = CreateExecutable();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(132, 4), 15);

        Assert.Throws<InvalidDataException>(() =>
            FbovOverlayProfileReader.Read(new MemoryStream(bytes), "synthetic"));
    }

    [Fact]
    public void RejectsDescriptorTableOutsideTheMZFile()
    {
        var bytes = CreateExecutable();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(136, 4), 121);

        Assert.Throws<InvalidDataException>(() =>
            FbovOverlayProfileReader.Read(new MemoryStream(bytes), "synthetic"));
    }

    private static byte[] CreateExecutable()
    {
        var bytes = new byte[160];
        bytes[0] = (byte)'M';
        bytes[1] = (byte)'Z';
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2, 2), 128);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4, 2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8, 2), 4);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(32, 2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(34, 2), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(36, 2), 3);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(38, 2), 5);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(40, 2), 3);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(42, 2), 4);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(44, 2), 4);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(46, 2), 8);
        bytes[80] = 0xcd;
        bytes[81] = 0x3f;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(82, 2), 7);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(84, 4), 0);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(88, 2), 4);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(90, 2), 2);
        bytes[128] = (byte)'F';
        bytes[129] = (byte)'B';
        bytes[130] = (byte)'O';
        bytes[131] = (byte)'V';
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(132, 4), 16);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(136, 4), 32);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(140, 4), 2);
        return bytes;
    }
}
