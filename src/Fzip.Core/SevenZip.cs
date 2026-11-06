using System.Diagnostics;
using System.Text;

namespace Fzip.Core;

public sealed class SevenZip(string executable)
{
    private Process Start(IEnumerable<string> args, string? cwd = null)
    {
        if (!File.Exists(executable)) throw new FileNotFoundException("7-Zip 엔진 파일이 없습니다. Fzip을 다시 설치해주세요.", executable);
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
        if (cwd != null) info.WorkingDirectory = cwd;
        foreach (var arg in args) info.ArgumentList.Add(arg);
        var p = Process.Start(info) ?? throw new IOException("압축 엔진을 시작할 수 없습니다.");
        p.StandardInput.Close(); // Never wait indefinitely for an interactive password prompt.
        return p;
    }

    private static void Kill(Process process) { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) { } }
    private static void Check(Process p, string error)
    {
        if (p.ExitCode != 0) throw new InvalidDataException($"7-Zip 작업 실패 (코드 {p.ExitCode}). 암호·분할 파일·파일 손상을 확인해주세요.\n{error[..Math.Min(error.Length, 1600)]}");
    }
    private static async Task<string> ReadBounded(StreamReader reader, int max, CancellationToken ct)
    {
        var result = new StringBuilder(); var buffer = new char[4096]; int n;
        while ((n = await reader.ReadAsync(buffer.AsMemory(), ct)) != 0)
        {
            if (result.Length + n > max) throw new InvalidDataException("엔진 출력이 허용 크기를 초과했습니다.");
            result.Append(buffer, 0, n);
        }
        return result.ToString();
    }

    public async Task<List<ArchiveItem>> List(string archive, CancellationToken ct)
    {
        using var p = Start(["l", "-slt", "-ba", "-sccUTF-8", "--", archive]);
        using var cancel = ct.Register(() => Kill(p));
        try
        {
            var err = ReadBounded(p.StandardError, 1_000_000, ct);
            var output = await ReadBounded(p.StandardOutput, 32_000_000, ct);
            await p.WaitForExitAsync(ct); Check(p, await err);
            return ParseListing(output);
        }
        finally { Kill(p); }
    }

    public static List<ArchiveItem> ParseListing(string output)
    {
        var entries = new List<ArchiveItem>(); var row = new Dictionary<string, string>();
        void Flush()
        {
            if (!row.TryGetValue("Path", out var path)) { row.Clear(); return; }
            string Get(string key) => row.GetValueOrDefault(key, "");
            var attr = Get("Attributes"); var mode = Get("Mode");
            if (Get("Symbolic Link") != "" || Get("Hard Link") != "" || mode.StartsWith('l') || attr.Contains("lrwx", StringComparison.Ordinal))
                throw new InvalidDataException("압축파일 안의 링크는 지원하지 않습니다.");
            var directory = Get("Folder") == "+" || attr.StartsWith('D') || path.EndsWith('/') || path.EndsWith('\\');
            long.TryParse(Get("Size"), out var size); long.TryParse(Get("Packed Size"), out var packed);
            entries.Add(new(path, size, packed, directory, Get("Encrypted") == "+")); row.Clear();
            if (entries.Count > PathPolicy.MaxEntries) throw new InvalidDataException("파일 수 한도를 초과했습니다.");
        }
        foreach (var line in output.Replace("\r", "").Split('\n'))
        {
            if (line.Length == 0) { Flush(); continue; }
            var at = line.IndexOf(" = ", StringComparison.Ordinal);
            if (at < 0) throw new InvalidDataException("줄바꿈이 포함되었거나 해석할 수 없는 압축 항목입니다.");
            if (!row.TryAdd(line[..at], line[(at + 3)..])) throw new InvalidDataException("모호한 압축 항목 메타데이터입니다.");
        }
        Flush(); return entries;
    }

    public async Task ExtractEntry(string archive, ArchiveItem item, Stream destination, Action<long> progress, CancellationToken ct)
    {
        using var p = Start(["x", "-so", "-spd", "-y", "--", archive, item.Path]);
        using var cancel = ct.Register(() => Kill(p));
        try
        {
            var error = ReadBounded(p.StandardError, 1_000_000, ct);
            var buffer = new byte[131072]; long written = 0; int n;
            while ((n = await p.StandardOutput.BaseStream.ReadAsync(buffer, ct)) > 0)
            {
                written += n;
                if (written > item.Size || written > PathPolicy.MaxExtractBytes) throw new InvalidDataException("파일 크기가 압축 메타데이터와 다릅니다.");
                await destination.WriteAsync(buffer.AsMemory(0, n), ct); progress(written);
            }
            await p.WaitForExitAsync(ct); Check(p, await error);
            if (written != item.Size) throw new InvalidDataException("해제된 파일 크기가 일치하지 않습니다.");
        }
        finally { Kill(p); }
    }

    public async Task Compress(string output, string[] inputs, string format, int level, CancellationToken ct)
    {
        if (format is not ("zip" or "7z")) throw new ArgumentException("초기 버전의 생성 형식은 ZIP과 7z입니다.");
        if (inputs.Length == 0) throw new ArgumentException("압축할 항목을 선택해주세요.");
        inputs = inputs.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var cwd = Path.GetDirectoryName(inputs[0])!;
        if (inputs.Any(x => !string.Equals(Path.GetDirectoryName(x), cwd, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("초기 버전에서는 같은 폴더 안의 항목들을 선택해주세요.");
        foreach (var input in inputs)
        {
            if (!File.Exists(input) && !Directory.Exists(input)) throw new FileNotFoundException("입력 파일이 없습니다.", input);
            PathPolicy.RejectReparseAncestors(input);
            if (Path.GetFullPath(output).StartsWith(input.TrimEnd('\\') + '\\', StringComparison.OrdinalIgnoreCase) || string.Equals(output, input, StringComparison.OrdinalIgnoreCase))
                throw new IOException("압축 결과를 입력 폴더 안에 저장할 수 없습니다.");
            if (Directory.Exists(input))
            {
                var pending = new Stack<string>(); pending.Push(input); int count = 0;
                while (pending.TryPop(out var dir))
                    foreach (var child in Directory.EnumerateFileSystemEntries(dir))
                    {
                        ct.ThrowIfCancellationRequested();
                        var a = File.GetAttributes(child);
                        if ((a & FileAttributes.ReparsePoint) != 0) throw new IOException("링크가 포함된 폴더는 압축할 수 없습니다.");
                        if ((a & FileAttributes.Directory) != 0) pending.Push(child);
                        if (++count > PathPolicy.MaxEntries) throw new IOException("파일 수 한도를 초과했습니다.");
                    }
            }
        }
        var args = new List<string> { "a", "-t" + format, "-mx=" + Math.Clamp(level, 0, 9), "-sccUTF-8", "-spd", "--", output };
        args.AddRange(inputs.Select(x => ".\\" + Path.GetFileName(x)));
        using var p = Start(args, cwd); using var cancel = ct.Register(() => Kill(p));
        try
        {
            var stdout = ReadBounded(p.StandardOutput, 4_000_000, ct); var stderr = ReadBounded(p.StandardError, 1_000_000, ct);
            await Task.WhenAll(stdout, stderr); await p.WaitForExitAsync(ct); Check(p, await stderr);
        }
        finally { Kill(p); }
    }
}
