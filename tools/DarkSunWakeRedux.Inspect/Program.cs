using System.Security.Cryptography;
using System.Text.Json;
using DarkSunWakeRedux.Resources;

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
