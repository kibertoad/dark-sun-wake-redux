using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record GffTextResource(IReadOnlyList<string> Lines)
{
    public const int MaximumPayloadBytes = 1024 * 1024;
    public const int MaximumLines = 65_536;
    public const int MaximumLineBytes = 4_096;

    public static GffTextResource Read(ReadOnlyMemory<byte> payload, string sourceName = "TEXT resource")
    {
        if (payload.Length is 0 or > MaximumPayloadBytes)
            throw Error(sourceName, $"has invalid payload length {payload.Length}");
        var bytes = payload.Span;
        var lines = new List<string>();
        var start = 0;
        for (var index = 0; index < bytes.Length; index++)
        {
            var value = bytes[index];
            if (value == 13)
            {
                if (index + 1 >= bytes.Length || bytes[index + 1] != 10)
                    throw Error(sourceName, $"contains a bare carriage return at offset {index}");
                AddLine(bytes[start..index], lines, sourceName);
                index++;
                start = index + 1;
                continue;
            }
            if (value == 10)
                throw Error(sourceName, $"contains a bare line feed at offset {index}");
            if (value is < 32 or > 126)
                throw Error(sourceName, $"contains unsupported byte 0x{value:x2} at offset {index}");
        }
        if (start != bytes.Length)
            throw Error(sourceName, "does not end with CRLF");
        return new(lines);
    }

    private static void AddLine(ReadOnlySpan<byte> bytes, List<string> lines, string sourceName)
    {
        if (bytes.Length > MaximumLineBytes)
            throw Error(sourceName, $"contains a line longer than {MaximumLineBytes} bytes");
        if (lines.Count == MaximumLines)
            throw Error(sourceName, $"contains more than {MaximumLines} lines");
        lines.Add(Encoding.ASCII.GetString(bytes));
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
