using EggDotNet;
using System.Buffers.Binary;

namespace Fzip.Core;

public sealed class ArchiveService(string sevenZipPath)
{
    static ArchiveService() => System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    private readonly SevenZip seven = new(sevenZipPath);
    private static readonly uint[] CrcTable = Enumerable.Range(0, 256).Select(i =>
    { uint c = (uint)i; for (var k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xedb88320U ^ (c >> 1) : c >> 1; return c; }).ToArray();

    private static bool IsEgg(string path)
    {
        using var stream = File.OpenRead(path); Span<byte> magic = stackalloc byte[4];
        return stream.Read(magic) == 4 && BinaryPrimitives.ReadUInt32LittleEndian(magic) is 0x41474745 or 0x015a4c41;
    }
    private static ArchiveItem Describe(EggArchiveEntry e)
    {
        var name = e.FullName ?? throw new InvalidDataException("이름 없는 ALZ/EGG 항목입니다.");
        var attr = e.GetExtraAttributes(ExtraAttributeType.FileAttibutes);
        if (e.EntryInfoType == EntryInfoType.Posix && (attr & 0xf000) is not (0 or 0x4000 or 0x8000))
            throw new InvalidDataException("ALZ/EGG 안의 링크·특수 파일은 지원하지 않습니다.");
        if (e.EntryInfoType == EntryInfoType.Windows && (attr & 0x400) != 0) throw new InvalidDataException("링크 항목을 차단했습니다.");
        var dir = name.EndsWith('/') || name.EndsWith('\\') ||
            (e.EntryInfoType == EntryInfoType.Windows && (attr & 0x10) != 0) ||
            (e.EntryInfoType == EntryInfoType.Posix && (attr & 0xf000) == 0x4000);
        return new(name, e.UncompressedLength, e.CompressedLength, dir, e.IsEncrypted);
    }

    public async Task<List<ArchiveItem>> List(string path, CancellationToken ct)
    {
        PathPolicy.RejectReparseAncestors(path); ct.ThrowIfCancellationRequested();
        if (!IsEgg(path)) return await seven.List(path, ct);
        using var archive = EggFile.Open(path);
        if (archive.Entries.Count > PathPolicy.MaxEntries) throw new InvalidDataException("항목 수 한도를 초과했습니다.");
        return archive.Entries.Select(Describe).ToList();
    }

    public async Task Extract(string archive, string destination, Action<JobEvent> report, CancellationToken ct)
    {
        archive = Path.GetFullPath(archive); destination = Path.GetFullPath(destination);
        PathPolicy.RejectReparseAncestors(archive); PathPolicy.RejectReparseAncestors(destination);
        if (Directory.Exists(destination) || File.Exists(destination)) throw new IOException("해제 대상은 새 폴더여야 합니다. 기존 폴더는 변경하지 않습니다.");
        var parent = Path.GetDirectoryName(destination)!;
        if (!Directory.Exists(parent)) throw new DirectoryNotFoundException("출력 상위 폴더가 없습니다.");
        var stage = Path.Combine(parent, ".fzip-" + Guid.NewGuid().ToString("N"));
        var items = await List(archive, ct);
        PathPolicy.ValidateItems(stage, items);
        if (items.Any(x => x.IsEncrypted)) throw new NotSupportedException("암호화 압축파일 해제는 다음 버전에서 지원합니다.");
        var total = items.Sum(x => x.Size); long done = 0;
        Directory.CreateDirectory(stage);
        try
        {
            using var egg = IsEgg(archive) ? EggFile.Open(archive) : null;
            uint solidCrc = uint.MaxValue;
            for (var i = 0; i < items.Count; i++)
            {
                ct.ThrowIfCancellationRequested(); var item = items[i];
                var target = PathPolicy.Resolve(stage, item.Path);
                PathPolicy.RejectReparseAncestors(target);
                if (item.IsDirectory) { Directory.CreateDirectory(target); continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                using var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 131072, true);
                void Progress(long bytes) => report(new("progress", item.Path, total == 0 ? 0 : (done + bytes) * 100d / total));
                if (egg == null) await seven.ExtractEntry(archive, item, output, Progress, ct);
                else
                {
                    using var input = egg.Entries[i].Open(); var buffer = new byte[131072]; long size = 0; uint crc = uint.MaxValue; int n;
                    while ((n = await input.ReadAsync(buffer, ct)) != 0)
                    {
                        size += n;
                        if (size > item.Size || size > PathPolicy.MaxExtractBytes) throw new InvalidDataException("해제 크기 한도를 초과했습니다.");
                        for (var b = 0; b < n; b++)
                        {
                            crc = CrcTable[(crc ^ buffer[b]) & 255] ^ (crc >> 8);
                            if (egg.IsSolid) solidCrc = CrcTable[(solidCrc ^ buffer[b]) & 255] ^ (solidCrc >> 8);
                        }
                        await output.WriteAsync(buffer.AsMemory(0, n), ct); Progress(size);
                    }
                    if (size != item.Size || (!egg.IsSolid && ~crc != egg.Entries[i].Crc32)) throw new InvalidDataException($"파일 무결성 검사에 실패했습니다: {item.Path}");
                }
                done += item.Size;
            }
            // EggDotNet exposes the solid block CRC on the first entry, not per-file CRCs.
            if (egg is { IsSolid: true } && egg.Entries.Count > 0 && ~solidCrc != egg.Entries[0].Crc32)
                throw new InvalidDataException("EGG 솔리드 블록 무결성 검사에 실패했습니다. 지원되지 않는 블록 구성일 수 있습니다.");
            ct.ThrowIfCancellationRequested(); PathPolicy.RejectReparseAncestors(parent);
            Directory.Move(stage, destination);
        }
        finally { if (Directory.Exists(stage)) Directory.Delete(stage, true); }
        report(new("done", "압축을 모두 풀었습니다.", 100, Output: destination));
    }

    public async Task Compress(JobRequest request, Action<JobEvent> report, CancellationToken ct)
    {
        var output = Path.GetFullPath(request.Destination ?? throw new ArgumentException("저장 경로가 없습니다."));
        PathPolicy.RejectReparseAncestors(output);
        if (File.Exists(output) || Directory.Exists(output)) throw new IOException("기존 파일을 덮어쓸 수 없습니다. 새 이름을 선택해주세요.");
        var temp = Path.Combine(Path.GetDirectoryName(output)!, ".fzip-" + Guid.NewGuid().ToString("N") + "." + request.Format);
        try
        {
            report(new("progress", "파일을 압축하고 있습니다…", -1));
            await seven.Compress(temp, request.Inputs ?? [], request.Format, request.Level, ct);
            ct.ThrowIfCancellationRequested(); File.Move(temp, output);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
        report(new("done", "새 압축파일을 만들었습니다.", 100, Output: output));
    }
}
