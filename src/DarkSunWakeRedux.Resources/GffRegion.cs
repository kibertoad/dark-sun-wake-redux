using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record RegionEntityReference(
    short X,
    short Y,
    sbyte VerticalOffset,
    byte Flags,
    short SignedObjectResourceNumber)
{
    public uint ObjectResourceNumber => checked((uint)Math.Abs((int)SignedObjectResourceNumber));
}

public sealed record GffRegion(
    uint ResourceNumber,
    string Name,
    IndexedPalette Palette,
    byte[] TileMap,
    byte[] GeometryMap,
    IReadOnlyDictionary<byte, IndexedImageFrame> Tiles,
    IReadOnlyList<RegionEntityReference> Entities)
{
    public const int TileColumns = 128;
    public const int TileRows = 98;
    public const int TilePixelSize = 16;
    public const int MapByteCount = TileColumns * TileRows;
    public const int EntityRecordSize = 8;
    public const int MaximumNameBytes = 64;
    public const int MaximumEntityCount = 16_384;

    public static GffRegion Read(
        GffArchive regionArchive,
        GffArchive objectArchive,
        string sourceName = "region GFF")
    {
        ArgumentNullException.ThrowIfNull(regionArchive);
        ArgumentNullException.ThrowIfNull(objectArchive);

        var nameDescriptor = Single(regionArchive, "RNME", sourceName);
        var number = nameDescriptor.Number;
        Matching(regionArchive, "PAL ", number, sourceName);
        Matching(regionArchive, "MAP ", number, sourceName);
        Matching(regionArchive, "GMAP", number, sourceName);
        Matching(regionArchive, "ETAB", number, sourceName);

        var name = ReadName(regionArchive.GetResource("RNME", number).Span, sourceName);
        var palette = IndexedPalette.Read(regionArchive.GetResource("PAL ", number).Span,
            $"{sourceName}:PAL #{number}");
        var tileMap = ReadPlane(regionArchive.GetResource("MAP ", number), "MAP", sourceName, number);
        var geometryMap = ReadPlane(regionArchive.GetResource("GMAP", number), "GMAP", sourceName, number);
        var tiles = ReadTiles(regionArchive, tileMap, sourceName);
        var entities = ReadEntities(regionArchive.GetResource("ETAB", number), objectArchive,
            sourceName, number);

        return new(number, name, palette, tileMap, geometryMap, tiles, entities);
    }

    private static GffResource Single(GffArchive archive, string tag, string sourceName)
    {
        var matches = archive.Resources.Where(resource => resource.Tag == tag).ToArray();
        if (matches.Length != 1)
            throw Error(sourceName, $"must contain exactly one {tag.TrimEnd()} resource");
        return matches[0];
    }

    private static GffResource Matching(
        GffArchive archive, string tag, uint number, string sourceName)
    {
        var resource = Single(archive, tag, sourceName);
        if (resource.Number != number)
            throw Error(sourceName,
                $"{tag.TrimEnd()} resource #{resource.Number} does not match RNME #{number}");
        return resource;
    }

    private static string ReadName(ReadOnlySpan<byte> bytes, string sourceName)
    {
        if (bytes.Length is < 2 or > MaximumNameBytes)
            throw Error(sourceName, $"RNME length must be between 2 and {MaximumNameBytes} bytes");
        if (bytes[^1] != 0)
            throw Error(sourceName, "RNME is not NUL-terminated");
        for (var index = 0; index < bytes.Length - 1; index++)
            if (bytes[index] is < 0x20 or > 0x7e)
                throw Error(sourceName, $"RNME contains an unsupported byte at offset {index}");
        return Encoding.ASCII.GetString(bytes[..^1]);
    }

    private static byte[] ReadPlane(
        ReadOnlyMemory<byte> payload, string tag, string sourceName, uint number)
    {
        if (payload.Length != MapByteCount)
            throw Error(sourceName,
                $"{tag} #{number} must contain exactly {MapByteCount} bytes, found {payload.Length}");
        return payload.ToArray();
    }

    private static IReadOnlyDictionary<byte, IndexedImageFrame> ReadTiles(
        GffArchive archive, IReadOnlyList<byte> map, string sourceName)
    {
        var tileDescriptors = archive.Resources.Where(resource => resource.Tag == "TILE").ToArray();
        if (tileDescriptors.Length == 0)
            throw Error(sourceName, "contains no TILE resources");
        var tiles = new Dictionary<byte, IndexedImageFrame>();
        foreach (var descriptor in tileDescriptors)
        {
            if (descriptor.Number > byte.MaxValue)
                throw Error(sourceName, $"TILE #{descriptor.Number} cannot be addressed by the byte map");
            var number = checked((byte)descriptor.Number);
            var image = IndexedImage.Read(archive.GetResource("TILE", descriptor.Number),
                $"{sourceName}:TILE #{descriptor.Number}");
            if (image.Frames.Count != 1 || image.Frames[0].Width != TilePixelSize ||
                image.Frames[0].Height != TilePixelSize)
                throw Error(sourceName,
                    $"TILE #{descriptor.Number} must contain one {TilePixelSize}x{TilePixelSize} frame");
            tiles.Add(number, image.Frames[0]);
        }
        foreach (var number in map.Distinct())
            if (!tiles.ContainsKey(number))
                throw Error(sourceName, $"MAP references missing TILE #{number}");
        return tiles;
    }

    private static IReadOnlyList<RegionEntityReference> ReadEntities(
        ReadOnlyMemory<byte> payload,
        GffArchive objectArchive,
        string sourceName,
        uint number)
    {
        if (payload.Length % EntityRecordSize != 0)
            throw Error(sourceName, $"ETAB #{number} is not a sequence of {EntityRecordSize}-byte records");
        var count = payload.Length / EntityRecordSize;
        if (count > MaximumEntityCount)
            throw Error(sourceName, $"ETAB #{number} exceeds the {MaximumEntityCount}-record safety limit");
        var objectNumbers = objectArchive.Resources.Where(resource => resource.Tag == "OJFF")
            .Select(resource => resource.Number).ToHashSet();
        var bytes = payload.Span;
        var entities = new RegionEntityReference[count];
        for (var index = 0; index < count; index++)
        {
            var offset = index * EntityRecordSize;
            var entity = new RegionEntityReference(
                BinaryPrimitives.ReadInt16LittleEndian(bytes[offset..]),
                BinaryPrimitives.ReadInt16LittleEndian(bytes[(offset + 2)..]),
                unchecked((sbyte)bytes[offset + 4]),
                bytes[offset + 5],
                BinaryPrimitives.ReadInt16LittleEndian(bytes[(offset + 6)..]));
            if (!objectNumbers.Contains(entity.ObjectResourceNumber))
                throw Error(sourceName,
                    $"ETAB #{number} record {index} references missing OJFF #{entity.ObjectResourceNumber}");
            entities[index] = entity;
        }
        return entities;
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
