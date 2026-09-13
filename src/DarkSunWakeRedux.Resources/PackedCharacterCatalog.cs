using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedCharacterMetadata(
    uint ResourceNumber,
    byte TailRecordCount,
    string Name,
    GffCharacterAbilityScores AbilityScores,
    byte RawPsionicMask);

public sealed record PackedCharacterCatalog(IReadOnlyList<PackedCharacterMetadata> Characters)
{
    public const ushort FormatVersion = 1;
    public const int MaximumCharacters = 256;
    public const int MaximumNameBytes = GffCharacterIdentity.NameSlotLength - 1;
    public const long MaximumFileBytes = 64L * 1024;
    private const int HeaderBytes = 10;

    public static PackedCharacterCatalog From(IReadOnlyList<GffCharacterCatalogEntry> characters)
    {
        ArgumentNullException.ThrowIfNull(characters);
        return new(characters.Select(character => new PackedCharacterMetadata(
            character.ResourceNumber,
            character.TailRecordCount,
            character.Name,
            character.AbilityScores,
            character.RawPsionicMask)).ToArray());
    }

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("Character catalog stream must be writable.", nameof(stream));
        ValidateCatalog(Characters, "character catalog");
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSCH"u8);
        writer.Write(FormatVersion);
        writer.Write(checked((uint)Characters.Count));
        foreach (var character in Characters.OrderBy(character => character.ResourceNumber))
        {
            writer.Write(character.ResourceNumber);
            writer.Write(character.TailRecordCount);
            var name = Encoding.ASCII.GetBytes(character.Name);
            writer.Write(checked((byte)name.Length));
            writer.Write(name);
            foreach (var (_, score) in character.AbilityScores.All()) writer.Write(score);
            writer.Write(character.RawPsionicMask);
        }
    }

    public static PackedCharacterCatalog Read(Stream stream, string sourceName = "character catalog")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes || stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"has invalid file length {stream.Length}");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (!bytes.AsSpan(0, 4).SequenceEqual("DSCH"u8))
            throw Error(sourceName, "has an invalid signature");
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4, 2));
        if (version != FormatVersion)
            throw Error(sourceName, $"uses unsupported format version {version}");
        var count = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(6, 4));
        if (count is 0 or > MaximumCharacters)
            throw Error(sourceName, $"declares invalid character count {count}");

        var characters = new List<PackedCharacterMetadata>(checked((int)count));
        var position = HeaderBytes;
        uint? previousNumber = null;
        for (var index = 0; index < count; index++)
        {
            Ensure(bytes, position, 6, sourceName, $"character {index} header");
            var number = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(position, 4));
            var tailRecordCount = bytes[position + 4];
            var nameLength = bytes[position + 5];
            position += 6;
            if (nameLength is 0 or > MaximumNameBytes)
                throw Error(sourceName, $"character {number} has invalid name length {nameLength}");
            Ensure(bytes, position, nameLength + 7, sourceName, $"character {number} body");
            var nameBytes = bytes.AsSpan(position, nameLength);
            if (!IsPrintableAscii(nameBytes))
                throw Error(sourceName, $"character {number} name contains unsupported bytes");
            var name = Encoding.ASCII.GetString(nameBytes);
            position += nameLength;
            var abilityScores = new GffCharacterAbilityScores(
                bytes[position], bytes[position + 1], bytes[position + 2],
                bytes[position + 3], bytes[position + 4], bytes[position + 5]);
            var rawPsionicMask = bytes[position + 6];
            position += 7;
            var character = new PackedCharacterMetadata(
                number, tailRecordCount, name, abilityScores, rawPsionicMask);
            ValidateCharacter(character, sourceName);
            if (previousNumber is not null && number <= previousNumber)
                throw Error(sourceName, "character resources are duplicated or not in canonical order");
            previousNumber = number;
            characters.Add(character);
        }
        if (position != bytes.Length) throw Error(sourceName, "contains trailing bytes");
        return new(characters);
    }

    private static void ValidateCatalog(
        IReadOnlyList<PackedCharacterMetadata> characters, string sourceName)
    {
        if (characters is null || characters.Count is 0 or > MaximumCharacters)
            throw Error(sourceName, "contains an invalid number of characters");
        var numbers = new HashSet<uint>();
        foreach (var character in characters)
        {
            if (character is null) throw Error(sourceName, "contains a null character");
            if (!numbers.Add(character.ResourceNumber))
                throw Error(sourceName, $"duplicates character resource {character.ResourceNumber}");
            ValidateCharacter(character, sourceName);
        }
    }

    private static void ValidateCharacter(PackedCharacterMetadata character, string sourceName)
    {
        if (string.IsNullOrEmpty(character.Name) || character.Name.Length > MaximumNameBytes ||
            character.Name.Any(value => value is < ' ' or > '~'))
            throw Error(sourceName, $"character {character.ResourceNumber} has an invalid name");
        if (character.AbilityScores is null)
            throw Error(sourceName, $"character {character.ResourceNumber} has no ability scores");
        foreach (var (name, score) in character.AbilityScores.All())
            if (score is < GffCharacterAbilityScores.MinimumScore or
                > GffCharacterAbilityScores.MaximumScore)
                throw Error(sourceName,
                    $"character {character.ResourceNumber} has invalid {name} score {score}");
        if (character.RawPsionicMask == 0 ||
            (character.RawPsionicMask & ~GffPsionicMask.DefinedBits) != 0)
            throw Error(sourceName,
                $"character {character.ResourceNumber} has invalid raw PSIN mask " +
                $"0x{character.RawPsionicMask:x2}");
    }

    private static bool IsPrintableAscii(ReadOnlySpan<byte> bytes)
    {
        foreach (var value in bytes)
            if (value is < 32 or > 126) return false;
        return true;
    }

    private static void Ensure(
        byte[] bytes, int offset, int length, string sourceName, string field)
    {
        if (offset < 0 || length < 0 || (long)offset + length > bytes.Length)
            throw Error(sourceName, $"is truncated while reading {field}");
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
