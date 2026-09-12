using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedIndexedBitmapFont(
    IReadOnlyList<byte> CharacterMap,
    IReadOnlyList<IndexedFontGlyph> Glyphs)
{
    public const ushort FormatVersion = 1;
    public const long MaximumFileBytes = 8L * 1024 * 1024;
    private const int HeaderBytes = 10;

    public static PackedIndexedBitmapFont From(IndexedBitmapFont font) =>
        new(font.CharacterMap, font.Glyphs);

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite) throw new ArgumentException("Pack font stream must be writable.", nameof(stream));
        var height = Validate(CharacterMap, Glyphs, "pack font");
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSFT"u8);
        writer.Write(FormatVersion);
        writer.Write(checked((ushort)Glyphs.Count));
        writer.Write(checked((ushort)height));
        writer.Write(CharacterMap.ToArray());
        foreach (var glyph in Glyphs)
        {
            writer.Write(checked((ushort)glyph.Width));
            writer.Write(checked((uint)glyph.Pixels.Length));
            writer.Write(glyph.Pixels);
        }
    }

    public static PackedIndexedBitmapFont Read(Stream stream, string sourceName = "pack font")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes + IndexedBitmapFont.CharacterCount)
            throw Error(sourceName, "is shorter than the header and character map");
        if (stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"exceeds the {MaximumFileBytes}-byte safety limit");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (!bytes.AsSpan(0, 4).SequenceEqual("DSFT"u8))
            throw Error(sourceName, "has an invalid signature");
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4, 2));
        if (version != FormatVersion)
            throw Error(sourceName, $"uses unsupported format version {version}");
        var glyphCount = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(6, 2));
        if (glyphCount != IndexedBitmapFont.CharacterCount)
            throw Error(sourceName, $"declares {glyphCount} glyphs; expected {IndexedBitmapFont.CharacterCount}");
        var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(8, 2));
        if (height is 0 or > IndexedBitmapFont.MaximumGlyphHeight)
            throw Error(sourceName, $"declares invalid glyph height {height}");

        var position = HeaderBytes;
        var characterMap = bytes.AsSpan(position, IndexedBitmapFont.CharacterCount).ToArray();
        position += characterMap.Length;
        var glyphs = new IndexedFontGlyph[IndexedBitmapFont.CharacterCount];
        var totalPixels = 0;
        for (var index = 0; index < glyphs.Length; index++)
        {
            Ensure(bytes, position, 6, sourceName, $"glyph {index} header");
            var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(position, 2));
            var pixelCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(position + 2, 4));
            position += 6;
            if (width > IndexedBitmapFont.MaximumGlyphWidth)
                throw Error(sourceName, $"glyph {index} width {width} exceeds the safety limit");
            var expected = checked((uint)(width * height));
            if (pixelCount != expected)
                throw Error(sourceName, $"glyph {index} declares {pixelCount} pixels, expected {expected}");
            totalPixels = checked(totalPixels + (int)pixelCount);
            if (totalPixels > IndexedBitmapFont.MaximumPixels)
                throw Error(sourceName, "decoded glyph pixels exceed the safety limit");
            Ensure(bytes, position, checked((int)pixelCount), sourceName, $"glyph {index} pixels");
            glyphs[index] = new(width, height, bytes.AsSpan(position, (int)pixelCount).ToArray());
            position += (int)pixelCount;
        }
        if (position != bytes.Length) throw Error(sourceName, "contains trailing bytes");
        Validate(characterMap, glyphs, sourceName);
        return new(characterMap, glyphs);
    }

    private static int Validate(
        IReadOnlyList<byte> characterMap,
        IReadOnlyList<IndexedFontGlyph> glyphs,
        string sourceName)
    {
        if (characterMap.Count != IndexedBitmapFont.CharacterCount)
            throw Error(sourceName, $"must contain {IndexedBitmapFont.CharacterCount} character-map entries");
        if (glyphs.Count != IndexedBitmapFont.CharacterCount)
            throw Error(sourceName, $"must contain {IndexedBitmapFont.CharacterCount} glyphs");
        var height = glyphs[0].Height;
        if (height is <= 0 or > IndexedBitmapFont.MaximumGlyphHeight)
            throw Error(sourceName, $"contains invalid glyph height {height}");
        var totalPixels = 0;
        for (var index = 0; index < glyphs.Count; index++)
        {
            var glyph = glyphs[index];
            if (glyph.Height != height)
                throw Error(sourceName, $"glyph {index} height differs from the shared height");
            if (glyph.Width is < 0 or > IndexedBitmapFont.MaximumGlyphWidth)
                throw Error(sourceName, $"glyph {index} width {glyph.Width} exceeds the safety limit");
            if (glyph.Pixels is null || glyph.Pixels.Length != glyph.Width * glyph.Height)
                throw Error(sourceName, $"glyph {index} pixel length does not match its dimensions");
            totalPixels = checked(totalPixels + glyph.Pixels.Length);
            if (totalPixels > IndexedBitmapFont.MaximumPixels)
                throw Error(sourceName, "decoded glyph pixels exceed the safety limit");
        }
        return height;
    }

    private static void Ensure(byte[] bytes, int offset, int length, string sourceName, string field)
    {
        if (offset < 0 || length < 0 || (long)offset + length > bytes.Length)
            throw Error(sourceName, $"is truncated while reading {field}");
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
