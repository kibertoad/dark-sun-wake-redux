using System.Security.Cryptography;
using System.Text.Json;
using DarkSunWakeRedux.Resources;

if (args.Length == 2 && args[0].Equals("character-catalog", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var characters = GffCharacterCatalog.Read(archive, args[1]);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            characterCount = characters.Count,
            characters
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"[character_catalog_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 2 && args[0].Equals("text-catalog", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var texts = archive.Resources.Where(resource => resource.Tag == "TEXT").Select(resource =>
        {
            var text = GffTextResource.Read(archive.GetResource(resource.Tag, resource.Number),
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            return new { resource.Number, resource.Size, lineCount = text.Lines.Count,
                maximumLineLength = text.Lines.Count == 0 ? 0 : text.Lines.Max(line => line.Length) };
        }).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]), textCount = texts.Length,
            totalLines = texts.Sum(text => text.lineCount), texts
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"[text_catalog_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 2 && args[0].Equals("font-catalog", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var fonts = archive.Resources.Where(resource => resource.Tag == "FONT").Select(resource =>
        {
            var font = IndexedBitmapFont.Read(archive.GetResource(resource.Tag, resource.Number),
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            return new
            {
                resource.Number,
                resource.Size,
                glyphCount = font.Glyphs.Count,
                glyphHeight = font.Glyphs[0].Height,
                maximumGlyphWidth = font.Glyphs.Max(glyph => glyph.Width),
                identityCharacterMappings = font.CharacterMap
                    .Select((glyph, character) => glyph == character)
                    .Count(matches => matches),
                distinctMappedGlyphs = font.CharacterMap.Distinct().Count(),
                characterMapSha256 = Convert.ToHexString(
                    SHA256.HashData(font.CharacterMap.ToArray())).ToLowerInvariant(),
                distinctPixelIndices = font.Glyphs.SelectMany(glyph => glyph.Pixels)
                    .Distinct().Count(),
                minimumPixelIndex = font.Glyphs.SelectMany(glyph => glyph.Pixels)
                    .DefaultIfEmpty().Min(),
                maximumPixelIndex = font.Glyphs.SelectMany(glyph => glyph.Pixels)
                    .DefaultIfEmpty().Max()
            };
        }).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            fontCount = fonts.Length,
            fonts
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"[font_catalog_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 2 && args[0].Equals("ui-catalog", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var windows = archive.Resources.Where(resource => resource.Tag == "WIND").Select(resource =>
        {
            var window = UiWindowResource.Read(archive.GetResource(resource.Tag, resource.Number),
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            if (window.ResourceNumber != resource.Number)
                throw new InvalidDataException($"{args[1]}:{resource.Tag}#{resource.Number}: embedded resource number is {window.ResourceNumber}.");
            if (window.ImageResourceNumber != 0 &&
                !archive.Resources.Any(candidate => candidate.Tag == "BMP " && candidate.Number == window.ImageResourceNumber))
                throw new InvalidDataException($"{args[1]}:{resource.Tag}#{resource.Number}: BMP #{window.ImageResourceNumber} does not exist.");
            foreach (var child in window.Children)
                if (!archive.Resources.Any(candidate => candidate.Tag == child.Tag && candidate.Number == child.ResourceNumber))
                    throw new InvalidDataException($"{args[1]}:{resource.Tag}#{resource.Number}: child {child.Tag}#{child.ResourceNumber} does not exist.");
            return window;
        }).ToArray();
        var buttons = archive.Resources.Where(resource => resource.Tag == "BUTN").Select(resource =>
        {
            var button = UiButtonResource.Read(archive.GetResource(resource.Tag, resource.Number),
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            if (button.ResourceNumber != resource.Number)
                throw new InvalidDataException($"{args[1]}:{resource.Tag}#{resource.Number}: embedded resource number is {button.ResourceNumber}.");
            if (button.ImageResourceNumber != 0 &&
                !archive.Resources.Any(candidate => candidate.Tag == "ICON" && candidate.Number == button.ImageResourceNumber))
                throw new InvalidDataException($"{args[1]}:{resource.Tag}#{resource.Number}: ICON #{button.ImageResourceNumber} does not exist.");
            return button;
        }).ToArray();
        var applicationFrames = archive.Resources.Where(resource => resource.Tag == "APFM").Select(resource =>
        {
            var frame = UiApplicationFrameResource.Read(archive.GetResource(resource.Tag, resource.Number),
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            if (frame.ResourceNumber != resource.Number)
                throw new InvalidDataException($"{args[1]}:{resource.Tag}#{resource.Number}: embedded resource number is {frame.ResourceNumber}.");
            return frame;
        }).ToArray();
        var editBoxes = archive.Resources.Where(resource => resource.Tag == "EBOX").Select(resource =>
        {
            var editBox = UiEditBoxResource.Read(archive.GetResource(resource.Tag, resource.Number),
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            if (editBox.ResourceNumber != resource.Number)
                throw new InvalidDataException($"{args[1]}:{resource.Tag}#{resource.Number}: embedded resource number is {editBox.ResourceNumber}.");
            return editBox;
        }).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            windowCount = windows.Length,
            buttonCount = buttons.Length,
            applicationFrameCount = applicationFrames.Length,
            editBoxCount = editBoxes.Length,
            windows,
            buttons,
            applicationFrames,
            editBoxes
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"[ui_catalog_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 2 && args[0].Equals("image-catalog", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var imageTags = new HashSet<string>(["BMP ", "CBMP", "ICON"], StringComparer.Ordinal);
        var images = archive.Resources.Where(resource => imageTags.Contains(resource.Tag)).Select(resource =>
        {
            var image = IndexedImage.Read(archive.GetResource(resource.Tag, resource.Number),
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            return new
            {
                resource.Tag,
                resource.Number,
                resource.Size,
                frameCount = image.Frames.Count,
                maxWidth = image.Frames.Count == 0 ? 0 : image.Frames.Max(frame => frame.Width),
                maxHeight = image.Frames.Count == 0 ? 0 : image.Frames.Max(frame => frame.Height),
                frames = image.Frames.Select(frame => new { frame.Width, frame.Height })
            };
        }).ToArray();
        var palettes = archive.Resources.Where(resource => resource.Tag == "PAL ").Select(resource =>
        {
            IndexedPalette.Read(archive.GetResource(resource.Tag, resource.Number).Span,
                $"{args[1]}:{resource.Tag}#{resource.Number}");
            return new { resource.Number, resource.Size };
        }).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            imageCount = images.Length,
            paletteCount = palettes.Length,
            images,
            palettes
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"[image_catalog_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 2 && args[0].Equals("gff", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            resourceCount = archive.Resources.Count,
            resources = archive.Resources
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"[gff_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect <owned-original-directory>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect gff <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect image-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect character-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect text-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect font-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect ui-catalog <owned-original.gff>");
    return 64;
}

var root = Path.GetFullPath(args[0]);
var files = new List<object>();
foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Order())
{
    await using var stream = File.OpenRead(path);
    files.Add(new
    {
        path = Path.GetRelativePath(root, path).Replace('\\', '/'),
        size = stream.Length,
        sha256 = Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant()
    });
}
Console.WriteLine(JsonSerializer.Serialize(new { root, files }, new JsonSerializerOptions { WriteIndented = true }));
return 0;
