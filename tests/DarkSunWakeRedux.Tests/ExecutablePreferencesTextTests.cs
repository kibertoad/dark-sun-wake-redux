using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExecutablePreferencesTextTests
{
    [Fact]
    public void ReadsBoundedDifficultyDescriptionAndCenteredAboutTables()
    {
        using var stream = new MemoryStream(SyntheticExecutable());

        var text = ExecutablePreferencesTextReader.Read(stream, "synthetic executable");

        Assert.Equal(["LOW", "NORMAL", "HIGH", "EXTREME"], text.DifficultyLabels);
        Assert.Equal(10, text.Descriptions.Count);
        Assert.Equal("Description 0", text.Descriptions[0]);
        Assert.StartsWith("Description 9", text.Descriptions[9], StringComparison.Ordinal);
        Assert.Equal(9, text.AboutLines.Count);
        Assert.Equal("Synthetic About line 0", text.AboutLines[0]);
        Assert.Equal("Synthetic About line 8", text.AboutLines[8]);
    }

    [Fact]
    public void RejectsInvalidDescriptionSpanMissingControlPrefixAndOversizedStrings()
    {
        var invalidDescriptionSpan = SyntheticExecutable();
        invalidDescriptionSpan[ExecutablePreferencesTextReader.DescriptionTableOffset + 1] = 0;
        Assert.Contains("invalid Preferences description table",
            Assert.Throws<InvalidDataException>(() =>
                ExecutablePreferencesTextReader.Read(
                    new MemoryStream(invalidDescriptionSpan))).Message);

        var missingPrefix = SyntheticExecutable();
        missingPrefix[ExecutablePreferencesTextReader.AboutTableOffset] = (byte)'X';
        Assert.Contains("control prefix", Assert.Throws<InvalidDataException>(() =>
            ExecutablePreferencesTextReader.Read(new MemoryStream(missingPrefix))).Message);

        var oversized = SyntheticExecutable();
        Array.Fill(oversized, (byte)'X',
            ExecutablePreferencesTextReader.DifficultyTableOffset,
            ExecutablePreferencesTextReader.MaximumStringBytes + 1);
        Assert.Contains("unterminated or oversized",
            Assert.Throws<InvalidDataException>(() =>
                ExecutablePreferencesTextReader.Read(new MemoryStream(oversized))).Message);
    }

    [Fact]
    public void RejectsTruncationAndNonSeekableInput()
    {
        Assert.Throws<InvalidDataException>(() => ExecutablePreferencesTextReader.Read(
            new MemoryStream(new byte[ExecutablePreferencesTextReader.AboutTableOffset])));
        Assert.Throws<InvalidDataException>(() => ExecutablePreferencesTextReader.Read(
            new NonSeekableStream(SyntheticExecutable())));
    }

    internal static byte[] SyntheticExecutable()
    {
        var bytes = new byte[ExecutablePreferencesTextReader.AboutTableOffset + 1024];
        WriteStrings(bytes, ExecutablePreferencesTextReader.DifficultyTableOffset,
            ["LOW", "NORMAL", "HIGH", "EXTREME"]);
        WriteFixedSpanStrings(bytes, ExecutablePreferencesTextReader.DescriptionTableOffset,
            ExecutablePreferencesTextReader.AboutTableOffset,
            Enumerable.Range(0, ExecutablePreferencesTextReader.DescriptionCount)
                .Select(index => $"Description {index}").ToArray());
        WriteStrings(bytes, ExecutablePreferencesTextReader.AboutTableOffset,
            Enumerable.Range(0, ExecutablePreferencesTextReader.AboutLineCount)
                .Select(index => $"%C%C%CSynthetic About line {index}").ToArray());
        return bytes;
    }

    private static void WriteFixedSpanStrings(
        byte[] target, int offset, int endOffset, IReadOnlyList<string> values)
    {
        WriteStrings(target, offset, values);
        var contentBytes = values.Sum(value => Encoding.ASCII.GetByteCount(value) + 1);
        var padding = endOffset - offset - contentBytes;
        if (padding < 0) throw new InvalidOperationException("Synthetic strings exceed their span.");
        var last = values[^1];
        var lastStart = offset + values.Take(values.Count - 1)
            .Sum(value => Encoding.ASCII.GetByteCount(value) + 1);
        var replacement = last + new string('X', padding);
        Array.Clear(target, lastStart, Encoding.ASCII.GetByteCount(last) + 1);
        var encoded = Encoding.ASCII.GetBytes(replacement);
        encoded.CopyTo(target, lastStart);
    }

    private static void WriteStrings(byte[] target, int offset, IEnumerable<string> values)
    {
        var position = offset;
        foreach (var value in values)
        {
            var encoded = Encoding.ASCII.GetBytes(value);
            encoded.CopyTo(target, position);
            position += encoded.Length + 1;
        }
    }

    private sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
    }
}
