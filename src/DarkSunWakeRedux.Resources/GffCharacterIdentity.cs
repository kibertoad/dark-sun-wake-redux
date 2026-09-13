using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record GffCharacterIdentity(string Name)
{
    public const int NameOffset = 43;
    public const int NameSlotLength = 16;

    public static GffCharacterIdentity Read(ReadOnlyMemory<byte> data, string sourceName)
    {
        GffCharacterRecordEnvelope.Read(data, sourceName);

        var slot = data.Span.Slice(NameOffset, NameSlotLength);
        var terminator = slot.IndexOf((byte)0);
        if (terminator < 0)
            throw new InvalidDataException(
                $"{sourceName}: CHAR name is not terminated within its {NameSlotLength}-byte slot.");
        if (terminator == 0)
            throw new InvalidDataException($"{sourceName}: CHAR name is empty.");

        var encodedName = slot[..terminator];
        if (encodedName.ContainsAnyExceptInRange((byte)0x20, (byte)0x7e))
            throw new InvalidDataException(
                $"{sourceName}: CHAR name contains a non-printable ASCII byte.");

        return new(Encoding.ASCII.GetString(encodedName));
    }
}
