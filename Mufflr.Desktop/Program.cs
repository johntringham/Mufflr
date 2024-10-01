using System;
using System.ComponentModel.DataAnnotations;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Mufflr.Views;
using DesktopNotifications;
using DesktopNotifications.Avalonia;

namespace Mufflr.Desktop;

class Program
{
    public static INotificationManager NotificationManager = null!;

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
            lifetime.Startup += (s, e) => DesktopStartup(lifetime);
        });

        var res = appBuilder.StartWithClassicDesktopLifetime(args);

        var lifetime = appBuilder.Instance.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime.MainWindow as MainWindow;

        ;
    }

    private static void DesktopStartup(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        var window = lifetime.MainWindow as MainWindow;
        window.NotificationManager = NotificationManager;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .SetupDesktopNotifications(out NotificationManager!)
            .WithInterFont()
            .LogToTrace();

}
