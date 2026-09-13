using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record GffCharacterIdentity(string Name)
{
    public const int NameOffset = 43;
    public const int NameSlotLength = 16;
    public const int MinimumResourceSize = NameOffset + NameSlotLength;
    public const int MaximumResourceSize = 64 * 1024;

    public static GffCharacterIdentity Read(ReadOnlyMemory<byte> data, string sourceName)
    {
        if (data.Length < MinimumResourceSize)
            throw new InvalidDataException(
                $"{sourceName}: CHAR resource is too short for its name slot ({data.Length} bytes).");
        if (data.Length > MaximumResourceSize)
            throw new InvalidDataException(
                $"{sourceName}: CHAR resource exceeds the {MaximumResourceSize}-byte safety limit.");

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
