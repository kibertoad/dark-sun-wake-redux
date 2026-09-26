using System.IO.Hashing;

namespace DarkSunWakeRedux.Resources;

/// <summary>
/// The hash that identifies original files, source fingerprints and extracted assets: the 128-bit
/// form of xxHash3 (<c>XXH3_128bits</c>, which <c>xxhsum -H2</c> prints), written as 32 lower-case
/// hex digits in the byte order of its canonical form. The documentation standard names every file
/// a spec build manifest lists by the same hash, so a value recorded here can be copied there
/// unchanged.
/// </summary>
public static class ContentHash
{
    public const int HexLength = 32;

    public static string Xxh3(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(XxHash128.Hash(bytes));

    public static async Task<string> Xxh3Async(Stream stream, CancellationToken cancellationToken = default)
    {
        var hash = new XxHash128();
        await hash.AppendAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hash.GetCurrentHash());
    }

    public static bool IsValid(string? value) =>
        value is { Length: HexLength } && value.All(Uri.IsHexDigit);
}
