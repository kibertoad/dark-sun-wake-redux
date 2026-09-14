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

        var content = DialoguePreviewText.Create(Font(), projection);

        Assert.Equal(2, content.Speech.Height);
        Assert.Equal(5, content.Responses.Count);
        Assert.All(content.Responses, response => Assert.InRange(response.Width, 1, 296));
        Assert.Equal(2, DialoguePreviewText.Wrap(
            Font(), "one two three", 7, 2).Count);
    }

    [Fact]
    public void RejectsMissingLiteralSpeechExcessLinesAndUnsupportedCharacters()
    {
        var variable = new GplDialogueTextSource(
            GplDialogueTextSourceKind.Variable, string.Empty, 6, 2);
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [variable], [])));
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [Literal(string.Empty)], [])));
        Assert.Throws<InvalidDataException>(() => DialoguePreviewText.Create(
            Font(), new(18, [Literal("speech")],
                [new(Literal(string.Empty), 1,
                    new(GplDialogueConditionKind.Constant, 0, 1))])));
        Assert.Throws<InvalidDataException>(() =>
            DialoguePreviewText.Wrap(Font(), "a b c", 1, 2));
        Assert.Throws<InvalidDataException>(() =>
            DialoguePreviewText.Wrap(Font(), "\u20ac", 10, 2));
    }

    private static GplDialogueTextSource Literal(string text) =>
        new(GplDialogueTextSourceKind.Literal, text, 0, 0);

    private static PackedIndexedBitmapFont Font()
    {
        var map = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
        var glyphs = Enumerable.Range(0, 256)
            .Select(_ => new IndexedFontGlyph(1, 2, [1, 1])).ToArray();
        return new(map, glyphs);
    }
}
