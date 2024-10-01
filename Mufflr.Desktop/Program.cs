using System;
using System.ComponentModel.DataAnnotations;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Mufflr.Views;


namespace Mufflr.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var appBuilder = BuildAvaloniaApp();

        appBuilder.AfterSetup((a) =>
        {
            var lifetime = a.Instance.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var window = lifetime.MainWindow as MainWindow;
        });

        var res = appBuilder.StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

}
