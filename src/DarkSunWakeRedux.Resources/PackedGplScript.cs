using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedGplScript(uint ResourceNumber, byte[] Bytecode)
{
    public const ushort FormatVersion = 2;
    public const int MaximumBytecodeBytes = 1024 * 1024;
    public const string GplTag = "GPL ";
    public const string MasTag = "MAS ";
    private const int HeaderBytes = 18;

    public string SourceTag { get; init; } = GplTag;

    public static PackedGplScript FromOwned(string sourceTag, uint resourceNumber,
        byte[] bytecode) => new(resourceNumber, bytecode)
    {
        SourceTag = ValidateSourceTag(sourceTag, "script")
    };

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("GPL script stream must be writable.", nameof(stream));
        Validate(SourceTag, ResourceNumber, Bytecode, "script");
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSGP"u8);
        writer.Write(FormatVersion);
        writer.Write(Encoding.ASCII.GetBytes(SourceTag));
        writer.Write(ResourceNumber);
        writer.Write(checked((uint)Bytecode.Length));
        writer.Write(Bytecode);
    }

    public static PackedGplScript Read(Stream stream, string sourceName = "GPL script")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes || stream.Length > HeaderBytes + MaximumBytecodeBytes)
            throw Error(sourceName, $"has invalid file length {stream.Length}");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (!bytes.AsSpan(0, 4).SequenceEqual("DSGP"u8))
            throw Error(sourceName, "has an invalid signature");
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4, 2));
        if (version != FormatVersion)
            throw Error(sourceName, $"uses unsupported format version {version}");
        var sourceTag = Encoding.ASCII.GetString(bytes, 6, 4);
        var number = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(10, 4));
        var length = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(14, 4));
        if (length is 0 or > MaximumBytecodeBytes || HeaderBytes + (ulong)length != (ulong)bytes.Length)
            throw Error(sourceName, $"declares invalid bytecode length {length}");
        var bytecode = bytes.AsSpan(HeaderBytes, checked((int)length)).ToArray();
        Validate(sourceTag, number, bytecode, sourceName);
        return new(number, bytecode) { SourceTag = sourceTag };
    }

    private static void Validate(string sourceTag, uint number, byte[]? bytecode,
        string sourceName)
    {
        _ = ValidateSourceTag(sourceTag, sourceName);
        if (number == 0) throw Error(sourceName, "has invalid resource number 0");
        if (bytecode is null || bytecode.Length is 0 or > MaximumBytecodeBytes)
            throw Error(sourceName, "has an invalid bytecode payload");
    }

    private static string ValidateSourceTag(string? sourceTag, string sourceName)
    {
        if (sourceTag is GplTag or MasTag) return sourceTag;
        throw Error(sourceName, "has unsupported source tag");
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
