using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;
using System.Text;

namespace DarkSunWakeRedux.Game;

public sealed record DialoguePreviewResponse(
    int ChoiceIndex,
    int TargetOffset,
    IndexedGlyphRun Text);

public sealed record DialoguePreviewTextContent(
    IndexedGlyphBlock Speech,
    IReadOnlyList<DialoguePreviewResponse> Responses);

public static class DialoguePreviewText
{
    public const int SpeechWidth = 232;
    public const int MaximumSpeechLines = 5;
    public const int ResponseWidth = 296;
    public const int MaximumResponses = 5;

    public static DialoguePreviewTextContent Create(
        PackedIndexedBitmapFont font,
        FirstTyrDialogueProjection projection,
        DialogueVariableSnapshot variables,
        IReadOnlyDictionary<GplDialogueVariable, string>? variableText = null)
    {
        var session = DialogueSessionAdapter.Create(
            projection, variables, MaximumResponses);
        return Create(font, projection, session.Snapshot(), variableText);
    }

    public static DialoguePreviewTextContent Create(
        PackedIndexedBitmapFont font,
        FirstTyrDialogueProjection projection,
        DialogueSnapshot dialogue,
        IReadOnlyDictionary<GplDialogueVariable, string>? variableText = null,
        IReadOnlyList<GplDialogueOutput>? speechOutput = null)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(dialogue);
        variableText ??= new Dictionary<GplDialogueVariable, string>();
        if (dialogue.ScriptResourceNumber !=
                OriginalContent.FirstTyrDialogueScriptResourceNumber)
            throw new InvalidDataException(
                "The dialogue preview state has an unexpected script identity.");
        var speechLines = speechOutput is null
            ? OpeningSpeech(font, projection)
            : ResponseSpeech(font, speechOutput, variableText);
        if (speechLines.Count == 0 || speechLines.All(line => line.IsEmpty))
            throw new InvalidDataException("The dialogue preview speech is empty.");
        var speech = IndexedGlyphBlockRasterizer.Rasterize(font, speechLines);
        var responses = dialogue.Choices
            .Select(identity =>
            {
                var choice = ResolveChoice(projection, identity);
                return (Index: identity.SourceIndex, Choice: choice);
            })
            .Select(item =>
            {
                var text = Resolve(item.Choice.Label, variableText);
                var fitted = Fit(font, text.Trim(), ResponseWidth);
                if (fitted.IsEmpty)
                    throw new InvalidDataException("A dialogue preview response is empty.");
                return new DialoguePreviewResponse(
                    item.Index,
                    item.Choice.TargetOffset,
                    IndexedGlyphRunRasterizer.Rasterize(font, fitted.Span));
            })
            .ToArray();
        return new(speech, responses);
    }

    private static IReadOnlyList<ReadOnlyMemory<byte>> OpeningSpeech(
        PackedIndexedBitmapFont font,
        FirstTyrDialogueProjection projection)
    {
        var source = projection.SpeechVariants.FirstOrDefault(candidate =>
            candidate.Kind == GplDialogueTextSourceKind.Literal)
            ?? throw new InvalidDataException(
                "The dialogue preview has no literal opening speech source.");
        return Wrap(font, source.Text, SpeechWidth, MaximumSpeechLines);
    }

    private static IReadOnlyList<ReadOnlyMemory<byte>> ResponseSpeech(
        PackedIndexedBitmapFont font,
        IReadOnlyList<GplDialogueOutput> output,
        IReadOnlyDictionary<GplDialogueVariable, string> variableText)
    {
        ArgumentNullException.ThrowIfNull(output);
        var text = new StringBuilder();
        foreach (var item in output)
        {
            if (item is null)
                throw new InvalidDataException("Dialogue response output contains a null item.");
            switch (item.Kind)
            {
                case GplDialogueOutputKind.Text when item.Text is not null:
                    text.Append(Resolve(item.Text, variableText));
                    break;
                case GplDialogueOutputKind.NewLine when item.Text is null:
                    text.Append('\n');
                    break;
                case GplDialogueOutputKind.Text:
                    throw new InvalidDataException(
                        "Dialogue text output has no text source.");
                case GplDialogueOutputKind.NewLine:
                    throw new InvalidDataException(
                        "Dialogue newline output unexpectedly has a text source.");
                default:
                    throw new InvalidDataException(
                        $"Dialogue output kind {item.Kind} is unsupported.");
            }
        }
        var lines = new List<ReadOnlyMemory<byte>>();
        foreach (var paragraph in text.ToString().Split('\n'))
        {
            if (lines.Count == MaximumSpeechLines)
                throw new InvalidDataException(
                    $"Dialogue speech exceeds the {MaximumSpeechLines}-line preview area.");
            if (paragraph.Length == 0)
            {
                lines.Add(ReadOnlyMemory<byte>.Empty);
                continue;
            }
            lines.AddRange(Wrap(font, paragraph, SpeechWidth,
                MaximumSpeechLines - lines.Count));
        }
        return lines;
    }

    private static GplDialogueChoice ResolveChoice(
        FirstTyrDialogueProjection projection,
        DialogueChoiceIdentity identity)
    {
        foreach (var page in new[]
                 {
                     projection.InitialChoices,
                     projection.SecondChoices,
                     projection.ThirdChoices
                 })
        {
            if (identity.SourceIndex < page.Count &&
                page[identity.SourceIndex].TargetOffset == identity.BranchTargetOffset)
                return page[identity.SourceIndex];
        }
        throw new InvalidDataException(
            "The dialogue preview choice identity is outside the projected pages.");
    }

    private static string Resolve(
        GplDialogueTextSource source,
        IReadOnlyDictionary<GplDialogueVariable, string> variableText) =>
        source.Kind switch
        {
            GplDialogueTextSourceKind.Literal => source.Text,
            GplDialogueTextSourceKind.Variable when variableText.TryGetValue(
                new(source.VariableType, source.VariableId), out var text) => text,
            GplDialogueTextSourceKind.Variable => throw new InvalidDataException(
                $"Dialogue text variable type {source.VariableType} #{source.VariableId} is unresolved."),
            _ => throw new InvalidDataException(
                $"Dialogue response text source {source.Kind} is unsupported.")
        };

    public static IReadOnlyList<ReadOnlyMemory<byte>> Wrap(
        PackedIndexedBitmapFont font,
        string text,
        int maximumWidth,
        int maximumLines)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);
        if (maximumWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maximumWidth));
        if (maximumLines <= 0) throw new ArgumentOutOfRangeException(nameof(maximumLines));
        var words = text.Split((char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var lines = new List<ReadOnlyMemory<byte>>();
        var current = new List<byte>();
        foreach (var word in words)
        {
            var encoded = Encode(font, word);
            var candidate = current.Count == 0
                ? encoded
                : [.. current, Map(font, ' '), .. encoded];
            if (Width(font, candidate) <= maximumWidth)
            {
                current = [.. candidate];
                continue;
            }
            if (current.Count > 0)
            {
                AddLine(lines, current, maximumLines);
                current.Clear();
            }
            var remaining = encoded.AsMemory();
            while (remaining.Length > 0)
            {
                var fitted = Fit(font, remaining, maximumWidth);
                if (fitted.Length == remaining.Length)
                {
                    current.AddRange(fitted.ToArray());
                    break;
                }
                AddLine(lines, fitted.ToArray(), maximumLines);
                remaining = remaining[fitted.Length..];
            }
        }
        if (current.Count > 0) AddLine(lines, current, maximumLines);
        return lines;
    }

    private static ReadOnlyMemory<byte> Fit(
        PackedIndexedBitmapFont font,
        string text,
        int maximumWidth) => Fit(font, Encode(font, text), maximumWidth);

    private static ReadOnlyMemory<byte> Fit(
        PackedIndexedBitmapFont font,
        ReadOnlyMemory<byte> glyphs,
        int maximumWidth)
    {
        var width = 0;
        var count = 0;
        foreach (var glyph in glyphs.Span)
        {
            var glyphWidth = font.Glyphs[glyph].Width;
            if (width + glyphWidth > maximumWidth) break;
            width += glyphWidth;
            count++;
        }
        if (count == 0 && glyphs.Length > 0)
            throw new InvalidDataException(
                "A dialogue glyph is wider than the available text area.");
        return glyphs[..count];
    }

    private static byte[] Encode(PackedIndexedBitmapFont font, string text)
    {
        var result = new byte[text.Length];
        for (var index = 0; index < text.Length; index++)
            result[index] = Map(font, text[index]);
        return result;
    }

    private static byte Map(PackedIndexedBitmapFont font, char character)
    {
        if (character > byte.MaxValue)
            throw new InvalidDataException(
                $"Dialogue preview character U+{(int)character:X4} is outside the font map.");
        return font.CharacterMap[(byte)character];
    }

    private static int Width(PackedIndexedBitmapFont font, IReadOnlyList<byte> glyphs)
    {
        var width = 0;
        foreach (var glyph in glyphs) width = checked(width + font.Glyphs[glyph].Width);
        return width;
    }

    private static void AddLine(
        ICollection<ReadOnlyMemory<byte>> lines,
        IReadOnlyCollection<byte> glyphs,
        int maximumLines)
    {
        if (lines.Count == maximumLines)
            throw new InvalidDataException(
                $"Dialogue speech exceeds the {maximumLines}-line preview area.");
        lines.Add(glyphs.ToArray());
    }
}
