using System.Security.Cryptography;
using System.Text;
using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class TitleAssetExtractorTests
{
    [Fact]
    public async Task InstallsMappedTitleAsVerifiedDerivedAsset()
    {
        var root = Path.Combine(Path.GetTempPath(), "dark-sun-title-tests", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var output = Path.Combine(root, "pack");
        Directory.CreateDirectory(sourceRoot);
        try
        {
            var sourcePath = Path.Combine(sourceRoot, TitleAssetExtractor.SourcePath);
            await File.WriteAllBytesAsync(sourcePath, TitleArchive(), TestContext.Current.CancellationToken);
            string hash;
            await using (var sourceStream = File.OpenRead(sourcePath))
                hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(
                    sourceStream, TestContext.Current.CancellationToken));
            var edition = new SourceManifest(OriginalContent.GameId, "synthetic-title-edition",
                [new SourceFile(TitleAssetExtractor.SourcePath, new FileInfo(sourcePath).Length, hash)]);

            var manifest = await AssetPackInstaller.InstallAsync(output, staging =>
                TitleAssetExtractor.WritePackAsync(sourceRoot, staging, edition, "test"));

            var asset = Assert.Single(manifest.Files);
            Assert.Equal(OriginalContent.TitleImageAssetPath, asset.Path);
            Assert.Contains("BMP #11011", asset.Conversion, StringComparison.Ordinal);
            Assert.Empty(await OriginalContent.VerifyInstalledAsync(
                output, TestContext.Current.CancellationToken));
            using var packedStream = File.OpenRead(Path.Combine(output, "images", "title.dsix"));
            var packed = PackedIndexedImage.Read(packedStream);
            var frame = Assert.Single(packed.Frames);
            Assert.Equal(320, frame.Width);
            Assert.Equal(200, frame.Height);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static byte[] TitleArchive()
    {
        var image = TransparentImage();
        var palette = new byte[IndexedPalette.EncodedLength];
        var indexOffset = 28 + image.Length + palette.Length;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("GFFI"));
        writer.Write(0x0003_0000U);
        writer.Write(28U);
        writer.Write((uint)indexOffset);
        writer.Write(new byte[12]);
        writer.Write(image);
        writer.Write(palette);
        writer.Write(0U);
        writer.Write(0U);
        writer.Write((ushort)2);
        WriteTable(writer, "BMP ", TitleAssetExtractor.ImageNumber, 28, image.Length);
        WriteTable(writer, "PAL ", TitleAssetExtractor.PaletteNumber, 28 + image.Length, palette.Length);
        return stream.ToArray();
    }

    private static byte[] TransparentImage()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(20U);
        writer.Write((ushort)1);
        writer.Write(10U);
        writer.Write((ushort)320);
        writer.Write((ushort)200);
        writer.Write((byte)0xff);
        writer.Write(Encoding.ASCII.GetBytes("PLAN"));
        writer.Write((byte)0);
        return stream.ToArray();
    }

    private static void WriteTable(BinaryWriter writer, string tag, uint number, int offset, int size)
    {
        writer.Write(Encoding.ASCII.GetBytes(tag));
        writer.Write(1U);
        writer.Write(number);
        writer.Write((uint)offset);
        writer.Write((uint)size);
    }
}
