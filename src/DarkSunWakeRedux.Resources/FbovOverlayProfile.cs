using System.Buffers.Binary;

namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Reports the self-describing envelope of a physical FBOV overlay without
/// decoding its payload or assigning executable meaning to its descriptors.
/// </summary>
public sealed record FbovOverlayFlagProfile(ushort Flags, int DescriptorCount);

public sealed record FbovOverlayProfile(
    long FileByteLength,
    long MzFileByteLength,
    long OverlayStart,
    uint DeclaredPayloadByteLength,
    uint SegmentTableFileOffset,
    int SegmentDescriptorCount,
    long SegmentTableEnd,
    int AscendingDescriptorRangeCount,
    int DescendingDescriptorRangeCount,
    ushort MinimumSegment,
    ushort MaximumSegment,
    IReadOnlyList<FbovOverlayFlagProfile> Flags);

public static class FbovOverlayProfileReader
{
    public const long MaximumExecutableBytes = 16L * 1024 * 1024;
    public const int MaximumSegmentDescriptorCount = 65_536;
    private const int MzHeaderMinimumLength = 6;
    private const int FbovEnvelopeLength = 16;
    private const int SegmentDescriptorLength = 8;

    public static FbovOverlayProfile Read(Stream stream, string sourceName = "executable")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < MzHeaderMinimumLength || stream.Length > MaximumExecutableBytes)
            throw Error(sourceName, $"has invalid file length {stream.Length}");

        Span<byte> mzHeader = stackalloc byte[MzHeaderMinimumLength];
        ReadExactly(stream, mzHeader, sourceName, "MZ header");
        if (mzHeader[0] != (byte)'M' || mzHeader[1] != (byte)'Z')
            throw Error(sourceName, "does not begin with an MZ header");
        var finalPageBytes = BinaryPrimitives.ReadUInt16LittleEndian(mzHeader[2..]);
        var pageCount = BinaryPrimitives.ReadUInt16LittleEndian(mzHeader[4..]);
        if (pageCount == 0 || finalPageBytes > 512)
            throw Error(sourceName, "has invalid MZ page counts");
        var mzFileByteLength = finalPageBytes == 0
            ? checked((long)pageCount * 512)
            : checked(((long)pageCount - 1) * 512 + finalPageBytes);
        if (mzFileByteLength < MzHeaderMinimumLength ||
            mzFileByteLength > stream.Length - FbovEnvelopeLength)
            throw Error(sourceName, "does not leave a complete FBOV envelope after the MZ file");

        stream.Position = mzFileByteLength;
        Span<byte> envelope = stackalloc byte[FbovEnvelopeLength];
        ReadExactly(stream, envelope, sourceName, "FBOV envelope");
        if (envelope[0] != (byte)'F' || envelope[1] != (byte)'B' ||
            envelope[2] != (byte)'O' || envelope[3] != (byte)'V')
            throw Error(sourceName, "does not begin its physical overlay with FBOV");
        var declaredPayloadLength = BinaryPrimitives.ReadUInt32LittleEndian(envelope[4..]);
        var segmentTableFileOffset = BinaryPrimitives.ReadUInt32LittleEndian(envelope[8..]);
        var descriptorCount = BinaryPrimitives.ReadInt32LittleEndian(envelope[12..]);
        if (descriptorCount is < 1 or > MaximumSegmentDescriptorCount)
            throw Error(sourceName, $"declares invalid FBOV descriptor count {descriptorCount}");
        if (declaredPayloadLength != stream.Length - mzFileByteLength - FbovEnvelopeLength)
            throw Error(sourceName, "declared FBOV payload length does not match the physical overlay");

        var segmentTableEnd = checked((long)segmentTableFileOffset +
            (long)descriptorCount * SegmentDescriptorLength);
        if (segmentTableFileOffset >= mzFileByteLength || segmentTableEnd > mzFileByteLength)
            throw Error(sourceName, "places the FBOV descriptor table outside the MZ file");

        stream.Position = segmentTableFileOffset;
        var flags = new Dictionary<ushort, int>();
        var ascendingRanges = 0;
        var descendingRanges = 0;
        ushort minimumSegment = ushort.MaxValue;
        ushort maximumSegment = ushort.MinValue;
        Span<byte> descriptor = stackalloc byte[SegmentDescriptorLength];
        for (var index = 0; index < descriptorCount; index++)
        {
            ReadExactly(stream, descriptor, sourceName, $"FBOV descriptor {index}");
            var segment = BinaryPrimitives.ReadUInt16LittleEndian(descriptor);
            var maximumOffset = BinaryPrimitives.ReadUInt16LittleEndian(descriptor[2..]);
            var flag = BinaryPrimitives.ReadUInt16LittleEndian(descriptor[4..]);
            var minimumOffset = BinaryPrimitives.ReadUInt16LittleEndian(descriptor[6..]);
            minimumSegment = Math.Min(minimumSegment, segment);
            maximumSegment = Math.Max(maximumSegment, segment);
            if (maximumOffset >= minimumOffset) ascendingRanges++;
            else descendingRanges++;
            flags[flag] = flags.GetValueOrDefault(flag) + 1;
        }

        return new(stream.Length, mzFileByteLength, mzFileByteLength, declaredPayloadLength,
            segmentTableFileOffset, descriptorCount, segmentTableEnd, ascendingRanges,
            descendingRanges, minimumSegment, maximumSegment,
            flags.OrderBy(pair => pair.Key).Select(pair => new FbovOverlayFlagProfile(pair.Key,
                pair.Value)).ToArray());
    }

    private static void ReadExactly(Stream stream, Span<byte> destination, string sourceName,
        string field)
    {
        var totalRead = 0;
        while (totalRead < destination.Length)
        {
            var read = stream.Read(destination[totalRead..]);
            if (read == 0) throw Error(sourceName, $"is truncated while reading {field}");
            totalRead += read;
        }
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
