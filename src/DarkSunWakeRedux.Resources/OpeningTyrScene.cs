namespace DarkSunWakeRedux.Resources;

public static class OpeningTyrScene
{
    public const int OriginX = 1_024;
    public const int OriginY = 1_368;
    public const int Width = 320;
    public const int Height = 200;
    public const int LeaderWorldX = 1_184;
    public const int LeaderWorldY = 1_459;
    public const int LeaderWidth = 17;
    public const int LeaderHeight = 35;
    public const int LeaderAnchorCellX = LeaderWorldX / GffRegion.TilePixelSize;
    public const int LeaderAnchorCellY = LeaderWorldY / GffRegion.TilePixelSize;

    public static IndexedRegionViewport Rasterize(
        PackedRegion region,
        PackedObjectFrameCatalog objects) =>
        RegionSceneRasterizer.Rasterize(region, objects, OriginX, OriginY, Width, Height);
}
