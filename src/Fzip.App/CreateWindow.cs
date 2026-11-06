using Fzip.Core;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Fzip.App;
public sealed class CreateWindow : Window
{
    private readonly ListBox files = new() { Height = 150, Margin = new(0, 8, 0, 8) };
    private readonly TextBox output = new();
    private readonly ComboBox format = new() { ItemsSource = new[] { "zip", "7z" }, SelectedIndex = 0, MinWidth = 100, Padding = new(8) };
    private readonly ComboBox level = new() { ItemsSource = new[] { "빠르게", "균형 있게", "작게" }, SelectedIndex = 1, MinWidth = 130, Padding = new(8), Margin = new(10, 0, 0, 0) };
    public JobRequest? Request { get; private set; }
    public CreateWindow(string[] inputs)
    {
        Title = "새 압축 만들기 · Fzip"; Width = 620; Height = 570; ResizeMode = ResizeMode.NoResize; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = System.Windows.Media.Brushes.White; FontFamily = new("Malgun Gothic");
        var root = new StackPanel { Margin = new(26) }; Content = root;
        root.Children.Add(new TextBlock { Text = "파일을 하나로 모으세요", FontSize = 23, FontWeight = FontWeights.Bold });
        root.Children.Add(new TextBlock { Text = "같은 폴더의 파일과 폴더를 선택할 수 있습니다.", Margin = new(0, 8, 0, 8) });
        root.Children.Add(files);
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        AddButton(row, "파일 추가", () => { var d = new OpenFileDialog { Multiselect = true }; if (d.ShowDialog(this) == true) Add(d.FileNames); });
        AddButton(row, "폴더 추가", () => { var d = new OpenFolderDialog(); if (d.ShowDialog(this) == true) Add([d.FolderName]); });
        AddButton(row, "선택 제거", () => { if (files.SelectedItem != null) files.Items.Remove(files.SelectedItem); });
        root.Children.Add(row);
        root.Children.Add(new TextBlock { Text = "압축 형식 / 압축 수준", Margin = new(0, 18, 0, 6) });
        var options = new StackPanel { Orientation = Orientation.Horizontal }; options.Children.Add(format); options.Children.Add(level); root.Children.Add(options);
        format.SelectionChanged += (_, _) => { if (!string.IsNullOrWhiteSpace(output.Text)) output.Text = Path.ChangeExtension(output.Text, (string)format.SelectedItem); };
        root.Children.Add(new TextBlock { Text = "저장할 파일", Margin = new(0, 18, 0, 6) }); root.Children.Add(output);
        var footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new(0, 18, 0, 0) };
        AddButton(footer, "저장 위치", () => { var d = new SaveFileDialog { FileName = "새 압축." + format.SelectedItem, Filter = "압축파일|*." + format.SelectedItem, OverwritePrompt = true }; if (d.ShowDialog(this) == true) output.Text = d.FileName; });
        AddButton(footer, "압축 시작", () =>
        {
            try
            {
                if (files.Items.Count == 0) throw new ArgumentException("압축할 항목을 추가해주세요.");
                if (!Path.IsPathFullyQualified(output.Text)) throw new ArgumentException("저장할 전체 경로를 선택해주세요.");
                if (File.Exists(output.Text) || Directory.Exists(output.Text)) throw new ArgumentException("새 파일명을 지정해주세요. 기존 파일은 덮어쓰지 않습니다.");
                Request = new("compress", Destination: Path.ChangeExtension(output.Text, (string)format.SelectedItem), Inputs: files.Items.Cast<string>().ToArray(), Format: (string)format.SelectedItem, Level: new[] { 1, 5, 9 }[level.SelectedIndex]); DialogResult = true;
            }
            catch (Exception e) { MessageBox.Show(this, e.Message, "Fzip"); }
        });
        root.Children.Add(footer); Add(inputs);
    }
    private void Add(string[] paths)
    {
        foreach (var path in paths) if (!files.Items.Contains(path)) files.Items.Add(path);
        if (output.Text.Length == 0 && files.Items.Count != 0)
        {
            var first = (string)files.Items[0]; output.Text = Path.Combine(Path.GetDirectoryName(first)!, Path.GetFileNameWithoutExtension(first) + "." + format.SelectedItem);
        }
    }
    private static void AddButton(Panel p, string text, Action click) { var b = new Button { Content = text, Margin = new(0, 0, 8, 0) }; b.Click += (_, _) => click(); p.Children.Add(b); }
}
