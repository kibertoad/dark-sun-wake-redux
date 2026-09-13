namespace DarkSunWakeRedux.Resources;

public static class OpeningTyrScene
{
    public const int OriginX = 1_024;
    public const int OriginY = 1_368;
    public const int Width = 320;
    public const int Height = 200;

    public static IndexedRegionViewport Rasterize(
        PackedRegion region,
        PackedObjectFrameCatalog objects) =>
        RegionSceneRasterizer.Rasterize(region, objects, OriginX, OriginY, Width, Height);
}
