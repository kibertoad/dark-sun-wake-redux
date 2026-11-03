using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using DarkSunWakeRedux.Resources;
using RefurbishedDinosaurs.Core.Assets;

internal static class RdffTypeInspection
{
    internal const int RelationLimit = 131072;
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
        public required int ResourceCount { get; init; }
        public required int InlineCount { get; init; }
        public required int TypeRows { get; init; }
        public required uint ControlResource { get; init; }
        public required ushort ControlReference { get; init; }
        public required bool ControlIndexedMatch { get; init; }
    }

    internal sealed record Row(uint ResourceNumber, ushort RecordReference,
        bool ReferenceInRange, bool IndexedMatch, IReadOnlyList<int> MatchingRows);
    internal sealed record Census(int Resources, int InlineRecords, int OutsideModel,
        int TypeRows, int IndexedMatches, int OtherRowMatches, int NoRowMatches,
        int OutOfRangeReferences, int MatchingRelations, IReadOnlyList<Row> Rows);

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
        RdffKeyInspection.ValidateSourceHash(controls.SourceSha256);
        if (controls.ResourceCount is < 1 or > RdffKeyInspection.ResourceLimit ||
            controls.InlineCount < 1 || controls.InlineCount > controls.ResourceCount ||
            controls.TypeRows is < 1 or > RdffKeyInspection.ResourceLimit)
            throw new InvalidDataException("Resource, inline and type-row controls must be positive and bounded by 4096.");
    }

    // FMT-ACTOR-002: the known 68-byte shape only. Shared rows remain opaque;
    // this does not implement the unknown field schema of FMT-ITEM-002.
    internal static Census Analyze(IEnumerable<RdffKeyInspection.Resource> input,
        ReadOnlyMemory<byte> typeTable, Controls controls)
    {
        ValidateControls(controls);
        if (typeTable.Length != controls.TypeRows * 15)
            throw new InvalidDataException("ALL/4 extent missed the explicit fifteen-byte row control.");
        var resources = input.Take(RdffKeyInspection.ResourceLimit + 1)
            .OrderBy(value => value.Number).ToArray();
        if (resources.Length > RdffKeyInspection.ResourceLimit)
            throw new InvalidDataException("RDFF resource limit exceeded.");
        if (resources.Select(value => value.Number).Distinct().Count() != resources.Length)
            throw new InvalidDataException("Duplicate RDFF resource identifier.");
        if (resources.Length != controls.ResourceCount)
            throw new InvalidDataException("RDFF resource-count control missed.");
        var rows = new List<Row>();
        int relations = 0;
        foreach (var resource in resources)
        {
            var data = resource.Data.Span;
            if (data.Length > 1024 * 1024)
                throw new InvalidDataException($"RDFF/{resource.Number}: resource size exceeds 1 MiB.");
            if (data.Length != 68) continue;
            if (data[0] != 1 || data[2] != 1 || data[3] != 0 ||
                Word(data, 6) != 0 || Word(data, 8) != 23 ||
                data[33] != 3 || data[34] != 0 || data[35] != 4 || data[36] != 0 ||
                Word(data, 39) != 5 || Word(data, 41) != 15 || data[58] != 0xFF)
                throw new InvalidDataException($"RDFF/{resource.Number}: unsupported 68-byte chunk shape.");
            ushort reference = Word(data, 37);
            var payload = data[43..58];
            var matching = new List<int>();
            for (int index = 0; index < controls.TypeRows; index++)
            {
                if (!payload.SequenceEqual(typeTable.Span.Slice(index * 15, 15))) continue;
                if (relations == RelationLimit)
                    throw new InvalidDataException("Matching-row relation limit exceeded; no partial census.");
                relations++;
                matching.Add(index);
            }
            bool inRange = reference < controls.TypeRows;
            rows.Add(new Row(resource.Number, reference, inRange,
                inRange && matching.Contains(reference), matching));
        }
        if (rows.Count != controls.InlineCount)
            throw new InvalidDataException("Fixed-shape inline-count control missed.");
        var control = rows.SingleOrDefault(value => value.ResourceNumber == controls.ControlResource);
        if (control is null || control.RecordReference != controls.ControlReference ||
            control.IndexedMatch != controls.ControlIndexedMatch)
            throw new InvalidDataException("Independent payload/reference control missed.");
        return new Census(resources.Length, rows.Count, resources.Length - rows.Count,
            controls.TypeRows, rows.Count(value => value.IndexedMatch),
            rows.Count(value => !value.IndexedMatch && value.MatchingRows.Count != 0),
            rows.Count(value => value.MatchingRows.Count == 0),
            rows.Count(value => !value.ReferenceInRange), relations, rows);
    }

    private static ushort Word(ReadOnlySpan<byte> data, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, 2));

    internal static int Run(string path, string controlsPath)
    {
        try
        {
            using var controlsFile = File.OpenRead(controlsPath);
            if (controlsFile.Length is 0 or > ControlsLimit)
                throw new InvalidDataException("Controls must contain 1..65536 bytes.");
            var bytes = new byte[(int)controlsFile.Length];
            controlsFile.ReadExactly(bytes);
            var controls = ParseControls(bytes);
            using var source = File.OpenRead(path);
            string sourceSha256 = RdffKeyInspection.VerifySource(source, controls.SourceSha256);
            var archive = GffArchive.Read(source, path);
            var entries = archive.Resources.Where(value => value.Tag == "RDFF")
                .Take(RdffKeyInspection.ResourceLimit + 1).ToArray();
            if (entries.Length > RdffKeyInspection.ResourceLimit)
                throw new InvalidDataException("RDFF resource limit exceeded.");
            var census = Analyze(entries.Select(value => new RdffKeyInspection.Resource(
                value.Number, archive.GetResource(value.Tag, value.Number))),
                archive.GetResource("ALL ", 4), controls);
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                schema = 1, sourceSha256,
                controlsSha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)), controls, census,
                limits = "Known 68-byte shape only; other sizes remain outside the model. Opaque byte comparison, not field semantics or native admission."
            }, JsonOptions));
            return 0;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or
            UnauthorizedAccessException or ArgumentException or JsonException or KeyNotFoundException)
        {
            Console.Error.WriteLine($"[rdff_type_census_unavailable] {error.Message}");
            return 2;
        }
    }
}
