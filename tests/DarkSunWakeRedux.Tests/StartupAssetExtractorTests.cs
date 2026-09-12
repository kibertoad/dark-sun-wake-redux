using System.Security.Cryptography;
using System.Text;
using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartupAssetExtractorTests
{
    [Fact]
    public void StartMenuMappingsAreUniqueAndInsideLogicalCanvas()
    {
        Assert.Equal(4, OriginalContent.StartMenuButtons.Count);
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.Path).Distinct().Count());
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.ImageResourceNumber).Distinct().Count());
        Assert.All(OriginalContent.StartMenuButtons, button =>
        {
            Assert.InRange(button.X, 0, 319);
            Assert.InRange(button.Y, 0, 199);
            Assert.InRange(button.X + button.Width, 1, 320);
            Assert.InRange(button.Y + button.Height, 1, 200);
        });
    }

    [Fact]
    public async Task InstallsMappedStartupImagesAsVerifiedDerivedAssets()
    {
        var root = Path.Combine(Path.GetTempPath(), "dark-sun-title-tests", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var output = Path.Combine(root, "pack");
        Directory.CreateDirectory(sourceRoot);
        try
        {
            var sourcePath = Path.Combine(sourceRoot, StartupAssetExtractor.SourcePath);
            await File.WriteAllBytesAsync(sourcePath, StartupArchive(), TestContext.Current.CancellationToken);
            string hash;
            await using (var sourceStream = File.OpenRead(sourcePath))
                hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(
                    sourceStream, TestContext.Current.CancellationToken));
            var edition = new SourceManifest(OriginalContent.GameId, "synthetic-title-edition",
                [new SourceFile(StartupAssetExtractor.SourcePath, new FileInfo(sourcePath).Length, hash)]);

            var manifest = await AssetPackInstaller.InstallAsync(output, staging =>
                StartupAssetExtractor.WritePackAsync(sourceRoot, staging, edition, "test"));

            Assert.Equal(6, manifest.Files.Count);
            var asset = Assert.Single(manifest.Files, item => item.Path == OriginalContent.TitleImageAssetPath);
            Assert.Contains("BMP #11011", asset.Conversion, StringComparison.Ordinal);
            foreach (var button in OriginalContent.StartMenuButtons)
            {
                var buttonAsset = Assert.Single(manifest.Files, item => item.Path == button.Path);
                Assert.Contains($"ICON#{button.ImageResourceNumber}", buttonAsset.Conversion, StringComparison.Ordinal);
                using var buttonStream = File.OpenRead(Path.Combine(output,
                    button.Path.Replace('/', Path.DirectorySeparatorChar)));
                Assert.Equal(4, PackedIndexedImage.Read(buttonStream).Frames.Count);
            }
            var fontAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.InterfaceFontAssetPath);
            Assert.Contains("FONT#100", fontAsset.Conversion, StringComparison.Ordinal);
            using (var fontStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.InterfaceFontAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                Assert.Equal(IndexedBitmapFont.CharacterCount,
                    PackedIndexedBitmapFont.Read(fontStream).Glyphs.Count);
            }
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

    private static byte[] StartupArchive()
    {
        var title = TransparentImage(320, 200);
        var icons = OriginalContent.StartMenuButtons
            .Select(button => (button.ImageResourceNumber,
                Bytes: TransparentImage(Enumerable.Range(0, 4).Select(index =>
                    index == 2 && button.ThirdFrameIsPlaceholder
                        ? (Width: 1, Height: 1)
                        : (button.Width, button.Height)).ToArray())))
            .ToArray();
        var palette = new byte[IndexedPalette.EncodedLength];
        var font = Font();
        var indexOffset = 28 + title.Length + icons.Sum(item => item.Bytes.Length) + palette.Length + font.Length;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("GFFI"));
        writer.Write(0x0003_0000U);
        writer.Write(28U);
        writer.Write((uint)indexOffset);
        writer.Write(new byte[12]);
        var titleOffset = checked((int)stream.Position);
        writer.Write(title);
        var iconEntries = new List<(uint Number, int Offset, int Size)>();
        foreach (var icon in icons)
        {
            var offset = checked((int)stream.Position);
            writer.Write(icon.Bytes);
            iconEntries.Add((icon.ImageResourceNumber, offset, icon.Bytes.Length));
        }
        var paletteOffset = checked((int)stream.Position);
        writer.Write(palette);
        var fontOffset = checked((int)stream.Position);
        writer.Write(font);
        writer.Write(0U);
        writer.Write(0U);
        writer.Write((ushort)4);
        WriteTable(writer, "BMP ", [(StartupAssetExtractor.TitleImageNumber, titleOffset, title.Length)]);
        WriteTable(writer, "ICON", iconEntries);
        WriteTable(writer, "PAL ", [(StartupAssetExtractor.PaletteNumber, paletteOffset, palette.Length)]);
        WriteTable(writer, "FONT", [(StartupAssetExtractor.FontNumber, fontOffset, font.Length)]);
        return stream.ToArray();
    }

    private static byte[] Font()
    {
        const ushort height = 1;
        var bytes = new byte[IndexedBitmapFont.HeaderSize + IndexedBitmapFont.CharacterCount * 2];
        BitConverter.GetBytes((ushort)IndexedBitmapFont.CharacterCount).CopyTo(bytes, 0);
        BitConverter.GetBytes(height).CopyTo(bytes, 2);
        for (var index = 0; index < IndexedBitmapFont.CharacterCount; index++)
        {
            bytes[8 + index] = (byte)index;
            var offset = IndexedBitmapFont.HeaderSize + index * 2;
            BitConverter.GetBytes(checked((ushort)offset)).CopyTo(bytes, 264 + index * 2);
        }
        return bytes;
    }

    private static byte[] TransparentImage(int width, int height) =>
        TransparentImage([(width, height)]);

    private static byte[] TransparentImage(IReadOnlyList<(int Width, int Height)> frames)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        var tableEnd = 6 + frames.Count * 4;
        var totalSize = tableEnd + frames.Count * 10;
        writer.Write((uint)totalSize);
        writer.Write((ushort)frames.Count);
        for (var index = 0; index < frames.Count; index++) writer.Write((uint)(tableEnd + index * 10));
        foreach (var frame in frames)
        {
            writer.Write((ushort)frame.Width);
            writer.Write((ushort)frame.Height);
            writer.Write((byte)0xff);
            writer.Write(Encoding.ASCII.GetBytes("PLAN"));
            writer.Write((byte)0);
        }
        return stream.ToArray();
    }

    private static void WriteTable(
        BinaryWriter writer,
        string tag,
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
}
