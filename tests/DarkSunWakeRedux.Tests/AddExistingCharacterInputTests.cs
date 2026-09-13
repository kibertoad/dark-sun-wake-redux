using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class AddExistingCharacterInputTests
{
    [Fact]
    public void ResolvesExactOwnedGraphAndRecordedRuntimeSubstitutions()
    {
        var controls = AddExistingCharacterInput.Resolve(Catalog());

        Assert.Equal(17, controls.Count);
        Assert.Equal([18300U, 18301U, 18302U, 18303U, 18304U, 18305U, 18306U,
            18307U, 18308U, 18309U, 18310U, 18311U, 18312U, 18313U, 10314U,
            10315U, 18401U], controls.Select(control => control.ResourceNumber));
        var title = controls[0];
        Assert.Equal((18107U, "images/add-existing/title.dsix", 67, 23),
            (title.SourceImageResourceNumber, title.DisplayAssetPath, title.Width, title.Height));
        var add = controls[1];
        Assert.Equal((18108U, "images/add-existing/add.dsix"),
            (add.SourceImageResourceNumber, add.DisplayAssetPath));
        var rows = controls.Where(control => control.Kind == AddExistingControlKind.Row).ToArray();
        Assert.Equal(Enumerable.Range(0, 10).Select(index => (int?)index),
            rows.Select(control => control.RowIndex));
        Assert.All(rows, row => Assert.Equal((163, 11, 18100U, (ushort)208),
            (row.Width, row.Height, row.SourceImageResourceNumber, row.EventMask)));
    }

    [Fact]
    public void HitTestUsesExclusiveGraphRectanglesAndSkipsDecorativeControls()
    {
        var controls = AddExistingCharacterInput.Resolve(Catalog());
        var firstRow = Assert.Single(controls,
            control => control.Kind == AddExistingControlKind.Row && control.RowIndex == 0);

        Assert.Equal(firstRow, AddExistingCharacterInput.HitTest(controls, 46, 31));
        Assert.Equal(firstRow, AddExistingCharacterInput.HitTest(controls, 208, 41));
        Assert.Null(AddExistingCharacterInput.HitTest(controls, 209, 31));
        Assert.Null(AddExistingCharacterInput.HitTest(controls, 110, 0));
        Assert.Null(AddExistingCharacterInput.HitTest(controls, 49, 147));
    }

    [Fact]
    public void RejectsChangedWindowControlAndStaticImageContracts()
    {
        var catalog = Catalog();
        Assert.Throws<InvalidDataException>(() => AddExistingCharacterInput.Resolve(catalog with
        {
            Windows = [catalog.Windows[0] with { Height = 200 }]
        }));
        Assert.Throws<InvalidDataException>(() => AddExistingCharacterInput.Resolve(catalog with
        {
            Buttons = catalog.Buttons.Select(button => button.ResourceNumber == 18301
                ? button with { ImageResourceNumber = 18104 }
                : button).ToArray()
        }));
        Assert.Throws<InvalidDataException>(() => AddExistingCharacterInput.Resolve(catalog with
        {
            Windows = [catalog.Windows[0] with
            {
                Children = catalog.Windows[0].Children.Reverse().ToArray()
            }]
        }));
    }

    [Fact]
    public void OnlyTheEvidencedExitControlMapsToACoreCommand()
    {
        var controls = AddExistingCharacterInput.Resolve(Catalog());
        var exit = Assert.Single(controls, control => control.Kind == AddExistingControlKind.Exit);
        var add = Assert.Single(controls, control => control.Kind == AddExistingControlKind.Add);

        Assert.Equal(StartFlowCommand.Cancel(), AddExistingCharacterInput.CommandFor(exit));
        Assert.Null(AddExistingCharacterInput.CommandFor(add));

        var session = new StartFlowSession(0);
        session.Execute(StartFlowCommand.Choose(StartWindowChoice.CreateCharacters));
        session.Execute(StartFlowCommand.OpenEmptySlot());
        session.Execute(StartFlowCommand.Choose(EmptySlotChoice.Add));
        var result = session.Execute(AddExistingCharacterInput.CommandFor(exit)!);

        Assert.True(result.Accepted);
        Assert.Equal(StartFlowScreen.PartyOverview, session.Snapshot().Screen);
    }

    private static PackedUiCatalog Catalog()
    {
        var children = new List<UiChildReference>
        {
            new("BUTN", 18300, 110, 0), new("BUTN", 18301, 231, 30),
            new("BUTN", 18302, 231, 50), new("BUTN", 18303, 215, 148)
        };
        for (var index = 0; index < 10; index++)
            children.Add(new("BUTN", checked((uint)(18304 + index)), 46,
                checked((short)(31 + index * 11))));
        children.Add(new("BUTN", 10314, 215, 30));
        children.Add(new("BUTN", 10315, 215, 130));
        children.Add(new("EBOX", 18401, 49, 147));

        var buttons = new List<UiButtonResource>
        {
            new(18300, 67, 23, 18107, 4), new(18301, 44, 15, 18108, 0),
            new(18302, 44, 15, 18109, 0), new(18303, 62, 15, 18110, 0)
        };
        for (var index = 0; index < 10; index++)
            buttons.Add(new(checked((uint)(18304 + index)), 163, 11, 18100, 208));
        buttons.Add(new(10314, 14, 14, 12102, 0));
        buttons.Add(new(10315, 14, 14, 12101, 0));

        return new(
            [new(18501, 10002, 320, 181, children)], buttons, [],
            [new UiEditBoxResource(18401, 164, 12, 10)]);
    }
}
