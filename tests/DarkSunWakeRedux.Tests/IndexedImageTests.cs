using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class IndexedImageTests
{
    [Fact]
    public void DecodesSparseRowRuns()
    {
        using var frame = new MemoryStream();
        using (var writer = new BinaryWriter(frame, Encoding.ASCII, leaveOpen: true))
        {
            writer.Write((ushort)4);
            writer.Write((ushort)2);
            writer.Write((byte)0);
            WriteRun(writer, 0, 0x80, 4, [6, 1, 2, 3, 4]);
            writer.Write((byte)1);
            WriteRun(writer, 1, 0x80, 2, [3, 9]);
        }

        var image = IndexedImage.Read(Wrap(frame.ToArray()), "row-image");
        var decoded = Assert.Single(image.Frames);

        Assert.Equal([1, 2, 3, 4, 0, 9, 9, 0], decoded.Pixels);
        Assert.Equal([255, 255, 255, 255, 0, 255, 255, 0], decoded.Alpha);
    }

    [Fact]
    public void DecodesPlanSymbolsAndTransparency()
    {
        var frame = PlanarFrame("PLAN", 2, 2, 2, [0, 5, 6, 7], [0x6c]);

        var decoded = Assert.Single(IndexedImage.Read(Wrap(frame), "plan-image").Frames);

        Assert.Equal([5, 6, 7, 0], decoded.Pixels);
        Assert.Equal([255, 255, 255, 0], decoded.Alpha);
    }

    [Fact]
    public void DecodesPlnrRepeatedSymbols()
    {
        var frame = PlanarFrame("PLNR", 5, 1, 2, [0, 9, 10, 11], [0x48]);

        var decoded = Assert.Single(IndexedImage.Read(Wrap(frame), "plnr-image").Frames);

        Assert.Equal([9, 9, 9, 9, 9], decoded.Pixels);
        Assert.All(decoded.Alpha, value => Assert.Equal(255, value));
    }

    [Fact]
    public void RejectsDeclaredSizeMismatch()
    {
        var payload = Wrap(PlanarFrame("PLAN", 1, 1, 1, [0, 1], [0x80]));
        payload[0]++;

        var exception = Assert.Throws<InvalidDataException>(() => IndexedImage.Read(payload, "wrong-size"));

        Assert.Contains("declares size", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsTruncatedPlanarBitstream()
    {
        var payload = Wrap(PlanarFrame("PLAN", 2, 2, 2, [0, 5, 6, 7], [0x6c]));
        Array.Resize(ref payload, payload.Length - 1);
        BitConverter.GetBytes((uint)payload.Length).CopyTo(payload, 0);

        var exception = Assert.Throws<InvalidDataException>(() => IndexedImage.Read(payload, "truncated-plan"));

        Assert.Contains("bitstream is truncated", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsRowRunBeyondWidth()
    {
        using var frame = new MemoryStream();
        using (var writer = new BinaryWriter(frame, Encoding.ASCII, leaveOpen: true))
        {
            writer.Write((ushort)4);
            writer.Write((ushort)1);
            writer.Write((byte)0);
            WriteRun(writer, 3, 0x80, 2, [3, 9]);
        }

        var exception = Assert.Throws<InvalidDataException>(() =>
            IndexedImage.Read(Wrap(frame.ToArray()), "wide-run"));

        Assert.Contains("run exceeds width", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MarkerWithoutKnownPlanarTagIsAnEmptyRowFrame()
    {
        var frame = PlanarFrame("NOPE", 1, 1, 1, [0, 1], [0x80]);

        var decoded = Assert.Single(IndexedImage.Read(Wrap(frame), "empty-row-frame").Frames);

        Assert.Equal([0], decoded.Pixels);
        Assert.Equal([0], decoded.Alpha);
    }

    private static byte[] PlanarFrame(
        string tag, ushort width, ushort height, byte bits, byte[] dictionary, byte[] codes)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(width);
        writer.Write(height);
        writer.Write((byte)0xff);
        writer.Write(Encoding.ASCII.GetBytes(tag));
        writer.Write(bits);
        writer.Write(dictionary);
        writer.Write(codes);
        return stream.ToArray();
    }

    private static byte[] Wrap(byte[] frame)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write((uint)(10 + frame.Length));
        writer.Write((ushort)1);
        writer.Write(10U);
        writer.Write(frame);
        return stream.ToArray();
    }

    private static void WriteRun(
        BinaryWriter writer, byte startX, byte flags, byte decodedLength, byte[] encoded)
    {
        writer.Write(startX);
        writer.Write(flags);
        writer.Write(decodedLength);
        writer.Write((byte)encoded.Length);
        writer.Write(encoded);
    }
}
