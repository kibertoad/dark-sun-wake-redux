using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record UiChildReference(string Tag, uint ResourceNumber, short X, short Y);

public sealed record UiWindowResource(
    uint ResourceNumber,
    uint ImageResourceNumber,
    ushort Width,
    ushort Height,
    IReadOnlyList<UiChildReference> Children)
{
    public const int FixedSize = 261;
    public const int ChildRecordSize = 30;
    public const int MaximumChildren = 4_096;

    public static UiWindowResource Read(ReadOnlyMemory<byte> payload, string sourceName = "WIND resource")
    {
        var reader = new UiResourceReader(payload, sourceName);
        reader.RequireSignature("WIND");
        reader.RequireExactDeclaredSize();
        if (payload.Length < FixedSize)
            throw reader.Error($"is shorter than the {FixedSize}-byte fixed WIND record");

        var childBytes = payload.Length - FixedSize;
        if (childBytes % ChildRecordSize != 0)
            throw reader.Error($"has {childBytes} trailing bytes, which is not a whole {ChildRecordSize}-byte child record");
        var childCount = childBytes / ChildRecordSize;
        if (childCount > MaximumChildren)
            throw reader.Error($"declares {childCount} children, exceeding the safety limit");

        var children = new List<UiChildReference>(childCount);
        for (var index = 0; index < childCount; index++)
        {
            var offset = FixedSize + index * ChildRecordSize;
            var tag = reader.Tag(offset + 4, $"child {index} tag");
            children.Add(new(tag,
                reader.UInt32(offset + 8, $"child {index} resource number"),
                reader.Int16(offset + 12, $"child {index} x"),
                reader.Int16(offset + 14, $"child {index} y")));
        }

        var width = reader.UInt16(190, "width");
        var height = reader.UInt16(192, "height");
        reader.RequireDimensions(width, height);

        return new(reader.UInt32(8, "resource number"), reader.UInt32(58, "image resource number"),
            width, height, children);
    }
}

public sealed record UiButtonResource(
    uint ResourceNumber,
    ushort Width,
    ushort Height,
    uint ImageResourceNumber,
    ushort EventMask)
{
    public const int FixedSize = 110;
    public const int MaximumPayloadBytes = 1024 * 1024;

    public static UiButtonResource Read(ReadOnlyMemory<byte> payload, string sourceName = "BUTN resource")
    {
        var reader = new UiResourceReader(payload, sourceName);
        reader.RequireSignature("BUTN");
        reader.RequireExactDeclaredSize();
        if (payload.Length < FixedSize)
            throw reader.Error($"is shorter than the {FixedSize}-byte fixed BUTN record");
        if (payload.Length > MaximumPayloadBytes)
            throw reader.Error($"exceeds the {MaximumPayloadBytes}-byte safety limit");

        var resourceNumber = reader.UInt32(8, "resource number");
        var repeatedResourceNumber = reader.UInt32(90, "repeated resource number");
        if (repeatedResourceNumber != resourceNumber)
            throw reader.Error($"resource number {resourceNumber} conflicts with repeated value {repeatedResourceNumber}");

        var width = reader.UInt16(40, "width");
        var height = reader.UInt16(42, "height");
        reader.RequireDimensions(width, height);

        return new(resourceNumber, width, height, reader.UInt32(100, "image resource number"),
            reader.UInt16(88, "event mask"));
    }
}

public sealed record UiApplicationFrameResource(
    uint ResourceNumber,
    ushort Width,
    ushort Height,
    ushort EventMask)
{
    public const int RecordSize = 116;

    public static UiApplicationFrameResource Read(
        ReadOnlyMemory<byte> payload,
        string sourceName = "APFM resource")
    {
        var reader = new UiResourceReader(payload, sourceName);
        reader.RequireSignature("APFM");
        reader.RequireExactDeclaredSize();
        if (payload.Length != RecordSize)
            throw reader.Error($"has unsupported size {payload.Length}; expected {RecordSize}");
        var width = reader.UInt16(40, "width");
        var height = reader.UInt16(42, "height");
        reader.RequireDimensions(width, height);
        return new(reader.UInt32(8, "resource number"), width, height,
            reader.UInt16(88, "event mask"));
    }
}

public sealed record UiEditBoxResource(
    uint ResourceNumber,
    ushort Width,
    ushort Height,
    ushort EventMask)
{
    public const int RecordSize = 168;

    public static UiEditBoxResource Read(ReadOnlyMemory<byte> payload, string sourceName = "EBOX resource")
    {
        var reader = new UiResourceReader(payload, sourceName);
        reader.RequireSignature("EBOX");
        reader.RequireExactDeclaredSize();
        if (payload.Length != RecordSize)
            throw reader.Error($"has unsupported size {payload.Length}; expected {RecordSize}");

        var resourceNumber = reader.UInt32(8, "resource number");
        var repeatedResourceNumber = reader.UInt32(24, "repeated resource number");
        if (repeatedResourceNumber != resourceNumber)
            throw reader.Error($"resource number {resourceNumber} conflicts with repeated value {repeatedResourceNumber}");
        var width = reader.UInt16(34, "width");
        var height = reader.UInt16(36, "height");
        reader.RequireDimensions(width, height);
        return new(resourceNumber, width, height, reader.UInt16(150, "event mask"));
    }
}

internal sealed class UiResourceReader
{
    private readonly byte[] _bytes;
    private readonly string _sourceName;

    public UiResourceReader(ReadOnlyMemory<byte> payload, string sourceName)
    {
        _bytes = payload.ToArray();
        _sourceName = sourceName;
    }

    public void RequireSignature(string expected)
    {
        if (Tag(0, "signature") != expected)
            throw Error($"does not begin with the {expected} signature");
    }

    public void RequireExactDeclaredSize()
    {
        var declaredSize = UInt32(4, "declared size");
        if (declaredSize != _bytes.Length)
            throw Error($"declares size {declaredSize}, but contains {_bytes.Length} bytes");
    }

    public void RequireDimensions(ushort width, ushort height)
    {
        if (width is 0 or > IndexedImage.MaximumDimension || height is 0 or > IndexedImage.MaximumDimension)
            throw Error($"has invalid dimensions {width}x{height}");
    }

    public ushort UInt16(int offset, string field)
    {
        Ensure(offset, 2, field);
        return BinaryPrimitives.ReadUInt16LittleEndian(_bytes.AsSpan(offset, 2));
    }

    public short Int16(int offset, string field)
    {
        Ensure(offset, 2, field);
        return BinaryPrimitives.ReadInt16LittleEndian(_bytes.AsSpan(offset, 2));
    }

    public uint UInt32(int offset, string field)
    {
        Ensure(offset, 4, field);
        return BinaryPrimitives.ReadUInt32LittleEndian(_bytes.AsSpan(offset, 4));
    }

    public string Tag(int offset, string field)
    {
        Ensure(offset, 4, field);
        var bytes = _bytes.AsSpan(offset, 4);
        foreach (var value in bytes)
            if (value is < 0x20 or > 0x7e)
                throw Error($"contains a non-printable {field} at offset {offset}");
        return Encoding.ASCII.GetString(bytes);
    }

    public InvalidDataException Error(string message) => new($"{_sourceName}: {message}.");

    private void Ensure(int offset, int length, string field)
    {
        if (offset < 0 || length < 0 || (long)offset + length > _bytes.Length)
            throw Error($"is truncated while reading {field} at offset {offset}");
    }
}
