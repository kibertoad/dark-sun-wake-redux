using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class RdffKeyInspectionTests
{
    private static RdffKeyInspection.Controls Controls(int one = 0, int two = 0, int five = 1) =>
        new() { SourceSha256 = new string('0', 64), KindOne = one, KindTwo = two, KindFive = five };

    private static RdffKeyInspection.Resource Record(uint number, byte kind, short first, int trailing = 0)
    {
        int length = kind == 2 ? 49 : 23;
        var data = new byte[10 + length + 10 + trailing];
        data[0] = 1; data[2] = kind;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(8), (ushort)length);
        BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(10), first);
        data[10 + length] = 0xFF;
        return new(number, data);
    }

    [Fact]
    public void CensusDistinguishesExactIdentityLowWordAliasAndMismatchWithoutReadingTrailingData()
    {
        var report = RdffKeyInspection.Analyze(new[]
        {
            Record(65537, 5, -1), Record(100, 5, -100, 7), Record(200, 5, 77),
            Record(10, 1, -10), Record(20, 2, -20), Record(21, 2, -21)
        }, Controls(1, 2, 3));
        Assert.Equal(6, report.Resources);
        Assert.Equal(1, report.KindOne);
        Assert.Equal(2, report.KindTwo);
        Assert.Equal(3, report.KindFive);
        Assert.Equal(1, report.ExactMatches);
        Assert.Equal(2, report.LowWordMatches);
        Assert.Equal(new uint[] {100, 200, 65537}, report.Rows.Select(row => row.ResourceNumber));
        Assert.Equal(7, report.Rows[0].TrailingBytes);
        Assert.False(report.Rows[1].NegatedLowWord);
        Assert.False(report.Rows[2].ExactNegatedNumber);
        Assert.True(report.Rows[2].NegatedLowWord);
    }

    [Fact]
    public void MissingOrVacuousControlsAndUnsupportedJsonAreRejected()
    {
        var parsed = RdffKeyInspection.ParseControls(Encoding.UTF8.GetBytes(
            "{\"sourceSha256\":\"" + new string('0',64) + "\",\"kindOne\":0,\"kindTwo\":0,\"kindFive\":1}"));
        Assert.Equal(Controls(), parsed);
        foreach (string json in new[] { "{}", "null", "[]", "{",
            "{\"sourceSha256\":\"" + new string('0',64) + "\",\"kindOne\":0,\"kindTwo\":0,\"kindFive\":1,\"extra\":1}" })
        {
            var error = Xunit.Record.Exception(() => RdffKeyInspection.ParseControls(Encoding.UTF8.GetBytes(json)));
            Assert.True(error is JsonException or InvalidDataException, error?.ToString() ?? "accepted missing controls");
        }
        foreach (var controls in new[] { Controls(five:0), Controls(one:-1), Controls(one:4096),
            Controls() with {SourceSha256="bad"} })
            Assert.Throws<InvalidDataException>(() => RdffKeyInspection.Analyze([], controls));
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.ParseControls(new byte[65537]));
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.Analyze([Record(1,5,-1)], Controls(five:2)));
    }

    [Theory]
    [InlineData("short-header")]
    [InlineData("short-data")]
    [InlineData("short-end")]
    [InlineData("wrong-end")]
    [InlineData("wrong-type")]
    [InlineData("wrong-kind")]
    [InlineData("wrong-width")]
    [InlineData("oversized")]
    public void InvalidSelectedRecordsFailWithoutSuccessfulPartialCensus(string fault)
    {
        var data = Record(1,5,-1).Data.ToArray();
        switch (fault)
        {
            case "short-header": data = data[..9]; break;
            case "short-data": data = data[..32]; break;
            case "short-end": data = data[..42]; break;
            case "wrong-end": data[33] = 1; break;
            case "wrong-type": data[0] = 2; break;
            case "wrong-kind": data[2] = 4; break;
            case "wrong-width": data[8] = 22; break;
            case "oversized":
                var larger = new byte[1024 * 1024 + 1];
                data.CopyTo(larger,0);
                data = larger;
                break;
        }
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.Analyze([new(1,data)], Controls()));
    }

    [Fact]
    public void DuplicateAndExcessiveResourcesAreRejected()
    {
        var record = Record(1,5,-1);
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.Analyze([record, record], Controls(five:2)));
        var limitError = Assert.Throws<InvalidDataException>(() => RdffKeyInspection.Analyze(
            Enumerable.Repeat(record,4097), Controls()));
        Assert.Contains("resource limit", limitError.Message);
    }

    [Fact]
    public void SourceHashMustMatchBeforeAnArchiveCanBeReadAndVerificationRewinds()
    {
        using var source = new MemoryStream(Encoding.UTF8.GetBytes("synthetic non-archive"));
        var error = Assert.Throws<InvalidDataException>(() => RdffKeyInspection.VerifySource(source, Controls()));
        Assert.Contains("SHA-256", error.Message);
        string hash = Convert.ToHexStringLower(SHA256.HashData(source.ToArray()));
        Assert.Equal(hash, RdffKeyInspection.VerifySource(source, Controls() with {SourceSha256=hash}));
        Assert.Equal(0, source.Position);
        using var empty = new MemoryStream();
        var emptyControls = Controls() with {SourceSha256=Convert.ToHexStringLower(SHA256.HashData(Array.Empty<byte>()))};
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.VerifySource(empty, emptyControls));
        var largeBytes = new byte[32 * 1024 * 1024 + 1];
        using var oversized = new MemoryStream(largeBytes);
        var largeControls = Controls() with {SourceSha256=Convert.ToHexStringLower(SHA256.HashData(largeBytes))};
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.VerifySource(oversized, largeControls));
    }
}
