using System.Security.Cryptography;
using System.Text.Json;
using DarkSunWakeRedux.Resources;

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
                maxHeight = image.Frames.Count == 0 ? 0 : image.Frames.Max(frame => frame.Height)
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
