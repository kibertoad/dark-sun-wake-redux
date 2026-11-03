using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class RdffTypeInspectionTests
{
    private static RdffTypeInspection.Controls Controls(int resources = 1, int inline = 1, int rows = 1) =>
        new()
        {
            SourceSha256 = new string('0', 64), ResourceCount = resources, InlineCount = inline,
            TypeRows = rows, ControlResource = 10, ControlReference = 0, ControlIndexedMatch = true
        };

    private static byte[] Table(params byte[] values) =>
        values.SelectMany(value => Enumerable.Repeat(value, 15)).ToArray();

    private static RdffKeyInspection.Resource Inline(uint number, ushort reference, byte[] payload)
    {
        Assert.Equal(15, payload.Length);
        var data = new byte[68];
        data[0] = 1; data[2] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(8), 23);
        data[33] = 3; data[35] = 4;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(37), reference);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(39), 5);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(41), 15);
        payload.CopyTo(data, 43);
        data[58] = 0xFF;
        return new(number, data);
    }

    [Fact]
    public void IndexedOtherUnmatchedAndOutOfRangeRelationsRemainDistinct()
    {
        var controls = Controls(4, 3, 3) with { ControlReference = 1 };
        var report = RdffTypeInspection.Analyze(new[]
        {
            Inline(12, ushort.MaxValue, Table(0x77)),
            new RdffKeyInspection.Resource(13, new byte[43]),
            Inline(11, 0, Table(0x33)), Inline(10, 1, Table(0x22))
        }, Table(0x11, 0x22, 0x33), controls);
        Assert.Equal(4, report.Resources);
        Assert.Equal(3, report.InlineRecords);
        Assert.Equal(1, report.OutsideModel);
        Assert.Equal(1, report.IndexedMatches);
        Assert.Equal(1, report.OtherRowMatches);
        Assert.Equal(1, report.NoRowMatches);
        Assert.Equal(1, report.OutOfRangeReferences);
        Assert.Equal(2, report.MatchingRelations);
        Assert.Equal(new uint[] {10,11,12}, report.Rows.Select(row => row.ResourceNumber));
        Assert.Equal(new[] {2}, report.Rows[1].MatchingRows);
        Assert.True(report.Rows[1].ReferenceInRange);
        Assert.False(report.Rows[1].IndexedMatch);
        Assert.False(report.Rows[2].ReferenceInRange);
        Assert.Equal(ushort.MaxValue, report.Rows[2].RecordReference);
    }

    [Fact]
    public void DuplicateSharedRowsAreReportedWithoutAssumingUniqueness()
    {
        var report = RdffTypeInspection.Analyze([Inline(10,0,Table(0x11))],
            Table(0x11,0x11), Controls(rows:2));
        Assert.True(report.Rows[0].IndexedMatch);
        Assert.Equal(new[] {0,1}, report.Rows[0].MatchingRows);
        Assert.Equal(2, report.MatchingRelations);
    }

    [Fact]
    public void WireControlsRequireAllFieldsAndRejectUnsupportedInput()
    {
        string valid = "{\"sourceSha256\":\"" + new string('0',64) +
            "\",\"resourceCount\":1,\"inlineCount\":1,\"typeRows\":1,\"controlResource\":10,\"controlReference\":0,\"controlIndexedMatch\":true}";
        Assert.Equal(Controls(), RdffTypeInspection.ParseControls(Encoding.UTF8.GetBytes(valid)));
        foreach (string json in new[] {"{}", "null", "[]", "{",
            valid[..^1] + ",\"extra\":1}", valid.Replace(",\"controlIndexedMatch\":true", "")})
        {
            var error = Xunit.Record.Exception(() => RdffTypeInspection.ParseControls(Encoding.UTF8.GetBytes(json)));
            Assert.True(error is JsonException or InvalidDataException, error?.ToString() ?? "accepted invalid controls");
        }
        Assert.Throws<InvalidDataException>(() => RdffTypeInspection.ParseControls([]));
        Assert.Throws<InvalidDataException>(() => RdffTypeInspection.ParseControls(new byte[65537]));
    }

    [Theory]
    [InlineData(0,2)]
    [InlineData(2,5)]
    [InlineData(3,1)]
    [InlineData(6,1)]
    [InlineData(8,22)]
    [InlineData(33,2)]
    [InlineData(34,1)]
    [InlineData(35,3)]
    [InlineData(36,1)]
    [InlineData(39,4)]
    [InlineData(41,14)]
    [InlineData(58,0)]
    public void UnsupportedFixedShapesAreRejected(int offset, byte value)
    {
        var data = Inline(10,0,Table(0x11)).Data.ToArray();
        data[offset] = value;
        var error = Assert.Throws<InvalidDataException>(() =>
            RdffTypeInspection.Analyze([new(10,data)], Table(0x11), Controls()));
        Assert.Contains("68-byte", error.Message);
    }

    [Fact]
    public void CountsSizesAndIndependentControlCannotBeSilentlyMissed()
    {
        var resource = Inline(10,0,Table(0x11));
        foreach (var controls in new[]
        {
            Controls(resources:0), Controls(resources:4097), Controls(inline:0),
            Controls(inline:2), Controls(rows:0), Controls(rows:4097),
            Controls() with {SourceSha256="bad"}, Controls() with {ControlResource=99},
            Controls() with {ControlReference=1}, Controls() with {ControlIndexedMatch=false},
            Controls(resources:2)
        })
            Assert.Throws<InvalidDataException>(() => RdffTypeInspection.Analyze([resource], Table(0x11), controls));
        foreach (var table in new[] {Table(0x11)[..14], Table(0x11).Concat(new byte[] {0}).ToArray()})
            Assert.Throws<InvalidDataException>(() => RdffTypeInspection.Analyze([resource], table, Controls()));
        foreach (int length in new[] {67,69})
            Assert.Throws<InvalidDataException>(() => RdffTypeInspection.Analyze(
                [new(10,new byte[length])], Table(0x11), Controls()));
        var oversized = new RdffKeyInspection.Resource(20,new byte[1024*1024+1]);
        var sizeError = Assert.Throws<InvalidDataException>(() => RdffTypeInspection.Analyze(
            [resource,oversized], Table(0x11), Controls(resources:2)));
        Assert.Contains("size", sizeError.Message);
        Assert.Throws<InvalidDataException>(() => RdffTypeInspection.Analyze([resource],
            ReadOnlyMemory<byte>.Empty, Controls(rows:0) with {ControlIndexedMatch=false}));
    }

    [Fact]
    public void DuplicateAndExcessiveResourceListsFailBeforeReporting()
    {
        var resource = Inline(10,0,Table(0x11));
        Assert.Throws<InvalidDataException>(() => RdffTypeInspection.Analyze(
            [resource,resource], Table(0x11), Controls(2,2)));
        var limitError = Assert.Throws<InvalidDataException>(() => RdffTypeInspection.Analyze(
            Enumerable.Repeat(resource,4097), Table(0x11), Controls()));
        Assert.Contains("resource limit", limitError.Message);
    }

    [Fact]
    public void RelationLimitRefusesInsteadOfTruncatingAmbiguousRows()
    {
        var resources = Enumerable.Range(10,33).Select(number => Inline((uint)number,0,Table(0x11)));
        var table = Enumerable.Repeat((byte)0x11,4096*15).ToArray();
        var error = Assert.Throws<InvalidDataException>(() =>
            RdffTypeInspection.Analyze(resources, table, Controls(33,33,4096)));
        Assert.Contains("relation limit", error.Message);
    }
}
