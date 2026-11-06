using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        var app = new Fzip.App.App();
        app.InitializeComponent();
        var window = new Fzip.App.MainWindow();
        window.ShowInTaskbar = false;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = -30000; window.Top = -30000;
        window.Show();
        window.ApplyTemplate();
        var content = (FrameworkElement)window.Content;
        content.UpdateLayout();
        var labels = new[] { "압축파일 열기", "새 압축 만들기", "기본 앱 설정", "프로그램 정보 · 라이선스" };
        foreach (var label in labels)
        {
            var text = Descendants(content).OfType<TextBlock>().Single(t => t.Text == label);
            var color = ((SolidColorBrush)text.Foreground).Color;
            if (color.R < 200 || color.G < 200 || color.B < 200)
                throw new InvalidOperationException($"Sidebar label is too dark: {label} / {color}");
            Console.WriteLine($"PASS sidebar foreground: {label} = {color}");
        }
        var bitmap = new RenderTargetBitmap((int)content.ActualWidth, (int)content.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        var background = new DrawingVisual();
        using (var drawing = background.RenderOpen()) drawing.DrawRectangle(window.Background, null, new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
        bitmap.Render(background);
        bitmap.Render(content);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(args[0]); png.Save(stream);
        window.Close(); app.Shutdown();
        return 0;
    }
    static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}
