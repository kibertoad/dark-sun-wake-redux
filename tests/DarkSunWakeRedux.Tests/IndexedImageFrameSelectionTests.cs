using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class IndexedImageFrameSelectionTests
{
    [Fact]
    public void SelectsRequestedDecodedFrame()
    {
        var first = new IndexedImageFrame(1, 1, [1], [255]);
        var second = new IndexedImageFrame(2, 1, [2, 3], [255, 255]);
        var image = new IndexedImage([first, second]);

        Assert.Same(second, IndexedImageFrameSelection.Select(image, 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void RejectsFrameIndexOutsideDecodedImage(int frameIndex)
    {
        var image = new IndexedImage([new IndexedImageFrame(1, 1, [1], [255])]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexedImageFrameSelection.Select(image, frameIndex));

        Assert.Equal("frameIndex", exception.ParamName);
    }
}
