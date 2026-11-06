using Fzip.Core;
using System.Text;
using System.Text.Json;

Console.InputEncoding = Encoding.UTF8; Console.OutputEncoding = new UTF8Encoding(false);
using var cancel = new CancellationTokenSource();
var lastProgress = DateTime.MinValue;
void Report(JobEvent e)
{
    if (e.Type == "progress" && e.Percent >= 0 && (DateTime.UtcNow - lastProgress).TotalMilliseconds < 100) return;
    lastProgress = DateTime.UtcNow;
    Console.WriteLine(JsonSerializer.Serialize(e));
}
try
{
    var line = await Console.In.ReadLineAsync();
    var request = JsonSerializer.Deserialize<JobRequest>(line ?? "") ?? throw new ArgumentException("작업 요청이 없습니다.");
    // A second line requests cancellation. EOF means the parent UI went away.
    _ = Task.Run(async () => { await Console.In.ReadLineAsync(); cancel.Cancel(); });
    var service = new ArchiveService(Path.Combine(AppContext.BaseDirectory, "engine", "7z.exe"));
    switch (request.Operation)
    {
        case "list": Report(new("listed", Items: await service.List(request.Archive!, cancel.Token))); break;
        case "extract": await service.Extract(request.Archive!, request.Destination!, Report, cancel.Token); break;
        case "compress": await service.Compress(request, Report, cancel.Token); break;
        default: throw new ArgumentException("알 수 없는 작업입니다.");
    }
    return 0;
}
catch (OperationCanceledException) { Report(new("cancelled", "작업을 취소했습니다.")); return 2; }
catch (Exception e) { Report(new("error", e.Message)); return 1; }
