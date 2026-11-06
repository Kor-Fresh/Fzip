namespace Fzip.Core;

public sealed record ArchiveItem(string Path, long Size, long PackedSize, bool IsDirectory, bool IsEncrypted = false)
{
    public string Kind => IsDirectory ? "폴더" : IsEncrypted ? "암호화 파일" : "파일";
    public string DisplaySize => IsDirectory ? "—" : FormatSize(Size);
    public static string FormatSize(long bytes) => bytes >= 1L << 30 ? $"{bytes / (double)(1L << 30):0.##} GB" : bytes >= 1 << 20 ? $"{bytes / (double)(1 << 20):0.##} MB" : bytes >= 1024 ? $"{bytes / 1024d:0.#} KB" : $"{bytes:N0} B";
}

public sealed record JobRequest(string Operation, string? Archive = null, string? Destination = null,
    string[]? Inputs = null, string Format = "zip", int Level = 5);
public sealed record JobEvent(string Type, string Message = "", double Percent = 0,
    List<ArchiveItem>? Items = null, string? Output = null);
