namespace DarkSunWakeRedux.Resources;

public static class OpeningTyrScene
{
    private static readonly (int Width, int Height)[] LeaderFrames =
    [
        (17, 35), (17, 36), (11, 33), (18, 38), (18, 38), (16, 38), (16, 38),
        (13, 33), (22, 32), (15, 32), (20, 32), (28, 30), (28, 27)
    ];

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

    public static IReadOnlyList<(int Width, int Height)> LeaderFrameGeometry { get; } =
        Array.AsReadOnly(LeaderFrames);

    public static IndexedRegionViewport Rasterize(
        PackedRegion region,
        PackedObjectFrameCatalog objects) =>
        RegionSceneRasterizer.Rasterize(region, objects, OriginX, OriginY, Width, Height);
}
