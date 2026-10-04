using System.Reflection;
using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using RefurbishedDinosaurs.Core.Assets;
using RefurbishedDinosaurs.LegacyFormats;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    try
    {
        if (args.Length == 0 || args[0] is "--help" or "-h") return Usage();
        var command = args[0].ToLowerInvariant();
        var requestedOutput = Option(args, "--output");
        var editions = LoadManifests();

        if (command == "list-editions")
        {
            foreach (var edition in editions) Console.WriteLine(edition.SourceEdition);
            return 0;
        }
        if (command == "verify-pack")
        {
            // Resolved only here: the per-user default fails where no local application data folder
            // exists, which must not stop the commands that never touch the pack.
            var packOutput = requestedOutput ?? OriginalContent.DefaultAssetPackPath();
            var verification = await OriginalContent.VerifyInstalledAsync(packOutput);
            if (!verification.IsValid)
            {
                foreach (var issue in verification.Issues)
                    Console.Error.WriteLine($"[{issue.Problem}] {issue.Detail}");
                return 3;
            }
            Console.WriteLine($"Verified asset pack at {packOutput}");
            return 0;
        }

        var source = Option(args, "--source");
        if (string.IsNullOrWhiteSpace(source))
            return Fail("source_required", "--source must name a legally owned GOG installation.", 64);

        var identification = await AssetVerifier.IdentifyAsync(source, editions);
        if (!identification.IsSupported)
        {
            if (identification.IsAmbiguous)
            {
                // Two edition manifests describe this copy; they need a file that tells them apart.
                Console.Error.WriteLine("[edition_ambiguous] The selected source matches more than one edition: " +
                    string.Join(", ", identification.Matches.Select(edition => edition.SourceEdition)) + ".");
                return 2;
            }
            Console.Error.WriteLine("The selected directory does not match a supported edition.");
            if (identification.Mismatches.Count == 0)
                Console.Error.WriteLine("[source_editions_missing] The Extractor contains no supported-edition manifests.");
            foreach (var mismatch in identification.Mismatches)
            foreach (var issue in mismatch.Issues)
                Console.Error.WriteLine($"[{issue.Problem}] {mismatch.Edition.SourceEdition}: " +
                    (issue.Path is null ? issue.Detail : $"{issue.Path}: {issue.Detail}"));
            return 2;
        }
        var supported = identification.Edition!;
        if (command == "verify-source")
        {
            Console.WriteLine($"Verified {supported.SourceEdition}.");
            Console.WriteLine($"Source fingerprint: {supported.Fingerprint()}");
            return 0;
        }
        if (command == "inventory-source")
        {
            var inventory = SourceCorpusInventory.Read(source, supported);
            foreach (var group in inventory.Records.GroupBy(record => record.Disposition)
                         .OrderBy(group => group.Key))
                Console.WriteLine($"{group.Key}: {group.Count()} file(s)");
            var unrepresented = inventory.Records.Where(record =>
                record.Disposition == SourceCorpusDisposition.Unrepresented).ToArray();
            foreach (var record in unrepresented)
                Console.Error.WriteLine($"[source_unrepresented] {record.Path}");
            return unrepresented.Length == 0 ? 0 : 3;
        }
        if (command != "extract") return Usage();

        var output = requestedOutput ?? OriginalContent.DefaultAssetPackPath();
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        var manifest = await AssetPackInstaller.InstallAsync(output, staging =>
            StartupAssetExtractor.WritePackAsync(source, staging, supported, version));
        Console.WriteLine($"Installed verified asset pack for {manifest.SourceEdition} at {output}.");
        Console.WriteLine($"Extracted {manifest.Files.Count} file(s) transactionally.");
        return 0;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"[extractor_failed] Asset Extractor failed safely: {exception.Message}");
        return 1;
    }
}

static AssetManifest[] LoadManifests()
{
    var assembly = Assembly.GetExecutingAssembly();
    return assembly.GetManifestResourceNames()
        .Where(name => name.EndsWith(".json", StringComparison.Ordinal))
        .Order(StringComparer.Ordinal)
        .Select(name =>
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            return OriginalContent.LoadEdition(stream);
        })
        .ToArray();
}

static int Fail(string code, string message, int exitCode)
{
    Console.Error.WriteLine($"[{code}] {message}");
    return exitCode;
}

static string? Option(string[] args, string name)
{
    var index = Array.FindIndex(args, value => value.Equals(name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

static int Usage()
{
    Console.WriteLine("Dark Sun: Wake of the Ravager Redux Asset Extractor");
    Console.WriteLine("A legally owned supported GOG copy is required. The original installation is read-only.");
    Console.WriteLine("  list-editions");
    Console.WriteLine("  verify-source --source <gog-installation>");
    Console.WriteLine("  inventory-source --source <gog-installation>");
    Console.WriteLine("  extract --source <gog-installation> [--output <asset-pack>]");
    Console.WriteLine("  verify-pack [--output <asset-pack>]");
    return 64;
}
