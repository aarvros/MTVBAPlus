using System.IO;
using System.Reflection;
using System.Diagnostics;

public static class FFmpegHelper{
    private static string? _extractedPath;

    public static string ExtractFfmpeg(){
        if (_extractedPath != null)
            return _extractedPath;

        string resourceName = "MTVBAPlus.ffmpeg.ffmpeg.exe";

        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException("Embedded ffmpeg.exe not found.");

        string tempPath = Path.Combine(Path.GetTempPath(), "ffmpeg_embedded.exe");

        using FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fs);

        _extractedPath = tempPath;
        return _extractedPath;
    }

    public static bool TrimVideo(string inputPath, string outputPath, double startMs, double endMs){
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input file not found.", inputPath);

        string ffmpegPath = ExtractFfmpeg();

        string start    = TimeSpan.FromMilliseconds(startMs).ToString(@"hh\:mm\:ss\.fff");
        string duration = TimeSpan.FromMilliseconds(endMs - startMs).ToString(@"hh\:mm\:ss\.fff");

        var args = $"-hide_banner -y -ss {start} -i \"{inputPath}\" -t {duration} -c:v libx264 -preset fast -c:a aac -avoid_negative_ts make_zero -reset_timestamps 1 \"{outputPath}\"";

        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode == 0;
    }
}