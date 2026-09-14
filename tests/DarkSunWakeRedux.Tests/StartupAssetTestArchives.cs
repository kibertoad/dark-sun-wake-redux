using System.Text;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Tests;

internal static class StartupAssetTestArchives
{
    public static UiChildReference[] InteractionChildren() =>
    [
        new("BUTN", 15308, 23, 59),
        new("BUTN", 15307, 43, 59),
        new("BUTN", 15306, 3, 59),
        new("BUTN", 15309, 61, 60),
        new("APFM", 15200, 0, 0)
    ];

    public static UiChildReference[] DialogueSpeechChildren() =>
    [
        new("BUTN", 12300, 0, 0),
        new("EBOX", 12400, 75, 6),
        new("BUTN", 2093, 305, 4),
        new("BUTN", 2094, 305, 18)
    ];

    public static UiChildReference[] DialogueResponseChildren() =>
    [
        new("BUTN", 2096, 305, 18),
        new("BUTN", 2076, 3, 13),
        new("BUTN", 2077, 3, 21),
        new("BUTN", 2078, 3, 29),
        new("BUTN", 2079, 3, 37),
        new("BUTN", 2080, 3, 45),
        new("BUTN", 2095, 305, 4)
    ];

    public static byte[] Character()
    {
        var character = new byte[GffCharacterRecordEnvelope.FixedHeaderSize];
        character[0] = GffCharacterRecordEnvelope.SupportedVersion;
        Array.Fill(character, (byte)15, GffCharacterAbilityScores.ScoresOffset,
            GffCharacterAbilityScores.ScoreCount);
        Encoding.ASCII.GetBytes("Hero").CopyTo(character, GffCharacterIdentity.NameOffset);
        var indexOffset = GffArchive.HeaderSize + character.Length + 1;
        using var stream = Header(indexOffset);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        var characterOffset = checked((int)stream.Position);
        writer.Write(character);
        var psionicOffset = checked((int)stream.Position);
        writer.Write((byte)1);
        Index(writer, 2);
        Table(writer, "CHAR", [(7U, characterOffset, character.Length)]);
        Table(writer, "PSIN", [(7U, psionicOffset, 1)]);
        return stream.ToArray();
    }

    public static byte[] Dialogue()
    {
        var portrait = TransparentImage(72, 72);
        byte[] script = [0x19, 0x31];
        byte[] globals = GlobalStringScript();
        var indexOffset = GffArchive.HeaderSize + portrait.Length + script.Length + globals.Length;
        using var stream = Header(indexOffset);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        var portraitOffset = checked((int)stream.Position);
        writer.Write(portrait);
        var scriptOffset = checked((int)stream.Position);
        writer.Write(script);
        var globalsOffset = checked((int)stream.Position);
        writer.Write(globals);
        Index(writer, 3);
        Table(writer, "PORT",
            [(OriginalContent.FirstTyrDialoguePortraitResourceNumber,
                portraitOffset, portrait.Length)]);
        Table(writer, "GPL ",
            [(OriginalContent.FirstTyrDialogueScriptResourceNumber,
                scriptOffset, script.Length)]);
        Table(writer, "MAS ",
            [(OriginalContent.DialogueGlobalStringsScriptResourceNumber,
                globalsOffset, globals.Length)]);
        return stream.ToArray();
    }

    public static byte[] GlobalStringScript()
    {
        var bytes = new byte[GplGlobalStringProjectionReader.NextInstructionOffset];
        byte[] instruction = [0x0a, 0x86, 0x05, 0x92, .. EncodePacked("Depart!!")];
        instruction.CopyTo(bytes, GplGlobalStringProjectionReader.AssignmentInstructionOffset);
        return bytes;
    }

    private static byte[] EncodePacked(string text)
    {
        var values = text.Select(character => checked((byte)character))
            .Append(GplPackedString.Terminator).ToArray();
        var bits = values.SelectMany(value =>
            Enumerable.Range(0, 7).Select(bit => (value >> (6 - bit)) & 1)).ToArray();
        var packed = new List<byte> { GplPackedString.CompressedMarker };
        for (var offset = 0; offset < bits.Length; offset += 8)
        {
            byte value = 0;
            for (var bit = 0; bit < 8 && offset + bit < bits.Length; bit++)
                value |= checked((byte)(bits[offset + bit] << (7 - bit)));
            packed.Add(value);
        }
        return packed.ToArray();
    }

    private static MemoryStream Header(int indexOffset)
    {
        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("GFFI"u8);
        writer.Write(0x0003_0000U);
        writer.Write((uint)GffArchive.HeaderSize);
        writer.Write((uint)indexOffset);
        writer.Write(new byte[12]);
        return stream;
    }

    private static void Index(BinaryWriter writer, ushort tagCount)
    {
        writer.Write(0U);
        writer.Write(0U);
        writer.Write(tagCount);
    }

    private static void Table(BinaryWriter writer, string tag,
        IReadOnlyList<(uint Number, int Offset, int Size)> entries)
    {
        writer.Write(Encoding.ASCII.GetBytes(tag));
        writer.Write((uint)entries.Count);
        foreach (var entry in entries)
        {
            writer.Write(entry.Number);
            writer.Write((uint)entry.Offset);
            writer.Write((uint)entry.Size);
        }
    }

    private static byte[] TransparentImage(int width, int height)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(20U);
        writer.Write((ushort)1);
        writer.Write(10U);
        writer.Write((ushort)width);
        writer.Write((ushort)height);
        writer.Write((byte)0xff);
        writer.Write("PLAN"u8);
        writer.Write((byte)0);
        return stream.ToArray();
    }
}
