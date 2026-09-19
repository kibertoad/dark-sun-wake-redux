using System.Buffers.Binary;

namespace DarkSunWakeRedux.Resources;

/// <summary>
/// A bounded, lossless local-pack envelope for source bytes whose behavior has
/// not been established. The envelope deliberately records no inferred
/// semantics; provenance belongs to the asset-pack manifest.
/// </summary>
public sealed record PackedOpaquePayload(byte[] Bytes)
{
    public const ushort FormatVersion = 1;
    public const int HeaderLength = 10;
    public const int MaximumPayloadBytes = 128 * 1024 * 1024;
    private static ReadOnlySpan<byte> Magic => "DSOP"u8;

    public static PackedOpaquePayload From(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > MaximumPayloadBytes)
            throw new InvalidDataException($"Opaque payload exceeds the {MaximumPayloadBytes}-byte safety limit.");
        return new(bytes.ToArray());
    }

    /// <summary>Wraps a caller-owned buffer without an additional copy.</summary>
    public static PackedOpaquePayload FromOwned(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length > MaximumPayloadBytes)
            throw new InvalidDataException($"Opaque payload exceeds the {MaximumPayloadBytes}-byte safety limit.");
        return new(bytes);
    }

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite) throw new ArgumentException("Output stream must be writable.", nameof(stream));
        if (Bytes is null || Bytes.Length > MaximumPayloadBytes)
            throw new InvalidDataException($"Opaque payload exceeds the {MaximumPayloadBytes}-byte safety limit.");

        Span<byte> header = stackalloc byte[HeaderLength];
        Magic.CopyTo(header);
        BinaryPrimitives.WriteUInt16LittleEndian(header[4..], FormatVersion);
        BinaryPrimitives.WriteUInt32LittleEndian(header[6..], checked((uint)Bytes.Length));
        stream.Write(header);
        stream.Write(Bytes);
    }

    public static PackedOpaquePayload Read(Stream stream, string sourceName = "opaque payload")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderLength)
            throw Error(sourceName, $"is shorter than the {HeaderLength}-byte header");
        if (stream.Length > HeaderLength + MaximumPayloadBytes)
            throw Error(sourceName, $"exceeds the {MaximumPayloadBytes}-byte safety limit");

        Span<byte> header = stackalloc byte[HeaderLength];
        stream.Position = 0;
        stream.ReadExactly(header);
        if (!header[..4].SequenceEqual(Magic)) throw Error(sourceName, "does not begin with the DSOP signature");
        var version = BinaryPrimitives.ReadUInt16LittleEndian(header[4..]);
        if (version != FormatVersion) throw Error(sourceName, $"uses unsupported format version {version}");
        var length = BinaryPrimitives.ReadUInt32LittleEndian(header[6..]);
        if (length > MaximumPayloadBytes) throw Error(sourceName, "declares an oversized payload");
        if (stream.Length != HeaderLength + (long)length)
            throw Error(sourceName, "has a truncated payload or trailing data");
        var bytes = new byte[checked((int)length)];
        stream.ReadExactly(bytes);
        return new(bytes);
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
