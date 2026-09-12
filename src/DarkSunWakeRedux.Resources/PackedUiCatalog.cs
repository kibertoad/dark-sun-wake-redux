using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedUiCatalog(
    IReadOnlyList<UiWindowResource> Windows,
    IReadOnlyList<UiButtonResource> Buttons,
    IReadOnlyList<UiApplicationFrameResource> ApplicationFrames,
    IReadOnlyList<UiEditBoxResource> EditBoxes)
{
    public const ushort FormatVersion = 1;
    public const long MaximumFileBytes = 4L * 1024 * 1024;
    public const int MaximumRecordsPerType = 4_096;
    private const int HeaderBytes = 14;

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite) throw new ArgumentException("UI catalog stream must be writable.", nameof(stream));
        Validate(this, "UI catalog");
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSUI"u8);
        writer.Write(FormatVersion);
        writer.Write(checked((ushort)Windows.Count));
        writer.Write(checked((ushort)Buttons.Count));
        writer.Write(checked((ushort)ApplicationFrames.Count));
        writer.Write(checked((ushort)EditBoxes.Count));
        foreach (var window in Windows.OrderBy(item => item.ResourceNumber))
        {
            writer.Write(window.ResourceNumber);
            writer.Write(window.ImageResourceNumber);
            writer.Write(window.Width);
            writer.Write(window.Height);
            writer.Write(checked((ushort)window.Children.Count));
            foreach (var child in window.Children)
            {
                writer.Write(Encoding.ASCII.GetBytes(child.Tag));
                writer.Write(child.ResourceNumber);
                writer.Write(child.X);
                writer.Write(child.Y);
            }
        }
        foreach (var button in Buttons.OrderBy(item => item.ResourceNumber))
        {
            writer.Write(button.ResourceNumber);
            writer.Write(button.Width);
            writer.Write(button.Height);
            writer.Write(button.ImageResourceNumber);
            writer.Write(button.EventMask);
        }
        foreach (var frame in ApplicationFrames.OrderBy(item => item.ResourceNumber))
        {
            writer.Write(frame.ResourceNumber);
            writer.Write(frame.Width);
            writer.Write(frame.Height);
            writer.Write(frame.EventMask);
        }
        foreach (var editBox in EditBoxes.OrderBy(item => item.ResourceNumber))
        {
            writer.Write(editBox.ResourceNumber);
            writer.Write(editBox.Width);
            writer.Write(editBox.Height);
            writer.Write(editBox.EventMask);
        }
    }

    public static PackedUiCatalog Read(Stream stream, string sourceName = "UI catalog")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes) throw Error(sourceName, "is shorter than the header");
        if (stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"exceeds the {MaximumFileBytes}-byte safety limit");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        var reader = new Reader(bytes, sourceName);
        if (!reader.Bytes(4, "signature").SequenceEqual("DSUI"u8))
            throw Error(sourceName, "has an invalid signature");
        var version = reader.UInt16("version");
        if (version != FormatVersion) throw Error(sourceName, $"uses unsupported format version {version}");
        var windowCount = reader.Count("window count");
        var buttonCount = reader.Count("button count");
        var frameCount = reader.Count("application-frame count");
        var editBoxCount = reader.Count("edit-box count");

        var windows = new UiWindowResource[windowCount];
        for (var index = 0; index < windows.Length; index++)
        {
            var number = reader.UInt32($"window {index} number");
            var image = reader.UInt32($"window {index} image number");
            var width = reader.UInt16($"window {index} width");
            var height = reader.UInt16($"window {index} height");
            var childCount = reader.Count($"window {index} child count");
            var children = new UiChildReference[childCount];
            for (var child = 0; child < children.Length; child++)
                children[child] = new(reader.Tag($"window {index} child {child} tag"),
                    reader.UInt32($"window {index} child {child} number"),
                    reader.Int16($"window {index} child {child} x"),
                    reader.Int16($"window {index} child {child} y"));
            windows[index] = new(number, image, width, height, children);
        }
        var buttons = new UiButtonResource[buttonCount];
        for (var index = 0; index < buttons.Length; index++)
            buttons[index] = new(reader.UInt32($"button {index} number"),
                reader.UInt16($"button {index} width"), reader.UInt16($"button {index} height"),
                reader.UInt32($"button {index} image number"), reader.UInt16($"button {index} event mask"));
        var frames = new UiApplicationFrameResource[frameCount];
        for (var index = 0; index < frames.Length; index++)
            frames[index] = new(reader.UInt32($"application frame {index} number"),
                reader.UInt16($"application frame {index} width"),
                reader.UInt16($"application frame {index} height"),
                reader.UInt16($"application frame {index} event mask"));
        var editBoxes = new UiEditBoxResource[editBoxCount];
        for (var index = 0; index < editBoxes.Length; index++)
            editBoxes[index] = new(reader.UInt32($"edit box {index} number"),
                reader.UInt16($"edit box {index} width"), reader.UInt16($"edit box {index} height"),
                reader.UInt16($"edit box {index} event mask"));
        reader.RequireEnd();
        var catalog = new PackedUiCatalog(windows, buttons, frames, editBoxes);
        Validate(catalog, sourceName);
        return catalog;
    }

    private static void Validate(PackedUiCatalog catalog, string sourceName)
    {
        ArgumentNullException.ThrowIfNull(catalog.Windows);
        ArgumentNullException.ThrowIfNull(catalog.Buttons);
        ArgumentNullException.ThrowIfNull(catalog.ApplicationFrames);
        ArgumentNullException.ThrowIfNull(catalog.EditBoxes);
        CheckCount(catalog.Windows.Count, "windows", sourceName);
        CheckCount(catalog.Buttons.Count, "buttons", sourceName);
        CheckCount(catalog.ApplicationFrames.Count, "application frames", sourceName);
        CheckCount(catalog.EditBoxes.Count, "edit boxes", sourceName);
        CheckUnique(catalog.Windows.Select(item => item.ResourceNumber), "window", sourceName);
        CheckUnique(catalog.Buttons.Select(item => item.ResourceNumber), "button", sourceName);
        CheckUnique(catalog.ApplicationFrames.Select(item => item.ResourceNumber), "application frame", sourceName);
        CheckUnique(catalog.EditBoxes.Select(item => item.ResourceNumber), "edit box", sourceName);
        var buttonIds = catalog.Buttons.Select(item => item.ResourceNumber).ToHashSet();
        var frameIds = catalog.ApplicationFrames.Select(item => item.ResourceNumber).ToHashSet();
        var editBoxIds = catalog.EditBoxes.Select(item => item.ResourceNumber).ToHashSet();
        foreach (var window in catalog.Windows)
        {
            Dimensions(window.Width, window.Height, $"window {window.ResourceNumber}", sourceName);
            if (window.Children is null || window.Children.Count > UiWindowResource.MaximumChildren)
                throw Error(sourceName, $"window {window.ResourceNumber} has an invalid child count");
            foreach (var child in window.Children)
            {
                var exists = child.Tag switch
                {
                    "BUTN" => buttonIds.Contains(child.ResourceNumber),
                    "APFM" => frameIds.Contains(child.ResourceNumber),
                    "EBOX" => editBoxIds.Contains(child.ResourceNumber),
                    _ => throw Error(sourceName, $"window {window.ResourceNumber} has unsupported child tag '{child.Tag}'")
                };
                if (!exists)
                    throw Error(sourceName, $"window {window.ResourceNumber} references missing {child.Tag} #{child.ResourceNumber}");
            }
        }
        foreach (var item in catalog.Buttons)
            Dimensions(item.Width, item.Height, $"button {item.ResourceNumber}", sourceName);
        foreach (var item in catalog.ApplicationFrames)
            Dimensions(item.Width, item.Height, $"application frame {item.ResourceNumber}", sourceName);
        foreach (var item in catalog.EditBoxes)
            Dimensions(item.Width, item.Height, $"edit box {item.ResourceNumber}", sourceName);
    }

    private static void CheckCount(int count, string field, string sourceName)
    {
        if (count < 0 || count > MaximumRecordsPerType)
            throw Error(sourceName, $"contains too many {field}");
    }

    private static void CheckUnique(IEnumerable<uint> values, string field, string sourceName)
    {
        var seen = new HashSet<uint>();
        foreach (var value in values)
            if (!seen.Add(value)) throw Error(sourceName, $"contains duplicate {field} #{value}");
    }

    private static void Dimensions(ushort width, ushort height, string field, string sourceName)
    {
        if (width is 0 or > IndexedImage.MaximumDimension || height is 0 or > IndexedImage.MaximumDimension)
            throw Error(sourceName, $"{field} has invalid dimensions {width}x{height}");
    }

    private static InvalidDataException Error(string sourceName, string message) => new($"{sourceName}: {message}.");

    private sealed class Reader(byte[] bytes, string sourceName)
    {
        private int _position;

        public ReadOnlySpan<byte> Bytes(int count, string field)
        {
            Ensure(count, field);
            var result = bytes.AsSpan(_position, count);
            _position += count;
            return result;
        }

        public ushort UInt16(string field) => BinaryPrimitives.ReadUInt16LittleEndian(Bytes(2, field));
        public short Int16(string field) => BinaryPrimitives.ReadInt16LittleEndian(Bytes(2, field));
        public uint UInt32(string field) => BinaryPrimitives.ReadUInt32LittleEndian(Bytes(4, field));

        public int Count(string field)
        {
            var count = UInt16(field);
            if (count > MaximumRecordsPerType) throw Error(sourceName, $"{field} exceeds the safety limit");
            return count;
        }

        public string Tag(string field)
        {
            var value = Bytes(4, field);
            foreach (var item in value)
                if (item is < 0x20 or > 0x7e)
                    throw Error(sourceName, $"contains a non-printable {field}");
            return Encoding.ASCII.GetString(value);
        }

        public void RequireEnd()
        {
            if (_position != bytes.Length) throw Error(sourceName, "contains trailing bytes");
        }

        private void Ensure(int count, string field)
        {
            if (count < 0 || (long)_position + count > bytes.Length)
                throw Error(sourceName, $"is truncated while reading {field}");
        }
    }
}
