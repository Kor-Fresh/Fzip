using Fzip.Core;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Fzip.App;

public sealed class WorkerClient
{
    public async Task<JobEvent> Run(JobRequest request, IProgress<JobEvent> progress, CancellationToken ct)
    {
        var exe = Path.Combine(AppContext.BaseDirectory, "worker", "Fzip.Worker.exe");
        if (!File.Exists(exe)) throw new FileNotFoundException("작업 프로세스가 없습니다. 설치 패키지를 다시 설치해주세요.", exe);
        using var p = Process.Start(new ProcessStartInfo(exe)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true,
            RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardInputEncoding = new UTF8Encoding(false)
        }) ?? throw new IOException("작업 프로세스를 시작하지 못했습니다.");
        await p.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request));
        await p.StandardInput.FlushAsync();
        var stderr = p.StandardError.ReadToEndAsync();
        using var registration = ct.Register(() =>
        {
            try { p.StandardInput.WriteLine("cancel"); p.StandardInput.Flush(); } catch (Exception e) when (e is IOException or InvalidOperationException or ObjectDisposedException) { }
        });
        // Cooperative cancellation preserves cleanup; terminate hung engines after a grace period.
        var watchdog = Task.Run(async () =>
        {
            while (!p.HasExited && !ct.IsCancellationRequested) await Task.Delay(100);
            if (ct.IsCancellationRequested)
            {
                await Task.Delay(8000);
                try { if (!p.HasExited) p.Kill(true); } catch (InvalidOperationException) { }
            }
        });
        JobEvent? final = null;
        try
        {
            while (await p.StandardOutput.ReadLineAsync() is { } line)
            {
                var e = JsonSerializer.Deserialize<JobEvent>(line) ?? throw new InvalidDataException("잘못된 작업 응답입니다.");
                progress.Report(e);
                if (e.Type is "done" or "listed" or "error" or "cancelled") final = e;
            }
            await p.WaitForExitAsync();
            if (ct.IsCancellationRequested || final?.Type == "cancelled") throw new OperationCanceledException(ct);
            if (final?.Type == "error") throw new InvalidOperationException(final.Message);
            if (p.ExitCode != 0 || final == null) throw new IOException("작업 프로세스가 종료되었습니다. " + (await stderr)[..Math.Min((await stderr).Length, 500)]);
            return final;
        }
        finally
        {
            try { if (!p.HasExited) p.Kill(true); } catch (InvalidOperationException) { }
            await watchdog;
        }
    }
}
