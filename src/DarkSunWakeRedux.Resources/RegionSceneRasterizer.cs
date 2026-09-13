namespace DarkSunWakeRedux.Resources;

public sealed record IndexedRegionViewport(
    int OriginX,
    int OriginY,
    int Width,
    int Height,
    byte[] Pixels,
    byte[] Alpha);

public static class RegionSceneRasterizer
{
    public const int WorldWidth = GffRegion.TileColumns * GffRegion.TilePixelSize;
    public const int WorldHeight = GffRegion.TileRows * GffRegion.TilePixelSize;
    public const int MaximumViewportDimension = 4_096;
    public const int MaximumViewportPixels = 16 * 1024 * 1024;
    public const byte MirroredEntityFlag = 0x80;

    public static IndexedRegionViewport Rasterize(
        PackedRegion region,
        PackedObjectFrameCatalog objects,
        int originX,
        int originY,
        int width,
        int height)
    {
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(objects);
        ValidateViewport(originX, originY, width, height);
        ValidateRegion(region);

        if (objects.Definitions is null || objects.Images is null ||
            objects.Definitions.Any(item => item is null) || objects.Images.Any(item => item is null))
            throw Error("object catalog is incomplete");
        Dictionary<uint, PackedObjectFrameDefinition> definitions;
        Dictionary<uint, PackedObjectImage> images;
        try
        {
            definitions = objects.Definitions.ToDictionary(item => item.ResourceNumber);
            images = objects.Images.ToDictionary(item => item.ResourceNumber);
        }
        catch (ArgumentException)
        {
            throw Error("object catalog contains duplicate resource numbers");
        }
        foreach (var entity in region.Entities)
            if (!definitions.ContainsKey(entity.ObjectResourceNumber))
                throw Error($"ETAB references missing object definition #{entity.ObjectResourceNumber}");

        var pixels = new byte[checked(width * height)];
        var alpha = new byte[pixels.Length];
        RasterizeTerrain(region, originX, originY, width, height, pixels, alpha);
        RasterizeObjects(region, definitions, images,
            originX, originY, width, height, pixels, alpha);
        return new(originX, originY, width, height, pixels, alpha);
    }

    private static void RasterizeTerrain(
        PackedRegion region,
        int originX,
        int originY,
        int width,
        int height,
        byte[] pixels,
        byte[] alpha)
    {
        for (var viewportY = 0; viewportY < height; viewportY++)
        {
            var worldY = originY + viewportY;
            var tileY = worldY / GffRegion.TilePixelSize;
            var pixelY = worldY % GffRegion.TilePixelSize;
            for (var viewportX = 0; viewportX < width; viewportX++)
            {
                var worldX = originX + viewportX;
                var tileX = worldX / GffRegion.TilePixelSize;
                var pixelX = worldX % GffRegion.TilePixelSize;
                var tileNumber = region.TileMap[tileY * GffRegion.TileColumns + tileX];
                var tile = region.Tiles[tileNumber];
                var source = pixelY * GffRegion.TilePixelSize + pixelX;
                if (tile.Alpha[source] == 0) continue;
                var target = viewportY * width + viewportX;
                pixels[target] = tile.Pixels[source];
                alpha[target] = 255;
            }
        }
    }

    private static void RasterizeObjects(
        PackedRegion region,
        IReadOnlyDictionary<uint, PackedObjectFrameDefinition> definitions,
        IReadOnlyDictionary<uint, PackedObjectImage> images,
        int originX,
        int originY,
        int width,
        int height,
        byte[] pixels,
        byte[] alpha)
    {
        foreach (var entity in region.Entities)
        {
            var definition = definitions[entity.ObjectResourceNumber];
            if (!images.TryGetValue(definition.ImageResourceNumber, out var image))
                throw Error($"object definition #{definition.ResourceNumber} references missing " +
                    $"image #{definition.ImageResourceNumber}");
            if (image.Frames is null || image.Frames.Count == 0)
                throw Error($"object image #{image.ResourceNumber} contains no frames");
            var frame = image.Frames[0];
            ValidateFrame(frame, $"object image #{image.ResourceNumber} frame 0");
            var left = entity.X - definition.XOffset;
            var top = entity.Y - definition.YOffset - entity.VerticalOffset;
            var mirrored = (entity.Flags & MirroredEntityFlag) != 0;
            for (var sourceY = 0; sourceY < frame.Height; sourceY++)
            {
                var targetY = top + sourceY - originY;
                if (targetY < 0 || targetY >= height) continue;
                for (var sourceX = 0; sourceX < frame.Width; sourceX++)
                {
                    var source = sourceY * frame.Width + sourceX;
                    if (frame.Alpha[source] == 0) continue;
                    var worldX = left + (mirrored ? frame.Width - 1 - sourceX : sourceX);
                    var targetX = worldX - originX;
                    if (targetX < 0 || targetX >= width) continue;
                    var target = targetY * width + targetX;
                    pixels[target] = frame.Pixels[source];
                    alpha[target] = 255;
                }
            }
        }
    }

    private static void ValidateViewport(int originX, int originY, int width, int height)
    {
        if (width is <= 0 or > MaximumViewportDimension ||
            height is <= 0 or > MaximumViewportDimension ||
            (long)width * height > MaximumViewportPixels)
            throw Error($"viewport has invalid dimensions {width}x{height}");
        if (originX < 0 || originY < 0 ||
            (long)originX + width > WorldWidth || (long)originY + height > WorldHeight)
            throw Error($"viewport ({originX},{originY},{width},{height}) exceeds the " +
                $"{WorldWidth}x{WorldHeight} region");
    }

    private static void ValidateRegion(PackedRegion region)
    {
        if (region.TileMap is null || region.TileMap.Length != GffRegion.MapByteCount)
            throw Error($"region must contain a {GffRegion.MapByteCount}-byte terrain map");
        if (region.Tiles is null)
            throw Error("region has no tile catalog");
        foreach (var number in region.TileMap.Distinct())
        {
            if (!region.Tiles.TryGetValue(number, out var tile))
                throw Error($"MAP references missing tile #{number}");
            if (tile is null || tile.Width != GffRegion.TilePixelSize ||
                tile.Height != GffRegion.TilePixelSize ||
                tile.Pixels.Length != GffRegion.TilePixelSize * GffRegion.TilePixelSize ||
                tile.Alpha.Length != GffRegion.TilePixelSize * GffRegion.TilePixelSize)
                throw Error($"tile #{number} has invalid frame data");
            if (tile.Alpha.Any(value => value is not 0 and not 255))
                throw Error($"tile #{number} contains non-binary alpha");
        }
        if (region.Entities is null || region.Entities.Any(item => item is null))
            throw Error("region has an invalid entity table");
    }

    private static void ValidateFrame(IndexedImageFrame frame, string name)
    {
        if (frame is null || frame.Width is <= 0 or > IndexedImage.MaximumDimension ||
            frame.Height is <= 0 or > IndexedImage.MaximumDimension || frame.Pixels is null ||
            frame.Alpha is null || (long)frame.Width * frame.Height != frame.Pixels.Length ||
            frame.Alpha.Length != frame.Pixels.Length)
            throw Error($"{name} has invalid frame data");
        if (frame.Alpha.Any(value => value is not 0 and not 255))
            throw Error($"{name} contains non-binary alpha");
    }

    private static InvalidDataException Error(string message) =>
        new($"region scene: {message}.");
}
