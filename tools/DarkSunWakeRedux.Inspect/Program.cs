using System.Security.Cryptography;
using System.Text.Json;
using DarkSunWakeRedux.Resources;

if (args.Length == 3 && args[0].Equals("resource-pattern", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var archivePath = Path.GetFullPath(args[1]);
        await using var stream = File.OpenRead(archivePath);
        var archive = GffArchive.Read(stream, archivePath);
        var matches = GffResourcePatternLocator.FindAscii(archive, args[2]);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            archivePath,
            matchCount = matches.Count,
            matches = matches.Select(match => new
            {
                match.Tag,
                resourceNumber = match.ResourceNumber,
                resourceSize = match.ResourceSize,
                patternOffset = match.PatternOffset
            })
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException)
    {
        Console.Error.WriteLine($"[resource_pattern_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 5 && args[0].Equals("object-pattern-overlap", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        if (!uint.TryParse(args[2], out var objectNumber))
            throw new ArgumentException("Object resource number must be an unsigned integer.", nameof(args));
        var tag = args[3].Replace('_', ' ');
        if (tag.Length != 4 || !tag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var patternResources = GffResourcePatternLocator.FindAscii(archive, args[4])
            .Where(match => match.Tag == tag).Select(match => match.ResourceNumber)
            .ToHashSet();
        if (patternResources.Count == 0)
            throw new InvalidDataException($"{args[1]} has no {tag} resource containing the requested pattern.");
        var entry = GffObjectFrameCatalog.Read(archive, [objectNumber], args[1]).Entries.Single();
        var words = ObjectResourcePatternOverlap.Match(entry.RawWord0,
            entry.RawWord6, entry.RawWord8, entry.RawWord10, patternResources)
            .Select(match => new
        {
            offset = match.Offset,
            matchesPatternResource = match.MatchesPatternResource
        });
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            archivePath = Path.GetFullPath(args[1]),
            objectNumber,
            tag,
            patternMatchingResourceCount = patternResources.Count,
            words
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException)
    {
        Console.Error.WriteLine($"[object_pattern_overlap_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length is 6 or 7 && args[0].Equals("image-preview", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var archivePath = Path.GetFullPath(args[1]);
        var tag = args[2].Replace('_', ' ');
        if (tag.Length != 4 || !tag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Image tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        if (!uint.TryParse(args[3], out var imageNumber) ||
            !uint.TryParse(args[4], out var paletteNumber))
            throw new ArgumentException("Image and palette resource numbers must be unsigned integers.", nameof(args));
        int? requestedFrame = null;
        if (args.Length == 7)
        {
            if (!int.TryParse(args[5], out var frameIndex) || frameIndex < 0)
                throw new ArgumentException("Frame index must be a nonnegative integer.", nameof(args));
            requestedFrame = frameIndex;
        }
        var outputPath = Path.GetFullPath(args[^1]);
        var workingDirectory = Path.GetFullPath(Environment.CurrentDirectory) + Path.DirectorySeparatorChar;
        if (outputPath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Preview output must be outside the repository working directory.", nameof(args));
        await using var stream = File.OpenRead(archivePath);
        var archive = GffArchive.Read(stream, archivePath);
        var image = IndexedImage.Read(archive.GetResource(tag, imageNumber),
            $"{archivePath}:{tag}#{imageNumber}");
        if (requestedFrame is null && image.Frames.Count != 1)
            throw new InvalidDataException($"{tag}#{imageNumber} contains {image.Frames.Count} frames; specify a frame index.");
        var selectedFrameIndex = requestedFrame ?? 0;
        var frame = IndexedImageFrameSelection.Select(image, selectedFrameIndex);
        var palette = IndexedPalette.Read(archive.GetResource("PAL ", paletteNumber).Span,
            $"{archivePath}:PAL #{paletteNumber}");
        WritePreviewBmp(outputPath, frame, palette);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = outputPath,
            tag,
            imageNumber,
            paletteNumber,
            frameIndex = selectedFrameIndex,
            frame.Width,
            frame.Height
        }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException or KeyNotFoundException)
    {
        Console.Error.WriteLine($"[image_preview_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length is 2 or 3 && args[0].Equals("object-catalog", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var objectStream = File.OpenRead(args[1]);
        var objectArchive = GffArchive.Read(objectStream, args[1]);
        IEnumerable<uint>? requiredNumbers = null;
        string? regionName = null;
        if (args.Length == 3)
        {
            await using var regionStream = File.OpenRead(args[2]);
            var regionArchive = GffArchive.Read(regionStream, args[2]);
            var region = GffRegion.Read(regionArchive, objectArchive, args[2]);
            regionName = region.Name;
            requiredNumbers = region.Entities.Select(entity => entity.ObjectResourceNumber);
        }
        var catalog = GffObjectFrameCatalog.Read(objectArchive, requiredNumbers, args[1]);
        var images = catalog.Entries.GroupBy(entry => entry.ImageResourceNumber)
            .Select(group => group.First().Image).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            region = regionName,
            definitionCount = catalog.Entries.Count,
            imageCount = images.Length,
            frameCount = images.Sum(image => image.Frames.Count),
            maximumFrameWidth = images.SelectMany(image => image.Frames).Max(frame => frame.Width),
            maximumFrameHeight = images.SelectMany(image => image.Frames).Max(frame => frame.Height),
            distinctRawWord0 = catalog.Entries.Select(entry => entry.RawWord0).Distinct().Count(),
            distinctRawWord6 = catalog.Entries.Select(entry => entry.RawWord6).Distinct().Count(),
            distinctRawWord8 = catalog.Entries.Select(entry => entry.RawWord8).Distinct().Count(),
            distinctRawWord10 = catalog.Entries.Select(entry => entry.RawWord10).Distinct().Count()
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or KeyNotFoundException)
    {
        Console.Error.WriteLine($"[object_catalog_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 3 && args[0].Equals("region-catalog", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var regionStream = File.OpenRead(args[1]);
        await using var objectStream = File.OpenRead(args[2]);
        var regionArchive = GffArchive.Read(regionStream, args[1]);
        var objectArchive = GffArchive.Read(objectStream, args[2]);
        var region = GffRegion.Read(regionArchive, objectArchive, args[1]);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            region.ResourceNumber,
            region.Name,
            mapWidth = GffRegion.TileColumns,
            mapHeight = GffRegion.TileRows,
            tileCount = region.Tiles.Count,
            usedTileCount = region.TileMap.Distinct().Count(),
            geometryValueCount = region.GeometryMap.Distinct().Count(),
            entityCount = region.Entities.Count,
            referencedObjectCount = region.Entities.Select(entity => entity.ObjectResourceNumber)
                .Distinct().Count()
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or KeyNotFoundException)
    {
        Console.Error.WriteLine($"[region_catalog_unreadable] {exception.Message}");
        return 2;
    }
}

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
        var imageTags = new HashSet<string>(["BMP ", "CBMP", "ICON", "PORT"], StringComparer.Ordinal);
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

if (args.Length == 2 && args[0].Equals("resource-inventory", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var tags = archive.Resources.GroupBy(resource => resource.Tag)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new
            {
                tag = group.Key,
                resourceCount = group.Count(),
                totalBytes = group.Sum(resource => (long)resource.Size),
                minimumResourceNumber = group.Min(resource => resource.Number),
                maximumResourceNumber = group.Max(resource => resource.Number),
                distinctResourceNumbers = group.Select(resource => resource.Number).Distinct().Count()
            });
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = Path.GetFullPath(args[1]),
            resourceCount = archive.Resources.Count,
            tags
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"[resource_inventory_unreadable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 5 && args[0].Equals("record-profile", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var tag = args[2].Replace('_', ' ');
        if (tag.Length != 4 || !tag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        if (!uint.TryParse(args[3], out var resourceNumber))
            throw new ArgumentException("Resource number must be an unsigned integer.", nameof(args));
        if (!int.TryParse(args[4], out var recordWidth))
            throw new ArgumentException("Candidate record width must be an integer.", nameof(args));
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var profile = OpaqueRecordProfile.Create(archive.GetResource(tag, resourceNumber),
            recordWidth, $"{args[1]}:{tag}#{resourceNumber}");
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            archivePath = Path.GetFullPath(args[1]),
            tag,
            resourceNumber,
            profile
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException or KeyNotFoundException)
    {
        Console.Error.WriteLine($"[record_profile_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 6 && args[0].Equals("resource-word-overlap", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var sourceTag = args[2].Replace('_', ' ');
        var targetTag = args[5].Replace('_', ' ');
        if (sourceTag.Length != 4 || !sourceTag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Source resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        if (targetTag.Length != 4 || !targetTag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Target resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        if (!uint.TryParse(args[3], out var sourceResourceNumber))
            throw new ArgumentException("Source resource number must be an unsigned integer.", nameof(args));

        var sourceArchivePath = Path.GetFullPath(args[1]);
        var targetArchivePath = Path.GetFullPath(args[4]);
        await using var sourceStream = File.OpenRead(sourceArchivePath);
        await using var targetStream = File.OpenRead(targetArchivePath);
        var sourceArchive = GffArchive.Read(sourceStream, sourceArchivePath);
        var targetArchive = GffArchive.Read(targetStream, targetArchivePath);
        var targetResourceNumbers = targetArchive.Resources
            .Where(resource => resource.Tag == targetTag)
            .Select(resource => resource.Number)
            .ToHashSet();
        var overlap = OpaqueResourceWordOverlap.FindLittleEndianMatches(
            sourceArchive.GetResource(sourceTag, sourceResourceNumber), targetResourceNumbers,
            $"{sourceArchivePath}:{sourceTag}#{sourceResourceNumber}");
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            sourceArchivePath,
            sourceTag,
            sourceResourceNumber,
            targetArchivePath,
            targetTag,
            targetResourceCount = targetResourceNumbers.Count,
            overlap.SourceByteLength,
            overlap.CandidateWordCount,
            overlap.MatchingOffsets
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException or KeyNotFoundException)
    {
        Console.Error.WriteLine($"[resource_word_overlap_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 9 && args[0].Equals("lane-word-namespace-profile", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var sourceTag = args[2].Replace('_', ' ');
        var targetTag = args[8].Replace('_', ' ');
        if (sourceTag.Length != 4 || !sourceTag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Source resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        if (targetTag.Length != 4 || !targetTag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Target resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        if (!uint.TryParse(args[3], out var sourceResourceNumber))
            throw new ArgumentException("Source resource number must be an unsigned integer.", nameof(args));
        if (!int.TryParse(args[4], out var recordWidth) ||
            !int.TryParse(args[5], out var laneWidth) ||
            !int.TryParse(args[6], out var wordOffset))
            throw new ArgumentException("Record width, lane width, and word offset must be integers.", nameof(args));

        var sourceArchivePath = Path.GetFullPath(args[1]);
        var targetArchivePath = Path.GetFullPath(args[7]);
        await using var sourceStream = File.OpenRead(sourceArchivePath);
        await using var targetStream = File.OpenRead(targetArchivePath);
        var sourceArchive = GffArchive.Read(sourceStream, sourceArchivePath);
        var targetArchive = GffArchive.Read(targetStream, targetArchivePath);
        var targetResourceNumbers = targetArchive.Resources
            .Where(resource => resource.Tag == targetTag)
            .Select(resource => resource.Number)
            .ToHashSet();
        var profile = OpaqueLaneWordNamespaceProfiler.Create(
            sourceArchive.GetResource(sourceTag, sourceResourceNumber), recordWidth,
            laneWidth, wordOffset, targetResourceNumbers,
            $"{sourceArchivePath}:{sourceTag}#{sourceResourceNumber}");
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            sourceArchivePath,
            sourceTag,
            sourceResourceNumber,
            targetArchivePath,
            targetTag,
            targetResourceCount = targetResourceNumbers.Count,
            profile
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException or KeyNotFoundException)
    {
        Console.Error.WriteLine($"[lane_word_namespace_profile_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 4 && args[0].Equals("pair-resource-overlap", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var tablePath = Path.GetFullPath(args[1]);
        var tag = args[3].Replace('_', ' ');
        if (tag.Length != 4 || !tag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        var tableLength = new FileInfo(tablePath).Length;
        if (tableLength is 0 or > 1_048_576 || tableLength % 4 != 0)
            throw new InvalidDataException(
                $"{tablePath}: pair table must be nonempty, at most 1 MiB, and a multiple of four bytes.");
        var bytes = await File.ReadAllBytesAsync(tablePath);
        var left = new uint[bytes.Length / 4];
        var right = new uint[left.Length];
        for (var index = 0; index < left.Length; index++)
        {
            left[index] = BitConverter.ToUInt16(bytes, index * 4);
            right[index] = BitConverter.ToUInt16(bytes, index * 4 + 2);
        }
        await using var stream = File.OpenRead(args[2]);
        var archive = GffArchive.Read(stream, args[2]);
        var resourceNumbers = archive.Resources.Where(resource => resource.Tag == tag)
            .Select(resource => resource.Number).ToHashSet();
        if (resourceNumbers.Count == 0)
            throw new InvalidDataException($"{args[2]} has no {tag} resources.");
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            pairTablePath = tablePath,
            archivePath = Path.GetFullPath(args[2]),
            tag,
            pairCount = left.Length,
            tagResourceCount = resourceNumbers.Count,
            leftMatches = left.Count(resourceNumbers.Contains),
            rightMatches = right.Count(resourceNumbers.Contains),
            eitherMatches = left.Zip(right).Count(pair =>
                resourceNumbers.Contains(pair.First) || resourceNumbers.Contains(pair.Second)),
            bothMatches = left.Zip(right).Count(pair =>
                resourceNumbers.Contains(pair.First) && resourceNumbers.Contains(pair.Second))
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException)
    {
        Console.Error.WriteLine($"[pair_resource_overlap_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length is 3 or 4 && args[0].Equals("object-word-overlap", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var tag = args[2].Replace('_', ' ');
        if (tag.Length != 4 || !tag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var resourceNumbers = archive.Resources.Where(resource => resource.Tag == tag)
            .Select(resource => resource.Number).ToHashSet();
        if (resourceNumbers.Count == 0)
            throw new InvalidDataException($"{args[1]} has no {tag} resources.");
        var objects = GffObjectFrameCatalog.Read(archive, sourceName: args[1]);
        uint[]? pairLeft = null;
        uint[]? pairRight = null;
        string? pairTablePath = null;
        if (args.Length == 4)
        {
            pairTablePath = Path.GetFullPath(args[3]);
            var pairTableLength = new FileInfo(pairTablePath).Length;
            if (pairTableLength is 0 or > 1_048_576 || pairTableLength % 4 != 0)
                throw new InvalidDataException(
                    $"{pairTablePath}: pair table must be nonempty, at most 1 MiB, and a multiple of four bytes.");
            var pairBytes = await File.ReadAllBytesAsync(pairTablePath);
            pairLeft = new uint[pairBytes.Length / 4];
            pairRight = new uint[pairLeft.Length];
            for (var index = 0; index < pairLeft.Length; index++)
            {
                pairLeft[index] = BitConverter.ToUInt16(pairBytes, index * 4);
                pairRight[index] = BitConverter.ToUInt16(pairBytes, index * 4 + 2);
            }
        }
        var words = new[]
        {
            new { offset = 0, values = objects.Entries.Select(entry => (uint)entry.RawWord0).ToArray() },
            new { offset = 6, values = objects.Entries.Select(entry => (uint)entry.RawWord6).ToArray() },
            new { offset = 8, values = objects.Entries.Select(entry => (uint)entry.RawWord8).ToArray() },
            new { offset = 10, values = objects.Entries.Select(entry => (uint)entry.RawWord10).ToArray() }
        }.Select(word => new
        {
            word.offset,
            matchingRecords = word.values.Count(resourceNumbers.Contains),
            distinctMatchingValues = word.values.Where(resourceNumbers.Contains).Distinct().Count(),
            distinctValues = word.values.Distinct().Count(),
            pairLeftValuesPresent = pairLeft is null ? (int?)null : pairLeft.Count(word.values.Contains),
            pairRightValuesPresent = pairRight is null ? (int?)null : pairRight.Count(word.values.Contains)
        });
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            archivePath = Path.GetFullPath(args[1]),
            tag,
            objectCount = objects.Entries.Count,
            tagResourceCount = resourceNumbers.Count,
            pairTablePath,
            pairCount = pairLeft?.Length,
            words
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException)
    {
        Console.Error.WriteLine($"[object_word_overlap_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length == 4 && args[0].Equals("object-record-overlap", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        if (!uint.TryParse(args[2], out var objectNumber))
            throw new ArgumentException("Object resource number must be an unsigned integer.", nameof(args));
        var tag = args[3].Replace('_', ' ');
        if (tag.Length != 4 || !tag.All(character => character is >= ' ' and <= '~'))
            throw new ArgumentException("Resource tag must be exactly four printable ASCII characters; use '_' for a padded space.", nameof(args));
        await using var stream = File.OpenRead(args[1]);
        var archive = GffArchive.Read(stream, args[1]);
        var resourceNumbers = archive.Resources.Where(resource => resource.Tag == tag)
            .Select(resource => resource.Number).ToHashSet();
        if (resourceNumbers.Count == 0)
            throw new InvalidDataException($"{args[1]} has no {tag} resources.");
        var entry = GffObjectFrameCatalog.Read(archive, [objectNumber], args[1]).Entries.Single();
        var words = new[]
        {
            new { offset = 0, value = entry.RawWord0 },
            new { offset = 6, value = entry.RawWord6 },
            new { offset = 8, value = entry.RawWord8 },
            new { offset = 10, value = entry.RawWord10 }
        }.Select(word => new { word.offset, matchesTagResource = resourceNumbers.Contains(word.value) });
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            archivePath = Path.GetFullPath(args[1]),
            objectNumber,
            tag,
            tagResourceCount = resourceNumbers.Count,
            words
        }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    catch (Exception exception) when (exception is InvalidDataException or IOException or
                                      UnauthorizedAccessException or ArgumentException)
    {
        Console.Error.WriteLine($"[object_record_overlap_unavailable] {exception.Message}");
        return 2;
    }
}

if (args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect <owned-original-directory>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect image-preview <owned-original.gff> <tag; use _ for padded space> <image-number> <palette-number> [frame-index] <outside-repository.bmp>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect gff <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect resource-inventory <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect resource-pattern <owned-original.gff> <printable-ascii-pattern>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect object-pattern-overlap <owned-object.gff> <object-number> <tag; use _ for padded space> <printable-ascii-pattern>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect record-profile <owned-original.gff> <tag; use _ for padded space> <resource-number> <candidate-record-width>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect resource-word-overlap <source.gff> <source-tag; use _ for padded space> <source-number> <target.gff> <target-tag; use _ for padded space>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect lane-word-namespace-profile <source.gff> <source-tag; use _ for padded space> <source-number> <record-width> <lane-width> <word-offset> <target.gff> <target-tag; use _ for padded space>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect pair-resource-overlap <owned-pair-table> <owned-original.gff> <tag; use _ for padded space>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect object-word-overlap <owned-object.gff> <tag; use _ for padded space> [owned-pair-table]");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect object-record-overlap <owned-object.gff> <object-number> <tag; use _ for padded space>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect image-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect character-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect text-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect font-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect ui-catalog <owned-original.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect region-catalog <owned-region.gff> <owned-objects.gff>");
    Console.Error.WriteLine("  DarkSunWakeRedux.Inspect object-catalog <owned-objects.gff> [owned-region.gff]");
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

static void WritePreviewBmp(string outputPath, IndexedImageFrame frame, IndexedPalette palette)
{
    ArgumentNullException.ThrowIfNull(frame);
    ArgumentNullException.ThrowIfNull(palette);
    var rowBytes = checked(frame.Width * 3);
    var paddedRowBytes = checked((rowBytes + 3) & ~3);
    var pixelBytes = checked(paddedRowBytes * frame.Height);
    var parent = Path.GetDirectoryName(outputPath);
    if (string.IsNullOrWhiteSpace(parent) || !Directory.Exists(parent))
        throw new DirectoryNotFoundException("Preview output directory does not exist.");
    using var stream = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write,
        FileShare.None);
    using var writer = new BinaryWriter(stream);
    writer.Write("BM"u8);
    writer.Write(checked(54 + pixelBytes));
    writer.Write(0);
    writer.Write(54);
    writer.Write(40);
    writer.Write(frame.Width);
    writer.Write(frame.Height);
    writer.Write((ushort)1);
    writer.Write((ushort)24);
    writer.Write(0);
    writer.Write(pixelBytes);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    var padding = new byte[paddedRowBytes - rowBytes];
    for (var y = frame.Height - 1; y >= 0; y--)
    {
        for (var x = 0; x < frame.Width; x++)
        {
            var index = y * frame.Width + x;
            var color = frame.Alpha[index] == 0 ? new Rgb24(255, 0, 255) :
                palette.Colors[frame.Pixels[index]];
            writer.Write(color.Blue);
            writer.Write(color.Green);
            writer.Write(color.Red);
        }
        writer.Write(padding);
    }
}
