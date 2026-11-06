using Fzip.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Fzip.App;

public partial class MainWindow : Window
{
    private string? archivePath, resultPath;
    private CancellationTokenSource? cancellation;
    private bool busy;
    private readonly WorkerClient worker = new();
    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var args = App.LaunchArgs;
            if (args.Length == 0) return;
            if (args[0] == "--compress") { NewArchive(args.Skip(1).Where(x => File.Exists(x) || Directory.Exists(x)).ToArray()); return; }
            var path = args[0] == "--extract" ? args.ElementAtOrDefault(1) : args[0];
            if (path != null && File.Exists(path))
            {
                await OpenArchive(path);
                if (args[0] == "--extract" && archivePath != null) ExtractClick(this, new());
            }
        };
        Closing += (_, e) =>
        {
            if (!busy) return;
            e.Cancel = true; cancellation?.Cancel(); StatusText.Text = "작업을 정리하고 있습니다. 완료 후 창을 닫아주세요.";
        };
    }

    private async Task<JobEvent?> Run(JobRequest request)
    {
        if (busy) return null;
        busy = true; cancellation = new(); CancelButton.IsEnabled = true; ExtractButton.IsEnabled = false;
        Progress.Value = 0; Progress.IsIndeterminate = true; StatusText.Text = "작업을 준비하고 있습니다…";
        try
        {
            var result = await worker.Run(request, new Progress<JobEvent>(e =>
            {
                if (!string.IsNullOrEmpty(e.Message)) StatusText.Text = e.Message;
                if (e.Type == "progress") { Progress.IsIndeterminate = e.Percent < 0; if (e.Percent >= 0) Progress.Value = e.Percent; }
            }), cancellation.Token);
            if (result.Output != null) { resultPath = result.Output; FolderButton.IsEnabled = true; }
            Progress.Value = 100; return result;
        }
        catch (OperationCanceledException) { StatusText.Text = "작업을 취소했습니다."; return null; }
        catch (Exception e) { StatusText.Text = "작업을 완료하지 못했습니다."; MessageBox.Show(this, e.Message, "Fzip", MessageBoxButton.OK, MessageBoxImage.Warning); return null; }
        finally { busy = false; cancellation.Dispose(); cancellation = null; CancelButton.IsEnabled = false; Progress.IsIndeterminate = false; ExtractButton.IsEnabled = archivePath != null; }
    }

    private async Task OpenArchive(string path)
    {
        if (busy) return;
        archivePath = null; EntriesGrid.ItemsSource = null; EmptyState.Visibility = Visibility.Visible;
        ArchiveTitle.Text = "압축파일을 읽고 있습니다…"; ArchiveSummary.Text = path;
        var result = await Run(new("list", Archive: Path.GetFullPath(path)));
        if (result?.Items == null) { ArchiveTitle.Text = "압축파일을 열지 못했습니다"; return; }
        archivePath = Path.GetFullPath(path); ArchiveTitle.Text = Path.GetFileName(path);
        ArchiveSummary.Text = $"{result.Items.Count:N0}개 항목 · 해제 크기 {ArchiveItem.FormatSize(result.Items.Sum(x => x.Size))}";
        EntriesGrid.ItemsSource = result.Items; EmptyState.Visibility = result.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ExtractButton.IsEnabled = true; StatusText.Text = "압축파일을 열었습니다. 모두 풀기로 새 폴더에 저장하세요.";
    }
    private async void OpenClick(object sender, RoutedEventArgs e)
    {
        if (busy) return;
        var dialog = new OpenFileDialog { Title = "압축파일 열기", Filter = "압축파일|*.zip;*.7z;*.rar;*.arj;*.alz;*.egg;*.cab;*.lzh;*.tar;*.gz;*.bz2;*.xz;*.001|모든 파일|*.*" };
        if (dialog.ShowDialog(this) == true) await OpenArchive(dialog.FileName);
    }
    private void CreateClick(object sender, RoutedEventArgs e) { if (!busy) NewArchive([]); }
    private async void NewArchive(string[] inputs)
    {
        if (busy) return;
        var dialog = new CreateWindow(inputs) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Request == null) return;
        var result = await Run(dialog.Request);
        if (result?.Output != null) await OpenArchive(result.Output);
    }
    private async void ExtractClick(object sender, RoutedEventArgs e)
    {
        if (busy || archivePath == null) return;
        var dialog = new OpenFolderDialog { Title = "압축을 풀 상위 폴더 선택 (새 하위 폴더를 만듭니다)", InitialDirectory = Path.GetDirectoryName(archivePath)! };
        if (dialog.ShowDialog(this) != true) return;
        var name = Path.GetFileNameWithoutExtension(archivePath);
        if (string.IsNullOrWhiteSpace(name)) name = "압축해제";
        var output = Path.Combine(dialog.FolderName, name); var suffix = 2;
        while (Directory.Exists(output) || File.Exists(output)) output = Path.Combine(dialog.FolderName, $"{name} ({suffix++})");
        await Run(new("extract", Archive: archivePath, Destination: output));
    }
    private void CancelClick(object sender, RoutedEventArgs e) { cancellation?.Cancel(); CancelButton.IsEnabled = false; StatusText.Text = "취소 요청을 처리하고 있습니다…"; }
    private void FolderClick(object sender, RoutedEventArgs e)
    {
        if (resultPath == null) return;
        Process.Start(new ProcessStartInfo(Directory.Exists(resultPath) ? resultPath : Path.GetDirectoryName(resultPath)!) { UseShellExecute = true });
    }
    private void DefaultsClick(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo("ms-settings:defaultapps") { UseShellExecute = true });
    private void AboutClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(this,
            "Fzip 0.1.1\n제작자: KorFresh\n\n오픈소스 라이선스\n" +
            "7-Zip 26.05 — GNU LGPL, BSD 및 unRAR 제한\nhttps://www.7-zip.org\n\n" +
            "EggDotNet 0.4.0 — MIT 및 포함 구성요소 라이선스\nhttps://github.com/akolman/EggDotNet\n\n" +
            ".NET — MIT 및 구성요소 고지\nLLVM / MinGW — 동봉 라이선스\n\n" +
            "라이선스 전문과 저작권 고지는 설치 폴더의 licenses에 있습니다.", "Fzip 정보");
    }
    private void OnDragOver(object sender, DragEventArgs e) { e.Effects = !busy && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; e.Handled = true; }
    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (busy || e.Data.GetData(DataFormats.FileDrop) is not string[] paths || paths.Length == 0) return;
        var extensions = new[] { ".zip", ".7z", ".rar", ".arj", ".alz", ".egg", ".cab", ".lzh", ".tar", ".gz", ".bz2", ".xz", ".001" };
        if (paths.Length == 1 && File.Exists(paths[0]) && extensions.Contains(Path.GetExtension(paths[0]).ToLowerInvariant())) await OpenArchive(paths[0]);
        else NewArchive(paths);
    }
}
