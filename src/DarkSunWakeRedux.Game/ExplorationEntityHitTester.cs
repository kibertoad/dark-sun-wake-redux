using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record ExplorationEntityHit(
    int EntityIndex,
    RegionEntityReference Entity,
    PackedObjectFrameDefinition Definition);

public sealed class ExplorationEntityHitTester
{
    private sealed record Entry(
        int EntityIndex,
        RegionEntityReference Entity,
        PackedObjectFrameDefinition Definition,
        IndexedImageFrame Frame,
        int Left,
        int Top);

    private readonly IReadOnlyList<Entry> _entries;

    public ExplorationEntityHitTester(PackedRegion region, PackedObjectFrameCatalog objects)
    {
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(objects);
        if (region.Entities is null || objects.Definitions is null || objects.Images is null)
            throw new ArgumentException("Exploration hit-test catalogs must be complete.");
        Dictionary<uint, PackedObjectFrameDefinition> definitions;
        Dictionary<uint, PackedObjectImage> images;
        try
        {
            definitions = objects.Definitions.ToDictionary(item => item.ResourceNumber);
            images = objects.Images.ToDictionary(item => item.ResourceNumber);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException(
                "Exploration hit-test catalogs contain duplicate resource numbers.", exception);
        }

        var entries = new Entry[region.Entities.Count];
        for (var index = 0; index < entries.Length; index++)
        {
            var entity = region.Entities[index];
            if (!definitions.TryGetValue(entity.ObjectResourceNumber, out var definition) ||
                !images.TryGetValue(definition.ImageResourceNumber, out var image) ||
                image.Frames is null || image.Frames.Count == 0)
                throw new ArgumentException(
                    $"Exploration entity {index} has an incomplete object-frame reference.");
            var frame = image.Frames[0];
            if (frame.Pixels is null || frame.Alpha is null || frame.Width <= 0 ||
                frame.Height <= 0 || frame.Pixels.Length != frame.Width * frame.Height ||
                frame.Alpha.Length != frame.Pixels.Length)
                throw new ArgumentException(
                    $"Exploration entity {index} has invalid first-frame geometry.");
            entries[index] = new(index, entity, definition, frame,
                entity.X - definition.XOffset,
                entity.Y - definition.YOffset - entity.VerticalOffset);
        }
        _entries = entries;
    }

    public ExplorationEntityHit? HitTest(int worldX, int worldY)
    {
        for (var index = _entries.Count - 1; index >= 0; index--)
        {
            var entry = _entries[index];
            var sourceX = worldX - entry.Left;
            var sourceY = worldY - entry.Top;
            if (sourceX < 0 || sourceY < 0 ||
                sourceX >= entry.Frame.Width || sourceY >= entry.Frame.Height)
                continue;
            if ((entry.Entity.Flags & RegionSceneRasterizer.MirroredEntityFlag) != 0)
                sourceX = entry.Frame.Width - 1 - sourceX;
            if (entry.Frame.Alpha[sourceY * entry.Frame.Width + sourceX] == 0)
                continue;
            return new(entry.EntityIndex, entry.Entity, entry.Definition);
        }
        return null;
    }
}
