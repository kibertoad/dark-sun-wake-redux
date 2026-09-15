using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record ExecutablePreferencesText(
    IReadOnlyList<string> DifficultyLabels,
    IReadOnlyList<string> AboutLines);

public static class ExecutablePreferencesTextReader
{
    public const int DifficultyTableOffset = 0x4f6f5;
    public const int AboutTableOffset = 0x4f7cc;
    public const int DifficultyLabelCount = 4;
    public const int AboutLineCount = 9;
    public const int MaximumStringBytes = 64;
    public const long MaximumExecutableBytes = 16L * 1024 * 1024;
    private const string CenterPrefix = "%C%C%C";

    public static ExecutablePreferencesText Read(
        Stream stream,
        string sourceName = "executable")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length <= AboutTableOffset || stream.Length > MaximumExecutableBytes)
            throw Error(sourceName, $"has invalid file length {stream.Length}");

        stream.Position = DifficultyTableOffset;
        var difficulty = Enumerable.Range(0, DifficultyLabelCount)
            .Select(index => ReadString(stream, sourceName, $"difficulty label {index}"))
            .ToArray();
        if (difficulty.Any(label => string.IsNullOrWhiteSpace(label)))
            throw Error(sourceName, "contains an empty difficulty label");

        stream.Position = AboutTableOffset;
        var about = new string[AboutLineCount];
        for (var index = 0; index < about.Length; index++)
        {
            var encoded = ReadString(stream, sourceName, $"About line {index}");
            if (!encoded.StartsWith(CenterPrefix, StringComparison.Ordinal) ||
                encoded.Length == CenterPrefix.Length)
                throw Error(sourceName,
                    $"About line {index} lacks its centered-text control prefix");
            about[index] = encoded[CenterPrefix.Length..];
        }
        return new(difficulty, about);
    }

    private static string ReadString(Stream stream, string sourceName, string field)
    {
        var bytes = new List<byte>();
        for (var index = 0; index <= MaximumStringBytes; index++)
        {
            var value = stream.ReadByte();
            if (value < 0) throw Error(sourceName, $"is truncated while reading {field}");
            if (value == 0) return Encoding.ASCII.GetString(bytes.ToArray());
            if (value is < 32 or > 126)
                throw Error(sourceName, $"contains a non-printable byte in {field}");
            if (index == MaximumStringBytes)
                throw Error(sourceName, $"has an unterminated or oversized {field}");
            bytes.Add(checked((byte)value));
        }
        throw new InvalidOperationException("Unreachable Preferences string reader state.");
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
