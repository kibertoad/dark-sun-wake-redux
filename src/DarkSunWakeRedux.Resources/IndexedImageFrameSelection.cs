namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Selects one decoded image frame with a bounded, actionable index check.
/// </summary>
public static class IndexedImageFrameSelection
{
    public static IndexedImageFrame Select(IndexedImage image, int frameIndex)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (frameIndex < 0 || frameIndex >= image.Frames.Count)
            throw new ArgumentOutOfRangeException(nameof(frameIndex), frameIndex,
                $"Frame index must be from 0 through {image.Frames.Count - 1}.");
        return image.Frames[frameIndex];
    }
}
