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
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [variable], []), DialogueVariableSnapshot.Empty));
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [Literal(string.Empty)], []), DialogueVariableSnapshot.Empty));
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
