using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record LaunchOptionsCanvas(int Width, int Height, byte[] Pixels);

/// <summary>
/// Draws the launch options screen (DEV-UI-001) into a 320x200 canvas of indices into
/// <see cref="Palette"/>, with the game's extracted interface font. The colours and the layout are
/// the rebuild's own. Text that does not fit its place throws, so the content smoke test catches a
/// font that would clip it.
/// </summary>
public static class LaunchOptionsRasterizer
{
    public const byte Background = 0;
    public const byte PanelFill = 1;
    public const byte Frame = 2;
    public const byte Text = 3;
    public const byte DimText = 4;
    public const byte HighlightFill = 5;
    public const byte HighlightText = 6;
    public const byte ButtonFill = 7;

    public const string Title = "Dark Sun: Wake of the Ravager Redux";
    public const string Subtitle = "Options of the rebuild";
    public const string ConfirmLabel = "Confirm";
    public const string ExitLabel = "Exit";
    public const string Hint = "Arrows choose, Enter changes, Esc quits";

    public static IReadOnlyList<Rgb24> Palette { get; } =
    [
        new(18, 13, 9),
        new(44, 31, 21),
        new(156, 106, 54),
        new(226, 204, 158),
        new(150, 128, 96),
        new(116, 64, 26),
        new(255, 244, 216),
        new(70, 47, 29)
    ];

    public static LaunchOptionsCanvas Rasterize(
        PackedIndexedBitmapFont font,
        LaunchOptionsSession session)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(session);
        var canvas = new Canvas(font);
        canvas.Fill(new(0, 0, LogicalCanvasTransform.LogicalWidth,
            LogicalCanvasTransform.LogicalHeight), Background);
        canvas.TextCentered(Title, LaunchOptionsLayout.TitleY, Text);
        canvas.TextCentered(Subtitle, LaunchOptionsLayout.SubtitleY, DimText);
        canvas.Fill(LaunchOptionsLayout.Panel, PanelFill);
        canvas.Outline(LaunchOptionsLayout.Panel, Frame);

        for (var index = 0; index < LaunchOptionsSession.Options.Count; index++)
        {
            var option = LaunchOptionsSession.Options[index];
            var row = LaunchOptionsLayout.Item(index);
            var selected = session.Selected == index;
            if (selected) canvas.Fill(row, HighlightFill);
            var color = selected ? HighlightText : Text;
            var y = row.Y + (row.Height - canvas.GlyphHeight) / 2;
            canvas.TextAt(LaunchOptionsSession.Label(option),
                LaunchOptionsLayout.OptionTextLeft, y, color);
            canvas.TextRightAligned(
                LaunchOptionsSession.ValueText(session.Settings, option),
                LaunchOptionsLayout.OptionTextRight, y, color);
        }

        var description = session.Selected < LaunchOptionsSession.Options.Count
            ? LaunchOptionsSession.Description(LaunchOptionsSession.Options[session.Selected])
            : session.Selected == LaunchOptionsSession.ConfirmItem
                ? ["Start the game with these options."]
                : (IReadOnlyList<string>)["Quit without starting the game."];
        for (var line = 0; line < description.Count; line++)
            canvas.TextAt(description[line], LaunchOptionsLayout.OptionTextLeft,
                LaunchOptionsLayout.DescriptionTop + line * LaunchOptionsLayout.DescriptionLineHeight,
                DimText, LaunchOptionsLayout.Panel);

        Button(LaunchOptionsSession.ConfirmItem, ConfirmLabel);
        Button(LaunchOptionsSession.ExitItem, ExitLabel);
        canvas.TextCentered(Hint, LaunchOptionsLayout.HintY, DimText);
        return new(LogicalCanvasTransform.LogicalWidth, LogicalCanvasTransform.LogicalHeight,
            canvas.Pixels);

        void Button(int item, string label)
        {
            var bounds = LaunchOptionsLayout.Item(item);
            var selected = session.Selected == item;
            canvas.Fill(bounds, selected ? HighlightFill : ButtonFill);
            canvas.Outline(bounds, Frame);
            var width = canvas.Measure(label);
            canvas.TextAt(label, bounds.X + (bounds.Width - width) / 2,
                bounds.Y + (bounds.Height - canvas.GlyphHeight) / 2,
                selected ? HighlightText : Text, bounds);
        }
    }

    private sealed class Canvas
    {
        private const int Width = LogicalCanvasTransform.LogicalWidth;
        private const int Height = LogicalCanvasTransform.LogicalHeight;
        private readonly PackedIndexedBitmapFont _font;

        public Canvas(PackedIndexedBitmapFont font)
        {
            _font = font;
            if (font.Glyphs.Count != IndexedBitmapFont.CharacterCount ||
                font.CharacterMap.Count != IndexedBitmapFont.CharacterCount)
                throw new InvalidDataException(
                    $"The launch options font must map {IndexedBitmapFont.CharacterCount} glyphs.");
            GlyphHeight = font.Glyphs[0].Height;
            if (GlyphHeight is <= 0 or > LaunchOptionsLayout.MaximumTextHeight)
                throw new InvalidDataException(
                    $"The launch options font height {GlyphHeight} does not fit its rows.");
        }

        public byte[] Pixels { get; } = new byte[Width * Height];
        public int GlyphHeight { get; }

        public void Fill(LaunchOptionsBounds bounds, byte color)
        {
            for (var y = bounds.Y; y < bounds.Y + bounds.Height; y++)
                Pixels.AsSpan(y * Width + bounds.X, bounds.Width).Fill(color);
        }

        public void Outline(LaunchOptionsBounds bounds, byte color)
        {
            Fill(bounds with { Height = 1 }, color);
            Fill(bounds with { Y = bounds.Y + bounds.Height - 1, Height = 1 }, color);
            Fill(bounds with { Width = 1 }, color);
            Fill(bounds with { X = bounds.X + bounds.Width - 1, Width = 1 }, color);
        }

        public int Measure(string text)
        {
            var width = 0;
            foreach (var glyph in Encode(text)) width += _font.Glyphs[glyph].Width;
            return width;
        }

        public void TextCentered(string text, int y, byte color) =>
            TextAt(text, (Width - Measure(text)) / 2, y, color);

        public void TextRightAligned(string text, int right, int y, byte color) =>
            TextAt(text, right - Measure(text), y, color);

        public void TextAt(string text, int x, int y, byte color, LaunchOptionsBounds? within = null)
        {
            var run = IndexedGlyphRunRasterizer.Rasterize(_font, Encode(text));
            var area = within ?? new LaunchOptionsBounds(0, 0, Width, Height);
            if (x < area.X || y < area.Y ||
                x + run.Width > area.X + area.Width || y + run.Height > area.Y + area.Height)
                throw new InvalidDataException(
                    $"The launch options text '{text}' does not fit its place on the screen.");
            for (var row = 0; row < run.Height; row++)
            for (var column = 0; column < run.Width; column++)
                if (run.Pixels[row * run.Width + column] != 0)
                    Pixels[(y + row) * Width + x + column] = color;
        }

        private byte[] Encode(string text)
        {
            var glyphs = new byte[text.Length];
            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];
                if (character > byte.MaxValue)
                    throw new InvalidDataException(
                        $"Launch options character U+{(int)character:X4} is outside the font map.");
                glyphs[index] = _font.CharacterMap[character];
            }
            return glyphs;
        }
    }
}
