using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedRegion(
    uint ResourceNumber,
    string Name,
    IReadOnlyList<Rgb24> Palette,
    byte[] TileMap,
    byte[] GeometryMap,
    IReadOnlyDictionary<byte, IndexedImageFrame> Tiles,
    IReadOnlyList<RegionEntityReference> Entities)
{
    public const ushort FormatVersion = 1;
    public const long MaximumFileBytes = 2L * 1024 * 1024;
    private const int HeaderBytes = 24;
    private const int PaletteBytes = IndexedPalette.ColorCount * 3;
    private const int TilePixelCount = GffRegion.TilePixelSize * GffRegion.TilePixelSize;
    private const int PackedTileBytes = 1 + TilePixelCount * 2;

    public static PackedRegion From(GffRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        return new(region.ResourceNumber, region.Name, region.Palette.Colors,
            region.TileMap, region.GeometryMap, region.Tiles, region.Entities);
    }

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("Packed-region stream must be writable.", nameof(stream));
        Validate(this, "packed region");
        var nameBytes = Encoding.ASCII.GetBytes(Name);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSRG"u8);
        writer.Write(FormatVersion);
        writer.Write(ResourceNumber);
        writer.Write((ushort)GffRegion.TileColumns);
        writer.Write((ushort)GffRegion.TileRows);
        writer.Write((ushort)GffRegion.TilePixelSize);
        writer.Write(checked((ushort)nameBytes.Length));
        writer.Write(checked((ushort)Tiles.Count));
        writer.Write(checked((uint)Entities.Count));
        writer.Write(nameBytes);
        foreach (var color in Palette)
        {
            writer.Write(color.Red);
            writer.Write(color.Green);
            writer.Write(color.Blue);
        }
        writer.Write(TileMap);
        writer.Write(GeometryMap);
        foreach (var (number, tile) in Tiles.OrderBy(item => item.Key))
        {
            writer.Write(number);
            writer.Write(tile.Pixels);
            writer.Write(tile.Alpha);
        }
        foreach (var entity in Entities)
        {
            writer.Write(entity.X);
            writer.Write(entity.Y);
            writer.Write(entity.VerticalOffset);
            writer.Write(entity.Flags);
            writer.Write(entity.SignedObjectResourceNumber);
        }
    }

    public static PackedRegion Read(Stream stream, string sourceName = "packed region")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes + PaletteBytes + GffRegion.MapByteCount * 2L ||
            stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"has invalid file length {stream.Length}");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (!bytes.AsSpan(0, 4).SequenceEqual("DSRG"u8))
            throw Error(sourceName, "has an invalid signature");
        var version = UInt16(bytes, 4);
        if (version != FormatVersion)
            throw Error(sourceName, $"uses unsupported format version {version}");
        var resourceNumber = UInt32(bytes, 6);
        if (UInt16(bytes, 10) != GffRegion.TileColumns ||
            UInt16(bytes, 12) != GffRegion.TileRows ||
            UInt16(bytes, 14) != GffRegion.TilePixelSize)
            throw Error(sourceName, "declares unsupported region dimensions");
        var nameLength = UInt16(bytes, 16);
        var tileCount = UInt16(bytes, 18);
        var entityCount = UInt32(bytes, 20);
        if (nameLength is 0 or >= GffRegion.MaximumNameBytes)
            throw Error(sourceName, $"declares invalid name length {nameLength}");
        if (tileCount is 0 or > byte.MaxValue + 1)
            throw Error(sourceName, $"declares invalid tile count {tileCount}");
        if (entityCount > GffRegion.MaximumEntityCount)
            throw Error(sourceName, $"declares invalid entity count {entityCount}");

        var expectedLength = HeaderBytes + (long)nameLength + PaletteBytes +
            GffRegion.MapByteCount * 2L + tileCount * (long)PackedTileBytes +
            entityCount * GffRegion.EntityRecordSize;
        if (expectedLength != bytes.Length)
            throw Error(sourceName, $"declares content length {expectedLength}, found {bytes.Length}");
        var position = HeaderBytes;
        var nameBytes = bytes.AsSpan(position, nameLength);
        if (!IsPrintableAscii(nameBytes))
            throw Error(sourceName, "name contains unsupported bytes");
        var name = Encoding.ASCII.GetString(nameBytes);
        position += nameLength;
        var palette = new Rgb24[IndexedPalette.ColorCount];
        for (var index = 0; index < palette.Length; index++)
        {
            palette[index] = new(bytes[position], bytes[position + 1], bytes[position + 2]);
            position += 3;
        }
        var tileMap = bytes.AsSpan(position, GffRegion.MapByteCount).ToArray();
        position += GffRegion.MapByteCount;
        var geometryMap = bytes.AsSpan(position, GffRegion.MapByteCount).ToArray();
        position += GffRegion.MapByteCount;

        var tiles = new Dictionary<byte, IndexedImageFrame>();
        int? previousTile = null;
        for (var index = 0; index < tileCount; index++)
        {
            var number = bytes[position++];
            if (previousTile is not null && number <= previousTile)
                throw Error(sourceName, "tile resources are duplicated or not in canonical order");
            var pixels = bytes.AsSpan(position, TilePixelCount).ToArray();
            position += TilePixelCount;
            var alpha = bytes.AsSpan(position, TilePixelCount).ToArray();
            position += TilePixelCount;
            if (alpha.Any(value => value is not 0 and not 255))
                throw Error(sourceName, $"TILE #{number} contains non-binary alpha");
            tiles.Add(number,
                new IndexedImageFrame(GffRegion.TilePixelSize, GffRegion.TilePixelSize, pixels, alpha));
            previousTile = number;
        }
        foreach (var number in tileMap.Distinct())
            if (!tiles.ContainsKey(number))
                throw Error(sourceName, $"MAP references missing TILE #{number}");

        var entities = new RegionEntityReference[checked((int)entityCount)];
        for (var index = 0; index < entities.Length; index++)
        {
            entities[index] = new(
                Int16(bytes, position), Int16(bytes, position + 2),
                unchecked((sbyte)bytes[position + 4]), bytes[position + 5],
                Int16(bytes, position + 6));
            position += GffRegion.EntityRecordSize;
        }
        var result = new PackedRegion(resourceNumber, name, palette, tileMap,
            geometryMap, tiles, entities);
        Validate(result, sourceName);
        return result;
    }

    private static void Validate(PackedRegion region, string sourceName)
    {
        if (string.IsNullOrEmpty(region.Name) || region.Name.Length >= GffRegion.MaximumNameBytes ||
            region.Name.Any(value => value is < ' ' or > '~'))
            throw Error(sourceName, "has an invalid name");
        if (region.Palette is null || region.Palette.Count != IndexedPalette.ColorCount)
            throw Error(sourceName, $"must contain {IndexedPalette.ColorCount} palette colors");
        if (region.TileMap is null || region.TileMap.Length != GffRegion.MapByteCount ||
            region.GeometryMap is null || region.GeometryMap.Length != GffRegion.MapByteCount)
            throw Error(sourceName, $"must contain two {GffRegion.MapByteCount}-byte map planes");
        if (region.Tiles is null || region.Tiles.Count is 0 or > byte.MaxValue + 1)
            throw Error(sourceName, "has an invalid tile count");
        foreach (var (number, tile) in region.Tiles)
        {
            if (tile is null || tile.Width != GffRegion.TilePixelSize ||
                tile.Height != GffRegion.TilePixelSize || tile.Pixels is null ||
                tile.Pixels.Length != TilePixelCount || tile.Alpha is null ||
                tile.Alpha.Length != TilePixelCount)
                throw Error(sourceName, $"TILE #{number} has invalid frame data");
            if (tile.Alpha.Any(value => value is not 0 and not 255))
                throw Error(sourceName, $"TILE #{number} contains non-binary alpha");
        }
        foreach (var number in region.TileMap.Distinct())
            if (!region.Tiles.ContainsKey(number))
                throw Error(sourceName, $"MAP references missing TILE #{number}");
        if (region.Entities is null || region.Entities.Count > GffRegion.MaximumEntityCount ||
            region.Entities.Any(entity => entity is null))
            throw Error(sourceName, "has an invalid entity table");
    }

    private static bool IsPrintableAscii(ReadOnlySpan<byte> bytes)
    {
        foreach (var value in bytes)
            if (value is < 0x20 or > 0x7e) return false;
        return true;
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
