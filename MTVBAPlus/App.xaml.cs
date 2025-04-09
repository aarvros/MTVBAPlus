using System.Windows;

namespace MTVBAPlus;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e){
        base.OnStartup(e);
        string[] args = e.Args;
        var mainWindow = new MainWindow(args);
        mainWindow.Show();
    }
}

