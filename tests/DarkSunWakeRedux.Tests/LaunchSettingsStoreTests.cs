using System.Text;
using DarkSunWakeRedux.Game;
using Xunit;

namespace DarkSunWakeRedux.Tests;

// The settings file of the launch options screen (DEV-UI-001), which holds the Wide map view
// setting of DEV-EXPLORE-001.
public sealed class LaunchSettingsStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), $"dark-sun-wake-settings-{Guid.NewGuid():N}");

    private string SettingsPath => Path.Combine(_directory, LaunchSettingsStore.FileName);
    private string BackupPath => SettingsPath + LaunchSettingsStore.BackupSuffix;

    public LaunchSettingsStoreTests() => Directory.CreateDirectory(_directory);

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }

    [Fact]
    public void DefaultsTurnWideMapViewOn() =>
        Assert.True(LaunchSettings.Default.WideMapView);

    [Fact]
    public void DefaultPathSitsBesideUserContent()
    {
        var path = LaunchSettingsStore.DefaultPath();
        Assert.Equal(LaunchSettingsStore.FileName, Path.GetFileName(path));
        Assert.Equal(
            Path.GetDirectoryName(DarkSunWakeRedux.Resources.OriginalContent.DefaultAssetPackPath()),
            Path.GetDirectoryName(path));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RoundTripsEveryValue(bool wideMapView)
    {
        LaunchSettingsStore.WriteAtomic(SettingsPath, new(wideMapView));

        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.Equal(new LaunchSettings(wideMapView), loaded.Settings);
        Assert.False(loaded.RecoveredFromBackup);
        Assert.False(loaded.UsedDefaults);
        Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
    }

    [Fact]
    public void WritesTheVersionAndTheField()
    {
        var text = Encoding.UTF8.GetString(LaunchSettingsStore.Serialize(new(false)));

        Assert.Contains("\"version\": 1", text);
        Assert.Contains("\"wideMapView\": false", text);
    }

    [Fact]
    public void MissingFilesGiveTheDefaults()
    {
        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.Equal(LaunchSettings.Default, loaded.Settings);
        Assert.True(loaded.UsedDefaults);
        Assert.False(loaded.RecoveredFromBackup);
    }

    [Fact]
    public void SecondWriteKeepsTheFirstAsBackup()
    {
        LaunchSettingsStore.WriteAtomic(SettingsPath, new(false));
        LaunchSettingsStore.WriteAtomic(SettingsPath, new(true));

        Assert.Contains("false", File.ReadAllText(BackupPath));
        Assert.True(LaunchSettingsStore.ReadOrDefault(SettingsPath).Settings.WideMapView);
    }

    [Fact]
    public void CorruptFileRecoversFromTheBackup()
    {
        LaunchSettingsStore.WriteAtomic(SettingsPath, new(false));
        LaunchSettingsStore.WriteAtomic(SettingsPath, new(false));
        File.WriteAllText(SettingsPath, "{ not json");

        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.False(loaded.Settings.WideMapView);
        Assert.True(loaded.RecoveredFromBackup);
        Assert.False(loaded.UsedDefaults);
    }

    [Fact]
    public void CorruptFileIsNeverKeptAsTheBackup()
    {
        LaunchSettingsStore.WriteAtomic(SettingsPath, new(false));
        LaunchSettingsStore.WriteAtomic(SettingsPath, new(false));
        File.WriteAllText(SettingsPath, "{ not json");

        LaunchSettingsStore.WriteAtomic(SettingsPath, new(true));

        Assert.Contains("false", File.ReadAllText(BackupPath));
    }

    [Fact]
    public void CorruptFileAndBackupGiveTheDefaults()
    {
        File.WriteAllText(SettingsPath, "{ not json");
        File.WriteAllBytes(BackupPath, [0xff, 0x00, 0x13]);

        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.Equal(LaunchSettings.Default, loaded.Settings);
        Assert.True(loaded.UsedDefaults);
    }

    [Fact]
    public void OversizedFileIsNotRead()
    {
        var padding = new string(' ', LaunchSettingsStore.MaximumBytes);
        File.WriteAllText(SettingsPath, "{\"version\":1,\"wideMapView\":false}" + padding);

        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.Equal(LaunchSettings.Default, loaded.Settings);
        Assert.True(loaded.UsedDefaults);
    }

    [Theory]
    [InlineData("{\"version\":2,\"wideMapView\":false}")]
    [InlineData("{\"version\":0,\"wideMapView\":false}")]
    [InlineData("{\"version\":\"1\",\"wideMapView\":false}")]
    [InlineData("{\"wideMapView\":false}")]
    [InlineData("[{\"version\":1,\"wideMapView\":false}]")]
    public void UnknownVersionFallsBackToTheBackup(string content)
    {
        File.WriteAllText(BackupPath, "{\"version\":1,\"wideMapView\":false}");
        File.WriteAllText(SettingsPath, content);

        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.False(loaded.Settings.WideMapView);
        Assert.True(loaded.RecoveredFromBackup);
    }

    [Fact]
    public void UnknownVersionFileIsDroppedWithoutABackup()
    {
        File.WriteAllText(BackupPath, "{\"version\":1,\"wideMapView\":false}");
        File.WriteAllText(SettingsPath, "{\"version\":2,\"wideMapView\":false}");

        LaunchSettingsStore.WriteAtomic(SettingsPath, new(true));

        Assert.Equal("{\"version\":1,\"wideMapView\":false}", File.ReadAllText(BackupPath));
        Assert.True(LaunchSettingsStore.ReadOrDefault(SettingsPath).Settings.WideMapView);
    }

    [Fact]
    public void OlderFileWithoutAFieldGetsThatFieldsDefault()
    {
        File.WriteAllText(SettingsPath, "{\"version\":1}");

        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.Equal(LaunchSettings.Default, loaded.Settings);
        Assert.True(loaded.UsedDefaults);
        Assert.False(loaded.RecoveredFromBackup);
    }

    [Theory]
    [InlineData("\"no\"")]
    [InlineData("0")]
    [InlineData("null")]
    public void OutOfRangeValueFallsBackFieldByField(string value)
    {
        File.WriteAllText(BackupPath, "{\"version\":1,\"wideMapView\":false}");
        File.WriteAllText(SettingsPath, $"{{\"version\":1,\"wideMapView\":{value}}}");

        var loaded = LaunchSettingsStore.ReadOrDefault(SettingsPath);

        Assert.False(loaded.Settings.WideMapView);
        Assert.True(loaded.RecoveredFromBackup);
    }

    [Fact]
    public void UnknownFieldsAreIgnored()
    {
        File.WriteAllText(SettingsPath, "{\"version\":1,\"wideMapView\":false,\"later\":3}");

        Assert.False(LaunchSettingsStore.ReadOrDefault(SettingsPath).Settings.WideMapView);
    }
}
