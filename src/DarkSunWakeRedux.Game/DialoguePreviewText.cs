using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record DialoguePreviewTextContent(
    IndexedGlyphBlock Speech,
    IReadOnlyList<IndexedGlyphRun> Responses);

public static class DialoguePreviewText
{
    public const int SpeechWidth = 232;
    public const int MaximumSpeechLines = 5;
    public const int ResponseWidth = 296;
    public const int MaximumResponses = 5;

    public static DialoguePreviewTextContent Create(
        PackedIndexedBitmapFont font,
        FirstTyrDialogueProjection projection)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(projection);
        var speechSource = projection.SpeechVariants.FirstOrDefault(source =>
            source.Kind == GplDialogueTextSourceKind.Literal)
            ?? throw new InvalidDataException(
                "The dialogue preview has no literal opening speech source.");
        var speechLines = Wrap(font, speechSource.Text, SpeechWidth, MaximumSpeechLines);
        if (speechLines.Count == 0)
            throw new InvalidDataException("The dialogue preview speech is empty.");
        var speech = IndexedGlyphBlockRasterizer.Rasterize(font, speechLines);
        var responses = projection.InitialChoices
            .Where(choice => choice.Label.Kind == GplDialogueTextSourceKind.Literal)
            .Take(MaximumResponses)
            .Select(choice =>
            {
                var fitted = Fit(font, choice.Label.Text.Trim(), ResponseWidth);
                if (fitted.IsEmpty)
                    throw new InvalidDataException("A dialogue preview response is empty.");
                return IndexedGlyphRunRasterizer.Rasterize(font, fitted.Span);
            })
            .ToArray();
        return new(speech, responses);
    }

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
