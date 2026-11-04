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

    [Fact]
    public void LookInputsKeepDistinctWordsNamedBoundaryAndOtherSizeResources()
    {
        var named = Record(20, 1, -20).Data.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(named.AsSpan(12), 0x1234);
        BinaryPrimitives.WriteUInt16LittleEndian(named.AsSpan(16), 0x5678);
        BinaryPrimitives.WriteUInt16LittleEndian(named.AsSpan(20), 50);
        BinaryPrimitives.WriteUInt16LittleEndian(named.AsSpan(26), 0x9ABC);
        BinaryPrimitives.WriteUInt16LittleEndian(named.AsSpan(30), 9998);
        var unnamed = Record(10, 1, -10, 165).Data.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(unnamed.AsSpan(16), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(unnamed.AsSpan(20), 50);
        BinaryPrimitives.WriteUInt16LittleEndian(unnamed.AsSpan(30), 9999);
        var excluded = Record(30, 1, -30);
        var result = RdffKeyInspection.AnalyzeLook(
            [new(20,named), excluded, Record(40,2,-40), Record(50,5,-50), new(10,unnamed)],
            Controls(3,1,1));
        Assert.Equal(5, result.Resources);
        Assert.Equal(3, result.FirstItems);
        Assert.Equal(2, result.Type50);
        Assert.Equal(1, result.NamedType50);
        Assert.Equal(1, result.UnnamedType50);
        Assert.Equal(new uint[] {10,20}, result.Rows.Select(row => row.ResourceNumber));
        Assert.Equal(208, result.Rows[0].ResourceBytes);
        Assert.False(result.Rows[0].NameGate);
        Assert.Equal((ushort)1, result.Rows[0].Value);
        Assert.Equal((ushort)0, result.Rows[0].Word10);
        Assert.Equal(43, result.Rows[1].ResourceBytes);
        Assert.True(result.Rows[1].NameGate);
        Assert.Equal((ushort)0x1234, result.Rows[1].Quantity);
        Assert.Equal((ushort)0x5678, result.Rows[1].Value);
        Assert.Equal((ushort)0x9ABC, result.Rows[1].Word10);
        Assert.Equal((ushort)9998, result.Rows[1].NameNumber);
    }

    [Fact]
    public void LookInputsRejectVacuousDomainsAndInheritedFramingFailures()
    {
        var data = Record(10,1,-10).Data.ToArray();
        var five = Record(50,5,-50);
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.AnalyzeLook(
            [new(10,data),five], Controls(1)));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(20), 50);
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.AnalyzeLook(
            [new(10,data),five], Controls(2)));
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.AnalyzeLook(
            [new(10,data[..32]),five], Controls(1)));
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.AnalyzeLook(
            [new(10,data),new(10,data),five], Controls(2)));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(8), 22);
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.AnalyzeLook(
            [new(10,data),five], Controls(1)));
    }

    [Fact]
    public void LookChainsIncludeLaterItemsAndStopBeforeTrailingDecoys()
    {
        var first = Record(10, 1, -10).Data.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(20), 50);
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(30), 9998);
        var later = first[..33].ToArray();
        later[0] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(later.AsSpan(30), 9999);
        BinaryPrimitives.WriteUInt16LittleEndian(later.AsSpan(12), 17);
        BinaryPrimitives.WriteUInt16LittleEndian(later.AsSpan(16), 19);
        BinaryPrimitives.WriteUInt16LittleEndian(later.AsSpan(26), 23);
        byte[] chain = [..first[..33], ..later, ..first[33..], ..later];
        var result = RdffKeyInspection.AnalyzeLookChains([new(10,chain), Record(20,5,-20)], Controls(1));
        Assert.Equal(3, result.Chunks);
        Assert.Equal(2, result.Items);
        Assert.Equal(2, result.Type50);
        Assert.Equal(1, result.NamedType50);
        Assert.Equal(1, result.UnnamedType50);
        Assert.Equal(1, result.TrailingResources);
        Assert.Equal(new[] {0,1}, result.Rows.Select(row => row.ChunkIndex));
        Assert.Equal((ushort)17, result.Rows[1].Quantity);
        Assert.Equal((ushort)19, result.Rows[1].Value);
        Assert.Equal((ushort)23, result.Rows[1].Word10);
        Assert.Equal(1, result.NamedItems);
        Assert.Equal(1, result.UnnamedItems);
        Assert.Equal(new RdffKeyInspection.ItemNameDomain(50,2,1,1), Assert.Single(result.ItemTypes));
        Assert.Equal(new RdffKeyInspection.UnnamedItem(10,1,50,9999), Assert.Single(result.UnnamedRows));
    }

    [Fact]
    public void LookChainNameDomainKeepsUnnamedOtherTypesSeparateAndSorted()
    {
        var other = Record(30,1,-30).Data.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(other.AsSpan(20), 17);
        BinaryPrimitives.WriteUInt16LittleEndian(other.AsSpan(30), 65535);
        var selected = Record(10,1,-10).Data.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(selected.AsSpan(20), 50);
        BinaryPrimitives.WriteUInt16LittleEndian(selected.AsSpan(30), 9998);
        var result = RdffKeyInspection.AnalyzeLookChains(
            [new(30,other), Record(40,5,-40), new(10,selected)], Controls(2));
        Assert.Equal(2, result.Items);
        Assert.Equal(1, result.NamedItems);
        Assert.Equal(1, result.UnnamedItems);
        Assert.Equal(1, result.Type50);
        Assert.Equal(0, result.UnnamedType50);
        Assert.Equal(new ushort[] {17,50}, result.ItemTypes.Select(row => row.Type));
        Assert.Equal(new[] {1,1}, result.ItemTypes.Select(row => row.Items));
        Assert.Equal(new[] {0,1}, result.ItemTypes.Select(row => row.Named));
        Assert.Equal(new RdffKeyInspection.UnnamedItem(30,0,17,65535), Assert.Single(result.UnnamedRows));
    }

    [Theory]
    [InlineData("forward")]
    [InlineData("kind")]
    [InlineData("width")]
    [InlineData("type")]
    [InlineData("payload")]
    [InlineData("end")]
    [InlineData("limit")]
    public void LookChainsRejectMalformedLaterStateWithoutPartialSuccess(string fault)
    {
        var first = Record(10,1,-10).Data.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(20), 50);
        var later = first[..33].ToArray(); later[0] = 4;
        switch (fault)
        {
            case "forward": later[1] = 1; break;
            case "kind": later[2] = 9; break;
            case "width": later[8] = 24; break;
            case "type": later[0] = 1; break;
        }
        byte[] chain = [..first[..33], ..later, ..first[33..]];
        if (fault == "payload") chain = chain[..65];
        if (fault == "end") chain = chain[..75];
        if (fault == "limit") chain = [..first[..33], ..Enumerable.Range(0,80).SelectMany(_ => later), ..first[33..]];
        Assert.Throws<InvalidDataException>(() => RdffKeyInspection.AnalyzeLookChains(
            [new(10,chain),Record(20,5,-20)], Controls(1)));
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
