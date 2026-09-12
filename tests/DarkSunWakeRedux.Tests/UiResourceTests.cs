using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class UiResourceTests
{
    [Fact]
    public void ReadsWindowGeometryAndChildReferences()
    {
        var bytes = Window(19500, 320, 200, 19004, ("BUTN", 19300U, (short)94, (short)70));

        var window = UiWindowResource.Read(bytes, "synthetic WIND #19500");

        Assert.Equal(19500U, window.ResourceNumber);
        Assert.Equal(19004U, window.ImageResourceNumber);
        Assert.Equal((ushort)320, window.Width);
        Assert.Equal((ushort)200, window.Height);
        var child = Assert.Single(window.Children);
        Assert.Equal(new UiChildReference("BUTN", 19300, 94, 70), child);
    }

    [Fact]
    public void ReadsButtonGeometryAndImageReference()
    {
        var button = UiButtonResource.Read(Button(19300, 127, 12, 19111), "synthetic BUTN #19300");

        Assert.Equal(new UiButtonResource(19300, 127, 12, 19111), button);
    }

    [Fact]
    public void AcceptsBoundedUninterpretedButtonTail()
    {
        var bytes = Button(19300, 127, 12, 19111);
        Array.Resize(ref bytes, UiButtonResource.FixedSize + 30);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);

        var button = UiButtonResource.Read(bytes, "extended.butn");

        Assert.Equal(19111U, button.ImageResourceNumber);
    }

    [Fact]
    public void RejectsWindowPartialChildRecord()
    {
        var bytes = Window(1, 320, 200, 0, ("BUTN", 2U, (short)0, (short)0));
        Array.Resize(ref bytes, bytes.Length - 1);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);

        var exception = Assert.Throws<InvalidDataException>(() => UiWindowResource.Read(bytes, "partial.wind"));

        Assert.Contains("whole 30-byte child", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsNonPrintableChildTag()
    {
        var bytes = Window(1, 320, 200, 0, ("BUTN", 2U, (short)0, (short)0));
        bytes[UiWindowResource.FixedSize + 4] = 0;

        var exception = Assert.Throws<InvalidDataException>(() => UiWindowResource.Read(bytes, "bad-tag.wind"));

        Assert.Contains("non-printable", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsConflictingButtonResourceNumbers()
    {
        var bytes = Button(19300, 127, 12, 19111);
        BitConverter.GetBytes(19301U).CopyTo(bytes, 90);

        var exception = Assert.Throws<InvalidDataException>(() => UiButtonResource.Read(bytes, "conflict.butn"));

        Assert.Contains("conflicts", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsBadSignatureAndDeclaredSize()
    {
        var window = Window(1, 320, 200, 0);
        window[0] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => UiWindowResource.Read(window, "signature.wind"));

        var button = Button(1, 10, 10, 2);
        BitConverter.GetBytes(109U).CopyTo(button, 4);
        Assert.Throws<InvalidDataException>(() => UiButtonResource.Read(button, "size.butn"));
    }

    [Fact]
    public void RejectsZeroButtonDimension()
    {
        var bytes = Button(1, 0, 12, 2);

        var exception = Assert.Throws<InvalidDataException>(() => UiButtonResource.Read(bytes, "zero.butn"));

        Assert.Contains("invalid dimensions", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsZeroWindowDimension()
    {
        var bytes = Window(1, 0, 200, 0);

        var exception = Assert.Throws<InvalidDataException>(() => UiWindowResource.Read(bytes, "zero.wind"));

        Assert.Contains("invalid dimensions", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadsApplicationFrameGeometry()
    {
        var bytes = FixedRecord("APFM", UiApplicationFrameResource.RecordSize, 19200, 320, 200);
        BitConverter.GetBytes((ushort)494).CopyTo(bytes, 88);
        var frame = UiApplicationFrameResource.Read(
            bytes,
            "synthetic APFM #19200");

        Assert.Equal(new UiApplicationFrameResource(19200, 320, 200, 494), frame);
    }

    [Fact]
    public void ReadsEditBoxGeometryAndRepeatedIdentity()
    {
        var bytes = FixedRecord("EBOX", UiEditBoxResource.RecordSize, 4003, 95, 8);
        BitConverter.GetBytes(4003U).CopyTo(bytes, 24);
        BitConverter.GetBytes((ushort)95).CopyTo(bytes, 34);
        BitConverter.GetBytes((ushort)8).CopyTo(bytes, 36);

        var editBox = UiEditBoxResource.Read(bytes, "synthetic EBOX #4003");

        Assert.Equal(new UiEditBoxResource(4003, 95, 8), editBox);
    }

    [Fact]
    public void RejectsMalformedApplicationFrameAndEditBox()
    {
        var shortFrame = FixedRecord("APFM", UiApplicationFrameResource.RecordSize - 1, 1, 10, 10);
        Assert.Throws<InvalidDataException>(() => UiApplicationFrameResource.Read(shortFrame, "short.apfm"));

        var editBox = FixedRecord("EBOX", UiEditBoxResource.RecordSize, 4, 10, 10);
        BitConverter.GetBytes(5U).CopyTo(editBox, 24);
        var exception = Assert.Throws<InvalidDataException>(() => UiEditBoxResource.Read(editBox, "identity.ebox"));
        Assert.Contains("conflicts", exception.Message, StringComparison.Ordinal);
    }

    private static byte[] Window(uint number, ushort width, ushort height, uint imageNumber,
        params (string Tag, uint Number, short X, short Y)[] children)
    {
        var bytes = new byte[UiWindowResource.FixedSize + children.Length * UiWindowResource.ChildRecordSize];
        Encoding.ASCII.GetBytes("WIND").CopyTo(bytes, 0);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);
        BitConverter.GetBytes(number).CopyTo(bytes, 8);
        BitConverter.GetBytes(imageNumber).CopyTo(bytes, 58);
        BitConverter.GetBytes(width).CopyTo(bytes, 190);
        BitConverter.GetBytes(height).CopyTo(bytes, 192);
        for (var index = 0; index < children.Length; index++)
        {
            var offset = UiWindowResource.FixedSize + index * UiWindowResource.ChildRecordSize;
            Encoding.ASCII.GetBytes(children[index].Tag).CopyTo(bytes, offset + 4);
            BitConverter.GetBytes(children[index].Number).CopyTo(bytes, offset + 8);
            BitConverter.GetBytes(children[index].X).CopyTo(bytes, offset + 12);
            BitConverter.GetBytes(children[index].Y).CopyTo(bytes, offset + 14);
        }
        return bytes;
    }

    private static byte[] Button(uint number, ushort width, ushort height, uint imageNumber)
    {
        var bytes = new byte[UiButtonResource.FixedSize];
        Encoding.ASCII.GetBytes("BUTN").CopyTo(bytes, 0);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);
        BitConverter.GetBytes(number).CopyTo(bytes, 8);
        BitConverter.GetBytes(width).CopyTo(bytes, 40);
        BitConverter.GetBytes(height).CopyTo(bytes, 42);
        BitConverter.GetBytes(number).CopyTo(bytes, 90);
        BitConverter.GetBytes(imageNumber).CopyTo(bytes, 100);
        return bytes;
    }

    private static byte[] FixedRecord(string tag, int size, uint number, ushort width, ushort height)
    {
        var bytes = new byte[size];
        Encoding.ASCII.GetBytes(tag).CopyTo(bytes, 0);
        BitConverter.GetBytes((uint)size).CopyTo(bytes, 4);
        BitConverter.GetBytes(number).CopyTo(bytes, 8);
        BitConverter.GetBytes(width).CopyTo(bytes, 40);
        BitConverter.GetBytes(height).CopyTo(bytes, 42);
        return bytes;
    }
}
