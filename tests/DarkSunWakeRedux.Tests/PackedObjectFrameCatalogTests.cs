using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedObjectFrameCatalogTests
{
    [Fact]
    public void RoundTripsDefinitionsAndSharedImagesCanonically()
    {
        var source = Catalog();

        var first = Write(source);
        var decoded = Read(first);
        var second = Write(decoded);

        Assert.Equal(first, second);
        Assert.Equal([10U, 11U, 12U],
            decoded.Definitions.Select(item => item.ResourceNumber));
        Assert.Equal([5U, 6U], decoded.Images.Select(item => item.ResourceNumber));
        Assert.Equal((ushort)0x1234, decoded.Definitions[0].RawWord0);
        Assert.Equal((-4, 17),
            ((int)decoded.Definitions[0].XOffset, (int)decoded.Definitions[0].YOffset));
        Assert.Equal([5U, 5U, 6U],
            decoded.Definitions.Select(item => item.ImageResourceNumber));
        Assert.Equal(3, decoded.Images.Sum(item => item.Frames.Count));
        Assert.Equal([1, 2], decoded.Images[0].Frames[0].Pixels);
        Assert.Equal([255, 0], decoded.Images[0].Frames[0].Alpha);
    }

    [Fact]
    public void RejectsNonCanonicalOrDisconnectedCatalogsBeforeWriting()
    {
        var source = Catalog();
        Assert.Contains("definitions are duplicated or not in canonical order", WriteError(
            source with { Definitions = source.Definitions.Reverse().ToArray() }));
        Assert.Contains("images are duplicated or not in canonical order", WriteError(
            source with { Images = source.Images.Reverse().ToArray() }));
        Assert.Contains("references missing BMP #6", WriteError(
            source with { Images = source.Images.Take(1).ToArray() }));
        Assert.Contains("unreferenced BMP #6", WriteError(
            source with { Definitions = source.Definitions.Take(2).ToArray() }));
    }

    [Fact]
    public void RejectsMalformedFrameDataBeforeWriting()
    {
        var source = Catalog();
        var badFrame = source.Images[0].Frames[0] with { Alpha = [1, 0] };
        var badImage = source.Images[0] with { Frames = [badFrame] };

        Assert.Contains("non-binary alpha", WriteError(source with
        {
            Images = [badImage, source.Images[1]]
        }));
        Assert.Contains("pixel and alpha lengths differ", WriteError(source with
        {
            Images = [source.Images[0] with
            {
                Frames = [source.Images[0].Frames[0] with { Alpha = [255] }]
            }, source.Images[1]]
        }));
    }

    [Fact]
    public void RejectsMalformedHeadersTruncationAlphaAndTrailingData()
    {
        var valid = Write(SingleCatalog());
        var signature = valid.ToArray();
        signature[0] = (byte)'X';
        Assert.Contains("invalid signature", ReadError(signature));

        var version = valid.ToArray();
        version[4] = 2;
        Assert.Contains("unsupported format version", ReadError(version));

        var zeroDefinitions = valid.ToArray();
        Array.Clear(zeroDefinitions, 6, 4);
        Assert.Contains("invalid definition count", ReadError(zeroDefinitions));

        Assert.Contains("truncated", ReadError(valid[..^1]));

        var alpha = valid.ToArray();
        alpha[50] = 1;
        Assert.Contains("non-binary alpha", ReadError(alpha));

        Assert.Contains("trailing bytes", ReadError([.. valid, 0]));
    }

    private static PackedObjectFrameCatalog Catalog() => new(
    [
        new(10, 0x1234, -4, 17, 0x8000, 0xabcd, 256, 5),
        new(11, 2, 7, 63, 1, 2, 3, 5),
        new(12, 4, 1, -1, 5, 6, 7, 6)
    ],
    [
        new(5,
        [
            new IndexedImageFrame(2, 1, [1, 2], [255, 0]),
            new IndexedImageFrame(1, 1, [3], [255])
        ]),
        new(6, [new IndexedImageFrame(1, 2, [4, 5], [0, 255])])
    ]);

    private static PackedObjectFrameCatalog SingleCatalog() => new(
        [new PackedObjectFrameDefinition(10, 0, 0, 0, 0, 0, 0, 5)],
        [new PackedObjectImage(5, [new IndexedImageFrame(2, 1, [1, 2], [255, 0])])]);

    private static byte[] Write(PackedObjectFrameCatalog catalog)
    {
        using var stream = new MemoryStream();
        catalog.Write(stream);
        return stream.ToArray();
    }

    private static PackedObjectFrameCatalog Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return PackedObjectFrameCatalog.Read(stream, "objects.dsob");
    }

    private static string ReadError(byte[] bytes) =>
        Assert.Throws<InvalidDataException>(() => Read(bytes)).Message;

    private static string WriteError(PackedObjectFrameCatalog catalog) =>
        Assert.Throws<InvalidDataException>(() => Write(catalog)).Message;
}
