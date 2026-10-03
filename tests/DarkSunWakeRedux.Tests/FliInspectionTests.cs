using System.Buffers.Binary;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class FliInspectionTests
{
    [Theory]
    [InlineData(0xAF11, 10, 0)]
    [InlineData(0xAF12, 10, 2)]
    [InlineData(0xAF11, 1000, 2)]
    public void ReadinessCheckAcceptsFliAndRejectsUnsupportedOrOverrunningChunks(int magic, int chunkLength, int expected)
    {
        // Synthetic 2x2 COPY frame; no original game media.
        var bytes = new byte[154];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, bytes.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), (ushort)magic);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(6), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(10), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(12), 8);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(16), 7);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(128), 26);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(132), 0xF1FA);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(134), 1);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(144), chunkLength);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(148), 16);
        bytes[150] = 1; bytes[151] = 2; bytes[152] = 3; bytes[153] = 4;
        var path = Path.Combine(Path.GetTempPath(), $"fli-readiness-{Guid.NewGuid():N}.fli");
        try
        {
            File.WriteAllBytes(path, bytes);
            Assert.Equal(expected, FliInspection.Run(path));
        }
        finally { File.Delete(path); }
    }
}
