using System.Text.RegularExpressions;

namespace Fzip.Core;

public static class PathPolicy
{
    public const long MaxExtractBytes = 20L * 1024 * 1024 * 1024;
    public const int MaxEntries = 100_000;

    public static string Resolve(string root, string entry)
    {
        var path = entry.Replace('/', '\\');
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || path.Any(char.IsControl))
            throw new InvalidDataException($"안전하지 않은 압축 경로입니다: {entry}");
        var parts = path.TrimEnd('\\').Split('\\');
        foreach (var part in parts)
        {
            if (part is "" or "." or ".." || part.EndsWith('.') || part.EndsWith(' ') ||
                part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                Regex.IsMatch(part, @"^(CON|PRN|AUX|NUL|COM[0-9¹²³]|LPT[0-9¹²³])(?:\.|$)", RegexOptions.IgnoreCase))
                throw new InvalidDataException($"Windows에서 안전하게 만들 수 없는 경로입니다: {entry}");
        }
        var prefix = Path.GetFullPath(root).TrimEnd('\\') + '\\';
        var full = Path.GetFullPath(Path.Combine(prefix, Path.Combine(parts)));
        if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("출력 폴더 이탈을 차단했습니다.");
        return full;
    }

    public static void ValidateItems(string root, IEnumerable<ArchiveItem> items)
    {
        var paths = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        long total = 0;
        foreach (var item in items)
        {
            var path = Resolve(root, item.Path);
            if (!paths.TryAdd(path, item.IsDirectory)) throw new InvalidDataException("중복되거나 대소문자가 충돌하는 파일명이 있습니다.");
            if (item.Size < 0 || item.Size > MaxExtractBytes - total) throw new InvalidDataException("초기 버전의 해제 크기 한도는 20 GB입니다.");
            total += item.Size;
            if (paths.Count > MaxEntries) throw new InvalidDataException("항목 수가 100,000개 한도를 초과했습니다.");
        }
        foreach (var path in paths.Keys)
            for (var parent = Path.GetDirectoryName(path); parent != null && parent.Length > root.Length; parent = Path.GetDirectoryName(parent))
                if (paths.TryGetValue(parent, out var directory) && !directory) throw new InvalidDataException("파일과 폴더 경로가 충돌합니다.");
    }

    public static void RejectReparseAncestors(string path)
    {
        for (var p = Path.GetFullPath(path); p != null; p = Path.GetDirectoryName(p))
            if ((File.Exists(p) || Directory.Exists(p)) && (File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("링크 또는 연결 지점 경로는 초기 버전에서 지원하지 않습니다.");
    }
}
