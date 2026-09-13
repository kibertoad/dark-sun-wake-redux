namespace DarkSunWakeRedux.Resources;

public sealed class RegionTerrainGrid
{
    public const byte BlockingMask = 0x40;

    private readonly byte[] _geometry;

    public RegionTerrainGrid(PackedRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        if (region.GeometryMap is null || region.GeometryMap.Length != GffRegion.MapByteCount)
            throw new InvalidDataException(
                $"Region #{region.ResourceNumber} has an invalid geometry plane.");
        _geometry = region.GeometryMap.ToArray();
    }

    public int Width => GffRegion.TileColumns;

    public int Height => GffRegion.TileRows;

    public int OpenCellCount => _geometry.Count(value => (value & BlockingMask) == 0);

    public bool IsTerrainOpen(int cellX, int cellY) =>
        IsInBounds(cellX, cellY) &&
        (_geometry[cellY * GffRegion.TileColumns + cellX] & BlockingMask) == 0;

    public byte FlagsAt(int cellX, int cellY)
    {
        if (!IsInBounds(cellX, cellY))
            throw new ArgumentOutOfRangeException(nameof(cellX),
                $"Region cell ({cellX},{cellY}) is outside the {Width}x{Height} map.");
        return _geometry[cellY * GffRegion.TileColumns + cellX];
    }

    public (int X, int Y) CellAtWorldPixel(int worldX, int worldY)
    {
        if (worldX < 0 || worldY < 0 ||
            worldX >= Width * GffRegion.TilePixelSize ||
            worldY >= Height * GffRegion.TilePixelSize)
            throw new ArgumentOutOfRangeException(nameof(worldX),
                $"World pixel ({worldX},{worldY}) is outside the region.");
        return (worldX / GffRegion.TilePixelSize, worldY / GffRegion.TilePixelSize);
    }

    public (int X, int Y) CellCenter(int cellX, int cellY)
    {
        _ = FlagsAt(cellX, cellY);
        var halfTile = GffRegion.TilePixelSize / 2;
        return (cellX * GffRegion.TilePixelSize + halfTile,
            cellY * GffRegion.TilePixelSize + halfTile);
    }

    private static bool IsInBounds(int cellX, int cellY) =>
        cellX >= 0 && cellY >= 0 &&
        cellX < GffRegion.TileColumns && cellY < GffRegion.TileRows;
}
