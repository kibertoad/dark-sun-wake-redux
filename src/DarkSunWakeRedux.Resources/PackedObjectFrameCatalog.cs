using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedObjectFrameDefinition(
    uint ResourceNumber,
    ushort RawWord0,
    short XOffset,
    short YOffset,
    ushort RawWord6,
    ushort RawWord8,
    ushort RawWord10,
    uint ImageResourceNumber);

public sealed record PackedObjectImage(
    uint ResourceNumber,
    IReadOnlyList<IndexedImageFrame> Frames);

public sealed record PackedObjectFrameCatalog(
    IReadOnlyList<PackedObjectFrameDefinition> Definitions,
    IReadOnlyList<PackedObjectImage> Images)
{
    public const ushort FormatVersion = 1;
    public const long MaximumFileBytes = 64L * 1024 * 1024;
    public const int MaximumImages = GffObjectFrameCatalog.MaximumEntries;
    public const int MaximumTotalFrames = 65_536;
    public const long MaximumTotalPixels = 32L * 1024 * 1024;
    private const int HeaderBytes = 14;
    private const int DefinitionBytes = 20;
    private const int ImageHeaderBytes = 6;
    private const int FrameHeaderBytes = 8;

    public static PackedObjectFrameCatalog From(GffObjectFrameCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var definitions = catalog.Entries.OrderBy(entry => entry.ResourceNumber)
            .Select(entry => new PackedObjectFrameDefinition(
                entry.ResourceNumber, entry.RawWord0, entry.XOffset, entry.YOffset,
                entry.RawWord6, entry.RawWord8, entry.RawWord10, entry.ImageResourceNumber))
            .ToArray();
        var images = catalog.Entries.GroupBy(entry => entry.ImageResourceNumber)
            .OrderBy(group => group.Key)
            .Select(group => new PackedObjectImage(group.Key, group.First().Image.Frames))
            .ToArray();
        return new(definitions, images);
    }

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("Packed object-catalog stream must be writable.", nameof(stream));
        Validate(this, "packed object catalog");
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSOB"u8);
        writer.Write(FormatVersion);
        writer.Write(checked((uint)Definitions.Count));
        writer.Write(checked((uint)Images.Count));
        foreach (var definition in Definitions)
        {
            writer.Write(definition.ResourceNumber);
            writer.Write(definition.RawWord0);
            writer.Write(definition.XOffset);
            writer.Write(definition.YOffset);
            writer.Write(definition.RawWord6);
            writer.Write(definition.RawWord8);
            writer.Write(definition.RawWord10);
            writer.Write(definition.ImageResourceNumber);
        }
        foreach (var image in Images)
        {
            writer.Write(image.ResourceNumber);
            writer.Write(checked((ushort)image.Frames.Count));
            foreach (var frame in image.Frames)
            {
                writer.Write(checked((ushort)frame.Width));
                writer.Write(checked((ushort)frame.Height));
                writer.Write(checked((uint)frame.Pixels.Length));
                writer.Write(frame.Pixels);
                writer.Write(frame.Alpha);
            }
        }
    }

    public static PackedObjectFrameCatalog Read(
        Stream stream,
        string sourceName = "packed object catalog")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes || stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"has invalid file length {stream.Length}");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (!bytes.AsSpan(0, 4).SequenceEqual("DSOB"u8))
            throw Error(sourceName, "has an invalid signature");
        var version = UInt16(bytes, 4);
        if (version != FormatVersion)
            throw Error(sourceName, $"uses unsupported format version {version}");
        var definitionCount = UInt32(bytes, 6);
        var imageCount = UInt32(bytes, 10);
        if (definitionCount is 0 or > GffObjectFrameCatalog.MaximumEntries)
            throw Error(sourceName, $"declares invalid definition count {definitionCount}");
        if (imageCount is 0 or > MaximumImages)
            throw Error(sourceName, $"declares invalid image count {imageCount}");
        var minimumLength = HeaderBytes + (long)definitionCount * DefinitionBytes +
            (long)imageCount * ImageHeaderBytes;
        if (minimumLength > bytes.Length)
            throw Error(sourceName, $"minimum declared content length {minimumLength} exceeds the file");

        var position = HeaderBytes;
        var definitions = new PackedObjectFrameDefinition[checked((int)definitionCount)];
        uint? previousDefinition = null;
        for (var index = 0; index < definitions.Length; index++)
        {
            var resourceNumber = UInt32(bytes, position);
            if (previousDefinition is not null && resourceNumber <= previousDefinition)
                throw Error(sourceName, "definitions are duplicated or not in canonical order");
            definitions[index] = new(resourceNumber,
                UInt16(bytes, position + 4), Int16(bytes, position + 6),
                Int16(bytes, position + 8), UInt16(bytes, position + 10),
                UInt16(bytes, position + 12), UInt16(bytes, position + 14),
                UInt32(bytes, position + 16));
            previousDefinition = resourceNumber;
            position += DefinitionBytes;
        }

        var images = new PackedObjectImage[checked((int)imageCount)];
        uint? previousImage = null;
        var totalFrames = 0;
        var totalPixels = 0L;
        for (var imageIndex = 0; imageIndex < images.Length; imageIndex++)
        {
            Ensure(bytes, position, ImageHeaderBytes, sourceName, $"image {imageIndex} header");
            var resourceNumber = UInt32(bytes, position);
            var frameCount = UInt16(bytes, position + 4);
            position += ImageHeaderBytes;
            if (previousImage is not null && resourceNumber <= previousImage)
                throw Error(sourceName, "images are duplicated or not in canonical order");
            if (frameCount is 0 or > IndexedImage.MaximumFrameCount)
                throw Error(sourceName, $"BMP #{resourceNumber} declares invalid frame count {frameCount}");
            totalFrames = checked(totalFrames + frameCount);
            if (totalFrames > MaximumTotalFrames)
                throw Error(sourceName, $"declares more than {MaximumTotalFrames} total frames");
            var frames = new IndexedImageFrame[frameCount];
            for (var frameIndex = 0; frameIndex < frames.Length; frameIndex++)
            {
                Ensure(bytes, position, FrameHeaderBytes, sourceName,
                    $"BMP #{resourceNumber} frame {frameIndex} header");
                var width = UInt16(bytes, position);
                var height = UInt16(bytes, position + 2);
                var pixelCount = UInt32(bytes, position + 4);
                position += FrameHeaderBytes;
                ValidateDimensions(width, height, pixelCount, sourceName, resourceNumber, frameIndex);
                totalPixels = checked(totalPixels + pixelCount);
                if (totalPixels > MaximumTotalPixels)
                    throw Error(sourceName, $"exceeds the {MaximumTotalPixels}-pixel catalog limit");
                var count = checked((int)pixelCount);
                Ensure(bytes, position, checked(count * 2), sourceName,
                    $"BMP #{resourceNumber} frame {frameIndex} pixels and alpha");
                var pixels = bytes.AsSpan(position, count).ToArray();
                position += count;
                var alpha = bytes.AsSpan(position, count).ToArray();
                position += count;
                if (alpha.Any(value => value is not 0 and not 255))
                    throw Error(sourceName,
                        $"BMP #{resourceNumber} frame {frameIndex} contains non-binary alpha");
                frames[frameIndex] = new(width, height, pixels, alpha);
            }
            images[imageIndex] = new(resourceNumber, frames);
            previousImage = resourceNumber;
        }
        if (position != bytes.Length)
            throw Error(sourceName, "contains trailing bytes");
        var result = new PackedObjectFrameCatalog(definitions, images);
        Validate(result, sourceName);
        return result;
    }

    private static void Validate(PackedObjectFrameCatalog catalog, string sourceName)
    {
        if (catalog.Definitions is null || catalog.Definitions.Count is 0 or >
            GffObjectFrameCatalog.MaximumEntries)
            throw Error(sourceName, "has an invalid definition count");
        if (catalog.Images is null || catalog.Images.Count is 0 or > MaximumImages)
            throw Error(sourceName, "has an invalid image count");
        EnsureCanonical(catalog.Definitions.Select(item => item.ResourceNumber),
            sourceName, "definitions");
        EnsureCanonical(catalog.Images.Select(item => item.ResourceNumber), sourceName, "images");
        var imageNumbers = catalog.Images.Select(item => item.ResourceNumber).ToHashSet();
        foreach (var reference in catalog.Definitions.Select(item => item.ImageResourceNumber))
            if (!imageNumbers.Contains(reference))
                throw Error(sourceName, $"definition references missing BMP #{reference}");
        var referencedImages = catalog.Definitions.Select(item => item.ImageResourceNumber).ToHashSet();
        foreach (var number in imageNumbers)
            if (!referencedImages.Contains(number))
                throw Error(sourceName, $"contains unreferenced BMP #{number}");

        var totalFrames = 0;
        var totalPixels = 0L;
        foreach (var image in catalog.Images)
        {
            if (image.Frames is null || image.Frames.Count is 0 or > IndexedImage.MaximumFrameCount)
                throw Error(sourceName, $"BMP #{image.ResourceNumber} has an invalid frame count");
            totalFrames = checked(totalFrames + image.Frames.Count);
            if (totalFrames > MaximumTotalFrames)
                throw Error(sourceName, $"contains more than {MaximumTotalFrames} total frames");
            for (var index = 0; index < image.Frames.Count; index++)
            {
                var frame = image.Frames[index];
                if (frame is null || frame.Pixels is null || frame.Alpha is null)
                    throw Error(sourceName, $"BMP #{image.ResourceNumber} frame {index} is missing data");
                ValidateDimensions(frame.Width, frame.Height, checked((uint)frame.Pixels.Length),
                    sourceName, image.ResourceNumber, index);
                if (frame.Alpha.Length != frame.Pixels.Length)
                    throw Error(sourceName,
                        $"BMP #{image.ResourceNumber} frame {index} pixel and alpha lengths differ");
                if (frame.Alpha.Any(value => value is not 0 and not 255))
                    throw Error(sourceName,
                        $"BMP #{image.ResourceNumber} frame {index} contains non-binary alpha");
                totalPixels = checked(totalPixels + frame.Pixels.Length);
                if (totalPixels > MaximumTotalPixels)
                    throw Error(sourceName, $"exceeds the {MaximumTotalPixels}-pixel catalog limit");
            }
        }
    }

    private static void EnsureCanonical(IEnumerable<uint> numbers, string sourceName, string family)
    {
        uint? previous = null;
        foreach (var number in numbers)
        {
            if (previous is not null && number <= previous)
                throw Error(sourceName, $"{family} are duplicated or not in canonical order");
            previous = number;
        }
    }

    private static void ValidateDimensions(
        int width, int height, uint pixelCount, string sourceName, uint imageNumber, int frameIndex)
    {
        if (width is <= 0 or > IndexedImage.MaximumDimension ||
            height is <= 0 or > IndexedImage.MaximumDimension ||
            (long)width * height > IndexedImage.MaximumPixelsPerFrame)
            throw Error(sourceName,
                $"BMP #{imageNumber} frame {frameIndex} has invalid dimensions {width}x{height}");
        if (pixelCount != checked((uint)(width * height)))
            throw Error(sourceName,
                $"BMP #{imageNumber} frame {frameIndex} declares invalid pixel count {pixelCount}");
    }

    private static void Ensure(
        byte[] bytes, int offset, int length, string sourceName, string field)
    {
        if (offset < 0 || length < 0 || (long)offset + length > bytes.Length)
            throw Error(sourceName, $"is truncated while reading {field}");
    }

    private static short Int16(byte[] bytes, int offset) =>
        BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(offset, 2));

    private static ushort UInt16(byte[] bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));

    private static uint UInt32(byte[] bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
