using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class UiWindowGraphResolverTests
{
    [Fact]
    public void ResolvesMixedControlsInWindowOrder()
    {
        var resolved = UiWindowGraphResolver.Resolve(Catalog(), 19501);

        Assert.Equal((19501U, 19004U, (ushort)320, (ushort)200),
            (resolved.ResourceNumber, resolved.ImageResourceNumber, resolved.Width, resolved.Height));
        Assert.Equal([
            ResolvedUiControlKind.ApplicationFrame,
            ResolvedUiControlKind.Button,
            ResolvedUiControlKind.EditBox
        ], resolved.Controls.Select(item => item.Kind));
        Assert.Equal(new ResolvedUiControl(
                ResolvedUiControlKind.ApplicationFrame, 19200, 0, 0, 320, 200, 494, null),
            resolved.Controls[0]);
        Assert.Equal(new ResolvedUiControl(
                ResolvedUiControlKind.Button, 2002, 217, 10, 64, 6, 7, 12002),
            resolved.Controls[1]);
        Assert.Equal(new ResolvedUiControl(
                ResolvedUiControlKind.EditBox, 4003, 40, 125, 95, 8, 10, null),
            resolved.Controls[2]);
    }

    [Fact]
    public void RejectsMissingWindowChildAndDuplicateResource()
    {
        var catalog = Catalog();
        Assert.Throws<InvalidDataException>(() => UiWindowGraphResolver.Resolve(catalog, 999));
        Assert.Throws<InvalidDataException>(() => UiWindowGraphResolver.Resolve(
            catalog with { Buttons = [] }, 19501));
        Assert.Throws<InvalidDataException>(() => UiWindowGraphResolver.Resolve(
            catalog with { Buttons = [catalog.Buttons[0], catalog.Buttons[0]] }, 19501));
    }

    [Fact]
    public void RejectsUnsupportedChildTag()
    {
        var catalog = Catalog();
        var window = catalog.Windows[0] with
        {
            Children = [new UiChildReference("TEXT", 5000, 0, 0)]
        };

        Assert.Throws<InvalidDataException>(() => UiWindowGraphResolver.Resolve(
            catalog with { Windows = [window] }, 19501));
    }

    private static PackedUiCatalog Catalog() => new(
        [new UiWindowResource(19501, 19004, 320, 200,
        [
            new("APFM", 19200, 0, 0),
            new("BUTN", 2002, 217, 10),
            new("EBOX", 4003, 40, 125)
        ])],
        [new UiButtonResource(2002, 64, 6, 12002, 7)],
        [new UiApplicationFrameResource(19200, 320, 200, 494)],
        [new UiEditBoxResource(4003, 95, 8, 10)]);
}
