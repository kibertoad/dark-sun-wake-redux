using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record GffResource(string Tag, uint Number, uint Offset, uint Size);

public sealed class GffArchive
{
    public const int HeaderSize = 28;
    public const long MaximumFileBytes = 512L * 1024 * 1024;
    public const int MaximumTagCount = 4_096;
    public const int MaximumResourceCount = 1_000_000;

    private readonly byte[] _bytes;

    private GffArchive(byte[] bytes, IReadOnlyList<GffResource> resources)
    {
        _bytes = bytes;
        Resources = resources;
    }

    public IReadOnlyList<GffResource> Resources { get; }

    public static GffArchive Read(Stream stream, string sourceName = "GFF stream")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderSize)
            throw Error(sourceName, $"is shorter than the {HeaderSize}-byte header");
        if (stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"exceeds the {MaximumFileBytes}-byte safety limit");

        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        return Parse(bytes, sourceName);
    }

    public ReadOnlyMemory<byte> GetResource(string tag, uint number)
    {
        var resource = Resources.SingleOrDefault(candidate =>
            candidate.Number == number && candidate.Tag.Equals(tag, StringComparison.Ordinal));
        if (resource is null)
            throw new KeyNotFoundException($"GFF resource '{tag}' #{number} does not exist.");
        return _bytes.AsMemory(checked((int)resource.Offset), checked((int)resource.Size));
    }

    private static GffArchive Parse(byte[] bytes, string sourceName)
    {
        var reader = new BoundedReader(bytes, sourceName);
        if (!reader.ReadTag(0).Equals("GFFI", StringComparison.Ordinal))
            throw Error(sourceName, "does not begin with the GFFI signature");
        var version = reader.UInt32(4);
        if (version != 0x0003_0000)
            throw Error(sourceName, $"uses unsupported GFF version 0x{version:X8}");
        var declaredHeaderSize = reader.UInt32(8);
        if (declaredHeaderSize != HeaderSize)
            throw Error(sourceName, $"declares unsupported header size {declaredHeaderSize}");
        var indexOffset = reader.UInt32(12);
        if (indexOffset < HeaderSize || indexOffset > (uint)(bytes.Length - 10))
            throw Error(sourceName, $"has out-of-range index offset {indexOffset}");

        var position = checked((int)indexOffset);
        reader.UInt32(position, "index prefix");
        reader.UInt32(position + 4, "index prefix");
        position += 8;
        var tagCount = reader.UInt16(position, "tag count");
        position += 2;
        if (tagCount is 0 or > MaximumTagCount)
            throw Error(sourceName, $"declares invalid tag count {tagCount}");

        var primary = new Dictionary<string, List<GffResource>>(StringComparer.Ordinal);
        var secondary = new List<SecondaryDescriptor>();
        var totalResources = 0L;
        for (var tagIndex = 0; tagIndex < tagCount; tagIndex++)
        {
            var tag = reader.ReadTag(position);
            position += 4;
            if (primary.ContainsKey(tag) || secondary.Any(item => item.Tag == tag))
                throw Error(sourceName, $"contains duplicate tag table '{tag}'");
            var countMarker = reader.UInt32(position, $"{tag} entry count");
            position += 4;
            if (countMarker is > 0 and <= int.MaxValue)
            {
                var count = countMarker;
                EnsureCount(count, ref totalResources, sourceName, tag);
                var entries = new List<GffResource>(checked((int)count));
                for (var entryIndex = 0U; entryIndex < count; entryIndex++)
                {
                    var number = reader.UInt32(position, $"{tag} resource number");
                    var offset = reader.UInt32(position + 4, $"{tag} resource offset");
                    var size = reader.UInt32(position + 8, $"{tag} resource size");
                    position += 12;
                    ValidateRange(offset, size, bytes.Length, sourceName, tag, number);
                    entries.Add(new(tag, number, offset, size));
                }
                RejectDuplicateNumbers(entries, sourceName, tag);
                primary.Add(tag, entries);
                continue;
            }

            if ((countMarker & 0x8000_0000U) == 0)
                throw Error(sourceName, $"tag '{tag}' has an invalid zero entry marker");
            var markerCount = countMarker & 0x7fff_ffffU;
            var declaredSecondaryCount = reader.UInt32(position, $"{tag} secondary entry count");
            var gffiTableIndex = reader.UInt32(position + 4, $"{tag} secondary table index");
            var segmentCount = reader.UInt32(position + 8, $"{tag} numbering segment count");
            position += 12;
            if (markerCount != declaredSecondaryCount)
                throw Error(sourceName, $"tag '{tag}' has conflicting secondary entry counts {markerCount} and {declaredSecondaryCount}");
            if (segmentCount is 0 or > MaximumResourceCount)
                throw Error(sourceName, $"tag '{tag}' declares invalid numbering segment count {segmentCount}");
            var segments = new List<NumberingSegment>(checked((int)segmentCount));
            var expandedCount = 0L;
            for (var segmentIndex = 0U; segmentIndex < segmentCount; segmentIndex++)
            {
                var firstNumber = reader.UInt32(position, $"{tag} segment start");
                var segmentLength = reader.UInt32(position + 4, $"{tag} segment length");
                position += 8;
                if (segmentLength == 0 || firstNumber + (ulong)segmentLength - 1 > uint.MaxValue)
                    throw Error(sourceName, $"tag '{tag}' contains an invalid numbering segment");
                expandedCount += segmentLength;
                if (expandedCount > MaximumResourceCount)
                    throw Error(sourceName, $"tag '{tag}' exceeds the resource-count safety limit");
                segments.Add(new(firstNumber, segmentLength));
            }
            if (expandedCount != declaredSecondaryCount)
                throw Error(sourceName, $"tag '{tag}' numbering expands to {expandedCount} entries but declares {declaredSecondaryCount}");
            EnsureCount(checked((uint)expandedCount), ref totalResources, sourceName, tag);
            secondary.Add(new(tag, gffiTableIndex, segments, checked((int)expandedCount)));
        }

        primary.TryGetValue("GFFI", out var gffiEntries);
        if (secondary.Count != 0 && gffiEntries is null)
            throw Error(sourceName, "uses secondary tables without a primary GFFI table");

        var resources = primary.Values.SelectMany(value => value).ToList();
        foreach (var descriptor in secondary)
        {
            if (descriptor.TableIndex >= gffiEntries!.Count)
                throw Error(sourceName, $"tag '{descriptor.Tag}' references missing GFFI table index {descriptor.TableIndex}");
            var table = gffiEntries[checked((int)descriptor.TableIndex)];
            var tablePosition = checked((int)table.Offset);
            var count = reader.UInt32(tablePosition, $"{descriptor.Tag} secondary entry count");
            tablePosition += 4;
            if (count != descriptor.ExpandedCount)
                throw Error(sourceName, $"tag '{descriptor.Tag}' numbering expands to {descriptor.ExpandedCount} entries but its table contains {count}");
            if (4UL + count * 8UL > table.Size)
                throw Error(sourceName, $"tag '{descriptor.Tag}' secondary table exceeds its GFFI resource bounds");

            var numbers = ExpandNumbers(descriptor.Segments);
            var entries = new List<GffResource>(descriptor.ExpandedCount);
            for (var index = 0; index < descriptor.ExpandedCount; index++)
            {
                var offset = reader.UInt32(tablePosition, $"{descriptor.Tag} resource offset");
                var size = reader.UInt32(tablePosition + 4, $"{descriptor.Tag} resource size");
                tablePosition += 8;
                ValidateRange(offset, size, bytes.Length, sourceName, descriptor.Tag, numbers[index]);
                entries.Add(new(descriptor.Tag, numbers[index], offset, size));
            }
            RejectDuplicateNumbers(entries, sourceName, descriptor.Tag);
            resources.AddRange(entries);
        }

        RejectPartialOverlaps(resources, sourceName);
        return new(bytes, resources.OrderBy(item => item.Offset).ThenBy(item => item.Tag, StringComparer.Ordinal)
            .ThenBy(item => item.Number).ToArray());
    }

    private static List<uint> ExpandNumbers(IReadOnlyList<NumberingSegment> segments)
    {
        var result = new List<uint>();
        foreach (var segment in segments)
            for (var index = 0U; index < segment.Length; index++) result.Add(segment.First + index);
        return result;
    }

    private static void EnsureCount(uint count, ref long total, string sourceName, string tag)
    {
        if (count > MaximumResourceCount || (total += count) > MaximumResourceCount)
            throw Error(sourceName, $"tag '{tag}' exceeds the resource-count safety limit");
    }

    private static void RejectDuplicateNumbers(IEnumerable<GffResource> resources, string sourceName, string tag)
    {
        var seen = new HashSet<uint>();
        foreach (var resource in resources)
            if (!seen.Add(resource.Number))
                throw Error(sourceName, $"tag '{tag}' contains duplicate resource number {resource.Number}");
    }

    private static void ValidateRange(uint offset, uint size, int length, string sourceName, string tag, uint number)
    {
        if (offset > length || (ulong)offset + size > (ulong)length)
            throw Error(sourceName, $"resource '{tag}' #{number} extends beyond the file");
    }

    private static void RejectPartialOverlaps(IReadOnlyList<GffResource> resources, string sourceName)
    {
        var ordered = resources.Where(item => item.Size != 0)
            .OrderBy(item => item.Offset).ThenByDescending(item => item.Size).ToArray();
        for (var index = 1; index < ordered.Length; index++)
        {
            var previous = ordered[index - 1];
            var current = ordered[index];
            var previousEnd = (ulong)previous.Offset + previous.Size;
            if (current.Offset < previousEnd &&
                (current.Offset != previous.Offset || current.Size != previous.Size))
                throw Error(sourceName, $"resources '{previous.Tag}' #{previous.Number} and '{current.Tag}' #{current.Number} partially overlap");
        }
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");

    private sealed record SecondaryDescriptor(
        string Tag, uint TableIndex, IReadOnlyList<NumberingSegment> Segments, int ExpandedCount);

    private sealed record NumberingSegment(uint First, uint Length);

    private sealed class BoundedReader(byte[] bytes, string sourceName)
    {
        public ushort UInt16(int offset, string field = "field")
        {
            Ensure(offset, 2, field);
            return BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
        }

        public uint UInt32(int offset, string field = "field")
        {
            Ensure(offset, 4, field);
            return BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));
        }

        public string ReadTag(int offset)
        {
            Ensure(offset, 4, "tag");
            var span = bytes.AsSpan(offset, 4);
            foreach (var value in span)
                if (value is < 0x20 or > 0x7e)
                    throw Error(sourceName, $"contains a non-printable tag at offset {offset}");
            return Encoding.ASCII.GetString(span);
        }

        private void Ensure(int offset, int length, string field)
        {
            if (offset < 0 || length < 0 || (long)offset + length > bytes.Length)
                throw Error(sourceName, $"is truncated while reading {field} at offset {offset}");
        }
    }
}
