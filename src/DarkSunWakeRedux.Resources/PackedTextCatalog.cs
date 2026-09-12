using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedTextCatalog(IReadOnlyDictionary<uint, IReadOnlyList<string>> Resources)
{
    public const ushort FormatVersion = 1;
    public const int MaximumResources = 65_536;
    public const long MaximumFileBytes = 8L * 1024 * 1024;
    private const int HeaderBytes = 10;

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite) throw new ArgumentException("Text catalog stream must be writable.", nameof(stream));
        Validate(Resources, "text catalog");
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSTX"u8);
        writer.Write(FormatVersion);
        writer.Write(checked((uint)Resources.Count));
        foreach (var (number, lines) in Resources.OrderBy(item => item.Key))
        {
            writer.Write(number);
            writer.Write(checked((uint)lines.Count));
            foreach (var line in lines)
            {
                var bytes = Encoding.ASCII.GetBytes(line);
                writer.Write(checked((ushort)bytes.Length));
                writer.Write(bytes);
            }
        }
    }

    public static PackedTextCatalog Read(Stream stream, string sourceName = "text catalog")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes || stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"has invalid file length {stream.Length}");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (!bytes.AsSpan(0, 4).SequenceEqual("DSTX"u8)) throw Error(sourceName, "has an invalid signature");
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4, 2));
        if (version != FormatVersion) throw Error(sourceName, $"uses unsupported format version {version}");
        var count = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(6, 4));
        if (count > MaximumResources) throw Error(sourceName, $"declares too many resources: {count}");
        var position = HeaderBytes;
        var resources = new Dictionary<uint, IReadOnlyList<string>>();
        for (var resourceIndex = 0; resourceIndex < count; resourceIndex++)
        {
            Ensure(bytes, position, 8, sourceName, $"resource {resourceIndex} header");
            var number = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(position, 4));
            var lineCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(position + 4, 4));
            position += 8;
            if (lineCount > GffTextResource.MaximumLines)
                throw Error(sourceName, $"resource {number} declares too many lines: {lineCount}");
            if (resources.ContainsKey(number)) throw Error(sourceName, $"duplicates resource {number}");
            var lines = new string[lineCount];
            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                Ensure(bytes, position, 2, sourceName, $"resource {number} line {lineIndex} length");
                var length = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(position, 2));
                position += 2;
                if (length > GffTextResource.MaximumLineBytes)
                    throw Error(sourceName, $"resource {number} line {lineIndex} is too long");
                Ensure(bytes, position, length, sourceName, $"resource {number} line {lineIndex}");
                var line = bytes.AsSpan(position, length);
                if (!IsPrintableAscii(line))
                    throw Error(sourceName, $"resource {number} line {lineIndex} contains unsupported bytes");
                lines[lineIndex] = Encoding.ASCII.GetString(line);
                position += length;
            }
            resources.Add(number, lines);
        }
        if (position != bytes.Length) throw Error(sourceName, "contains trailing bytes");
        return new(resources);
    }

    private static void Validate(
        IReadOnlyDictionary<uint, IReadOnlyList<string>> resources,
        string sourceName)
    {
        if (resources.Count > MaximumResources) throw Error(sourceName, "contains too many resources");
        foreach (var (number, lines) in resources)
        {
            if (lines is null || lines.Count > GffTextResource.MaximumLines)
                throw Error(sourceName, $"resource {number} contains too many lines");
            foreach (var line in lines)
            {
                if (line is null || line.Length > GffTextResource.MaximumLineBytes ||
                    line.Any(value => value is < ' ' or > '~'))
                    throw Error(sourceName, $"resource {number} contains an invalid line");
            }
        }
    }

    private static bool IsPrintableAscii(ReadOnlySpan<byte> bytes)
    {
        foreach (var value in bytes)
            if (value is < 32 or > 126) return false;
        return true;
    }

    private static void Ensure(byte[] bytes, int offset, int length, string sourceName, string field)
    {
        if (offset < 0 || length < 0 || (long)offset + length > bytes.Length)
            throw Error(sourceName, $"is truncated while reading {field}");
    }

    private static InvalidDataException Error(string sourceName, string message) => new($"{sourceName}: {message}.");
}
