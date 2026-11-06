using System.Windows;

namespace Fzip.App;
public partial class App : Application
{
    public static string[] LaunchArgs { get; private set; } = [];
    protected override void OnStartup(StartupEventArgs e)
    {
        LaunchArgs = e.Args;
        DispatcherUnhandledException += (_, args) => { MessageBox.Show(args.Exception.Message, "Fzip", MessageBoxButton.OK, MessageBoxImage.Error); args.Handled = true; };
        base.OnStartup(e);
    }
}
