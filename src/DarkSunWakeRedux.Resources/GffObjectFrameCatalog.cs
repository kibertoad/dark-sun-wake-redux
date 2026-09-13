using System.Buffers.Binary;

namespace DarkSunWakeRedux.Resources;

public sealed record GffObjectFrameCatalogEntry(
    uint ResourceNumber,
    ushort RawWord0,
    short XOffset,
    short YOffset,
    ushort RawWord6,
    ushort RawWord8,
    ushort RawWord10,
    uint ImageResourceNumber,
    IndexedImage Image);

public sealed record GffObjectFrameCatalog(IReadOnlyList<GffObjectFrameCatalogEntry> Entries)
{
    public const int RecordSize = 16;
    public const int MaximumEntries = 65_536;

    public static GffObjectFrameCatalog Read(
        GffArchive archive,
        IEnumerable<uint>? requiredResourceNumbers = null,
        string sourceName = "object GFF")
    {
        ArgumentNullException.ThrowIfNull(archive);
        var descriptorList = archive.Resources.Where(resource => resource.Tag == "OJFF").ToArray();
        if (descriptorList.Length is 0 or > MaximumEntries)
            throw Error(sourceName, $"contains invalid OJFF count {descriptorList.Length}");
        var descriptors = descriptorList.ToDictionary(resource => resource.Number);
        uint[] numbers;
        if (requiredResourceNumbers is null)
        {
            numbers = descriptors.Keys.Order().ToArray();
        }
        else
        {
            var requested = new HashSet<uint>();
            foreach (var number in requiredResourceNumbers)
                if (requested.Add(number) && requested.Count > MaximumEntries)
                    throw Error(sourceName, $"requests more than {MaximumEntries} OJFF records");
            if (requested.Count == 0)
                throw Error(sourceName, "requests no OJFF records");
            numbers = requested.Order().ToArray();
        }

        var imageCache = new Dictionary<uint, IndexedImage>();
        var entries = new GffObjectFrameCatalogEntry[numbers.Length];
        for (var index = 0; index < numbers.Length; index++)
        {
            var number = numbers[index];
            if (!descriptors.ContainsKey(number))
                throw Error(sourceName, $"requested OJFF #{number} does not exist");
            var bytes = archive.GetResource("OJFF", number).Span;
            if (bytes.Length != RecordSize)
                throw Error(sourceName,
                    $"OJFF #{number} must contain exactly {RecordSize} bytes, found {bytes.Length}");
            var reserved = UInt16(bytes, 14);
            if (reserved != 0)
                throw Error(sourceName, $"OJFF #{number} has nonzero reserved word 0x{reserved:x4}");
            var imageNumber = UInt16(bytes, 12);
            if (!imageCache.TryGetValue(imageNumber, out var image))
            {
                try
                {
                    image = IndexedImage.Read(archive.GetResource("BMP ", imageNumber),
                        $"{sourceName}:BMP #{imageNumber}");
                }
                catch (KeyNotFoundException)
                {
                    throw Error(sourceName,
                        $"OJFF #{number} references missing BMP #{imageNumber}");
                }
                if (image.Frames.Count == 0)
                    throw Error(sourceName, $"OJFF #{number} references empty BMP #{imageNumber}");
                imageCache.Add(imageNumber, image);
            }
            entries[index] = new(number,
                UInt16(bytes, 0), Int16(bytes, 2), Int16(bytes, 4),
                UInt16(bytes, 6), UInt16(bytes, 8), UInt16(bytes, 10),
                imageNumber, image);
        }
        return new(entries);
    }

    private static short Int16(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadInt16LittleEndian(bytes[offset..]);

    private static ushort UInt16(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
