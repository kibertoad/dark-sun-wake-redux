using System.Text.Json;
using RefurbishedDinosaurs.Media.Fli;

internal static class FliInspection
{
    // FMT-VIDEO-001: format readiness check, not a claim about original decoder fidelity.
    internal static int Run(string path)
    {
        try
        {
            using var source = File.OpenRead(path);
            using var movie = new FliMovieStream(source);
            var surface = new FliSurface(movie.Width, movie.Height);
            var frame = new byte[movie.MaximumFrameLength];
            for (var index = 0; index < movie.FrameCount; index++)
            {
                var count = movie.ReadFrame(index, frame);
                surface.DecodeFrame(frame.AsSpan(0, count));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                movie.Width, movie.Height, movie.FrameCount,
                headerFrameMilliseconds = movie.HeaderFrameDuration?.TotalMilliseconds,
                result = "decoded-with-shared-format-library"
            }));
            return 0;
        }
        catch (Exception error) when (error is InvalidDataException or NotSupportedException
            or IOException or UnauthorizedAccessException or ArgumentException)
        {
            Console.Error.WriteLine($"[fli_unavailable] {error.Message}");
            return 2;
        }
    }
}
