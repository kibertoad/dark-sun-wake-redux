using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using DarkSunWakeRedux.Resources;
using RefurbishedDinosaurs.Core.Assets;

internal static class RdffKeyInspection
{
    internal const int ResourceLimit = 4096;
    private const int SourceLimit = 32 * 1024 * 1024;
    private const int RecordLimit = 1024 * 1024;
    private const int ControlsLimit = 64 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true
    };

    internal sealed record Controls
    {
        public required string SourceSha256 { get; init; }
        public required int KindOne { get; init; }
        public required int KindTwo { get; init; }
        public required int KindFive { get; init; }
    }

    internal sealed record Resource(uint Number, ReadOnlyMemory<byte> Data);
    internal sealed record Row(uint ResourceNumber, short FirstWord,
        bool ExactNegatedNumber, bool NegatedLowWord, int TrailingBytes);
    internal sealed record Census(int Resources, int KindOne, int KindTwo, int KindFive,
        int ExactMatches, int LowWordMatches, IReadOnlyList<Row> Rows);

    internal static Controls ParseControls(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is 0 or > ControlsLimit)
            throw new InvalidDataException("Controls must contain 1..65536 bytes.");
        var controls = JsonSerializer.Deserialize<Controls>(bytes, JsonOptions)
            ?? throw new InvalidDataException("Controls must be an object.");
        ValidateControls(controls);
        return controls;
    }

    private static void ValidateControls(Controls controls)
    {
        ValidateSourceHash(controls.SourceSha256);
        long count = (long)controls.KindOne + controls.KindTwo + controls.KindFive;
        if (controls.KindOne < 0 || controls.KindTwo < 0 || controls.KindFive < 1 ||
            count > ResourceLimit)
            throw new InvalidDataException("Kind controls must be nonnegative, select kind five, and total at most 4096.");
    }

    internal static string VerifySource(Stream source, Controls controls)
    {
        ValidateControls(controls);
        return VerifySource(source, controls.SourceSha256);
    }

    internal static void ValidateSourceHash(string? sourceHash)
    {
        if (sourceHash is not { Length: 64 } hash ||
            !hash.All(value => value is >= '0' and <= '9' or >= 'a' and <= 'f'))
            throw new InvalidDataException("Controls require a lowercase source SHA-256.");
    }

    internal static string VerifySource(Stream source, string expectedHash)
    {
        ValidateSourceHash(expectedHash);
        if (!source.CanSeek || source.Length is 0 or > SourceLimit)
            throw new InvalidDataException("Source must be a seekable file of 1..32 MiB.");
        source.Position = 0;
        string hash = Convert.ToHexStringLower(SHA256.HashData(source));
        if (hash != expectedHash)
            throw new InvalidDataException("Source SHA-256 does not match controls; archive was not interpreted.");
        source.Position = 0;
        return hash;
    }

    // FMT-ACTOR-002 and FMT-PARTY-006: first-chunk metadata only.
    // GFF directory/index parsing stays in the existing bounded archive reader.
    internal static Census Analyze(IEnumerable<Resource> input, Controls controls)
    {
        ValidateControls(controls);
        var resources = input.Take(ResourceLimit + 1).OrderBy(value => value.Number).ToArray();
        if (resources.Length > ResourceLimit)
            throw new InvalidDataException("RDFF resource limit exceeded.");
        if (resources.Select(value => value.Number).Distinct().Count() != resources.Length)
            throw new InvalidDataException("Duplicate RDFF resource identifier.");
        int one = 0, two = 0, five = 0;
        var rows = new List<Row>();
        foreach (var resource in resources)
        {
            var data = resource.Data.Span;
            if (data.Length is < 10 or > RecordLimit)
                throw new InvalidDataException($"RDFF/{resource.Number}: resource size outside bounds.");
            if (data[0] != 1)
                throw new InvalidDataException($"RDFF/{resource.Number}: unsupported first chunk type.");
            int width = data[2] switch
            {
                1 => 23,
                2 => 49,
                5 => 23,
                _ => throw new InvalidDataException($"RDFF/{resource.Number}: unsupported first kind.")
            };
            int length = BinaryPrimitives.ReadUInt16LittleEndian(data[8..10]);
            if (length != width || length > data.Length - 10)
                throw new InvalidDataException($"RDFF/{resource.Number}: invalid first data extent or width.");
            if (data[2] == 1) { one++; continue; }
            if (data[2] == 2) { two++; continue; }
            five++;
            int end = 10 + length;
            if (data.Length - end < 10 || data[end] != 0xFF)
                throw new InvalidDataException($"RDFF/{resource.Number}: kind five lacks its next complete end header.");
            short word = BinaryPrimitives.ReadInt16LittleEndian(data[10..12]);
            rows.Add(new Row(resource.Number, word, (long)word == -(long)resource.Number,
                unchecked((ushort)word) == unchecked((ushort)(0u - resource.Number)),
                data.Length - end - 10));
        }
        if (one != controls.KindOne || two != controls.KindTwo || five != controls.KindFive)
            throw new InvalidDataException("First-kind counts missed explicit controls.");
        return new Census(resources.Length, one, two, five,
            rows.Count(value => value.ExactNegatedNumber), rows.Count(value => value.NegatedLowWord), rows);
    }

    internal static int Run(string path, string controlsPath)
    {
        try
        {
            using var controlsFile = File.OpenRead(controlsPath);
            if (controlsFile.Length is 0 or > ControlsLimit)
                throw new InvalidDataException("Controls must contain 1..65536 bytes.");
            var controlsBytes = new byte[(int)controlsFile.Length];
            controlsFile.ReadExactly(controlsBytes);
            var controls = ParseControls(controlsBytes);
            using var source = File.OpenRead(path);
            string sourceSha256 = VerifySource(source, controls);
            var archive = GffArchive.Read(source, path);
            var entries = archive.Resources.Where(value => value.Tag == "RDFF")
                .Take(ResourceLimit + 1).ToArray();
            if (entries.Length > ResourceLimit)
                throw new InvalidDataException("RDFF resource limit exceeded.");
            var census = Analyze(entries.Select(value =>
                new Resource(value.Number, archive.GetResource(value.Tag, value.Number))), controls);
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                schema = 1, sourceSha256,
                controlsSha256 = Convert.ToHexStringLower(SHA256.HashData(controlsBytes)),
                controls, census,
                limits = "First chunks only; kind-five next end header and trailing extent. No native admission, semantic identity or full-chain claim."
            }, JsonOptions));
            return 0;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or
            UnauthorizedAccessException or ArgumentException or JsonException or KeyNotFoundException)
        {
            Console.Error.WriteLine($"[rdff_key_census_unavailable] {error.Message}");
            return 2;
        }
    }
}
