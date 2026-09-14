using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DialoguePreviewTextTests
{
    [Fact]
    public void WrapsSpeechAndSelectsFiveLiteralResponses()
    {
        var projection = new FirstTyrDialogueProjection(18,
            [Literal("one two three")],
            Enumerable.Range(0, 6).Select(index =>
                new GplDialogueChoice(Literal($"choice {index}"), 100 + index,
                    new(GplDialogueConditionKind.Constant, 0, 1))).ToArray());

        var content = DialoguePreviewText.Create(
            Font(), projection, DialogueVariableSnapshot.Empty);

        Assert.Equal(2, content.Speech.Height);
        Assert.Equal(5, content.Responses.Count);
        Assert.Equal([0, 1, 2, 3, 4],
            content.Responses.Select(response => response.ChoiceIndex));
        Assert.Equal([100, 101, 102, 103, 104],
            content.Responses.Select(response => response.TargetOffset));
        Assert.All(content.Responses,
            response => Assert.InRange(response.Text.Width, 1, 296));
        Assert.Equal(2, DialoguePreviewText.Wrap(
            Font(), "one two three", 7, 2).Count);
    }

    [Fact]
    public void RejectsMissingLiteralSpeechExcessLinesAndUnsupportedCharacters()
    {
        var variable = new GplDialogueTextSource(
            GplDialogueTextSourceKind.Variable, string.Empty, 6, 2);
        GplDialogueChoice response = new(Literal("choice"), 1,
            new(GplDialogueConditionKind.Constant, 0, 1));
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [variable], [response]), DialogueVariableSnapshot.Empty));
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [Literal(string.Empty)], [response]), DialogueVariableSnapshot.Empty));
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [Literal("speech")],
                [new(Literal(string.Empty), 1,
                    new(GplDialogueConditionKind.Constant, 0, 1))]),
            DialogueVariableSnapshot.Empty));
        Assert.Throws<InvalidDataException>(() =>
            DialoguePreviewText.Wrap(Font(), "a b c", 1, 2));
        Assert.Throws<InvalidDataException>(() =>
            DialoguePreviewText.Wrap(Font(), "\u20ac", 10, 2));
    }

    [Fact]
    public void FiltersFalseAndUnknownConditionsBeforeApplyingTheRowLimit()
    {
        var projection = new FirstTyrDialogueProjection(18, [Literal("speech")],
        [
            Choice("unknown", 10, GplDialogueConditionKind.LocalFlag, 4, 1),
            Choice("false", 11, GplDialogueConditionKind.Constant, 0, 0),
            Choice("first", 12, GplDialogueConditionKind.Constant, 0, 1),
            Choice("known", 13, GplDialogueConditionKind.LocalNumberEquals, 2, 7),
            Choice("second", 14, GplDialogueConditionKind.Constant, 0, 1),
            Choice("third", 15, GplDialogueConditionKind.Constant, 0, 1),
            Choice("fourth", 16, GplDialogueConditionKind.Constant, 0, 1),
            Choice("fifth", 17, GplDialogueConditionKind.Constant, 0, 1),
            Choice("sixth", 18, GplDialogueConditionKind.Constant, 0, 1)
        ]);
        var variables = new DialogueVariableSnapshot(
            new Dictionary<ushort, bool>(), new Dictionary<ushort, int> { [2] = 7 });

        var content = DialoguePreviewText.Create(Font(), projection, variables);

        Assert.Equal([2, 3, 4, 5, 6],
            content.Responses.Select(response => response.ChoiceIndex));
        Assert.Equal([12, 13, 14, 15, 16],
            content.Responses.Select(response => response.TargetOffset));
    }

    [Fact]
    public void ResolvesObservedOpeningVariableExitLabel()
    {
        var variable = new GplDialogueTextSource(
            GplDialogueTextSourceKind.Variable, string.Empty, 6, 5);
        var projection = new FirstTyrDialogueProjection(18, [Literal("speech")],
        [
            Choice("first", 10, GplDialogueConditionKind.LocalFlag, 0, 1),
            Choice("second", 11, GplDialogueConditionKind.LocalFlag, 1, 1),
            Choice("third", 12, GplDialogueConditionKind.LocalFlag, 2, 1),
            Choice("fourth", 13, GplDialogueConditionKind.LocalFlag, 3, 1),
            new(variable, 14, new(GplDialogueConditionKind.Constant, 0, 1))
        ]);

        var content = DialoguePreviewText.Create(
            Font(), projection, FirstTyrDialogueObservedState.Create(),
            new Dictionary<GplDialogueVariable, string> { [new(6, 5)] = "Depart!!" });

        Assert.Equal([0, 1, 2, 3, 4],
            content.Responses.Select(response => response.ChoiceIndex));
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), projection, FirstTyrDialogueObservedState.Create()));
    }

    [Fact]
    public void RendersTheRecomputedChoicesAfterAResponseReturns()
    {
        var projection = new FirstTyrDialogueProjection(18, [Literal("speech")],
        [
            Choice("first", 10, GplDialogueConditionKind.LocalFlag, 0, 1),
            Choice("second", 11, GplDialogueConditionKind.LocalFlag, 1, 1),
            Choice("third", 12, GplDialogueConditionKind.LocalFlag, 2, 1),
            Choice("fourth", 13, GplDialogueConditionKind.LocalFlag, 3, 1),
            Choice("exit", 14, GplDialogueConditionKind.Constant, 0, 1)
        ]);
        var session = DialogueSessionAdapter.Create(
            projection, FirstTyrDialogueObservedState.Create(), 5);
        session.Execute(DialogueCommand.Select(0));
        session.Execute(DialogueCommand.Apply(new(0, 10,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [0] = false })));

        var content = DialoguePreviewText.Create(
            Font(), projection, session.Snapshot());

        Assert.Equal([1, 2, 3, 4],
            content.Responses.Select(response => response.ChoiceIndex));
        Assert.Equal([11, 12, 13, 14],
            content.Responses.Select(response => response.TargetOffset));
    }

    private static GplDialogueTextSource Literal(string text) =>
        new(GplDialogueTextSourceKind.Literal, text, 0, 0);

    private static GplDialogueChoice Choice(
        string text,
        int target,
        GplDialogueConditionKind kind,
        ushort variable,
        int value) => new(Literal(text), target, new(kind, variable, value));

    private static PackedIndexedBitmapFont Font()
    {
        var map = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
        var glyphs = Enumerable.Range(0, 256)
            .Select(_ => new IndexedFontGlyph(1, 2, [1, 1])).ToArray();
        return new(map, glyphs);
    }
}
