namespace DarkSunWakeRedux.Resources;

public enum GplPackedStringKind
{
    ActiveCharacterName,
    Compressed
}

public sealed record GplPackedStringValue(
    GplPackedStringKind Kind,
    string Value,
    int BytesConsumed);

public static class GplPackedString
{
    public const int MaximumCharacters = 1_024;
    public const byte ActiveCharacterNameMarker = 0x01;
    public const byte UncompressedMarker = 0x02;
    public const byte Terminator = 0x03;
    public const byte CompressedMarker = 0x05;

    public static GplPackedStringValue Read(
        ReadOnlySpan<byte> bytes,
        string sourceName = "GPL packed string")
    {
        if (bytes.IsEmpty) throw Error(sourceName, "is missing its type marker");
        if (bytes[0] == ActiveCharacterNameMarker)
            return new(GplPackedStringKind.ActiveCharacterName, string.Empty, 1);
        if (bytes[0] == UncompressedMarker)
            throw Error(sourceName, "uses the unsupported uncompressed string form");
        if (bytes[0] != CompressedMarker)
            throw Error(sourceName, $"uses unknown type marker 0x{bytes[0]:X2}");

        uint buffer = 0;
        var shift = 1;
        var position = 1;
        var characters = new List<char>();
        while (characters.Count < MaximumCharacters)
        {
            if (shift > 0)
            {
                if (position >= bytes.Length)
                    throw Error(sourceName, "is truncated before its terminator");
                buffer = ((buffer << 8) & 0xff00) | bytes[position++];
            }
            var value = checked((byte)((buffer >> shift) & 0x7f));
            if (value == Terminator)
                return new(GplPackedStringKind.Compressed,
                    new string(characters.ToArray()), position);
            characters.Add((char)value);
            shift++;
            if (shift > 7) shift = 0;
        }
        throw Error(sourceName,
            $"exceeds the {MaximumCharacters}-character safety limit or lacks a terminator");
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
