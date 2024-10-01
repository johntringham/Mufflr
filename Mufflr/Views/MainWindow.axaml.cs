using Avalonia;
using Avalonia.Controls;
using System;
using DesktopNotifications;
using DesktopNotifications.Avalonia;


namespace Mufflr.Views;

public partial class MainWindow : Window
{
    private const string dontShowNotificationsActionId = "dontshow";
    private INotificationManager notificationManager;

    public DesktopNotifications.INotificationManager NotificationManager
    {
        get => notificationManager; 
        set
        {
            notificationManager = value;
            NotificationManager.NotificationActivated += NotificationButtonClicked;
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        //this.ShowInTaskbar = false;
        this.CanResize = false;

        this.Closing += (s, e) =>
        {
            if (e.CloseReason == Avalonia.Controls.WindowCloseReason.WindowClosing)
            {
                var settings = UserSettingsManager.GetSettings();
                if (settings.ShowCloseNotifications)
                {
                    var noti = new Notification()
                    {
                        Title = "Mufflr",
                        Body = "Mufflr minimised to tray - still running",
                    };

                    noti.Buttons.Add(("Don't show again", dontShowNotificationsActionId));

                    NotificationManager.ShowNotification(noti);
                }

                ((Window)s).Hide();
                e.Cancel = true;
            }
        };

        this.Loaded += this.OnLoaded;
    }

    private void NotificationButtonClicked(object? sender, NotificationActivatedEventArgs e)
    {
        var id = e.ActionId;
        if (id == dontShowNotificationsActionId)
        {
            var settings = UserSettingsManager.GetSettings();
            settings.ShowCloseNotifications = false;
            UserSettingsManager.SaveSettings(settings);
        }
    }

    private void OnLoaded(object? sender, EventArgs args)
    {
        PixelSize screenSize = Screens.Primary.WorkingArea.Size;
        PixelSize windowSize = PixelSize.FromSize(ClientSize, Screens.Primary.Scaling);

        var marginx = 5;
        var marginy = 35;

        Position = new PixelPoint(
          screenSize.Width - windowSize.Width - marginx,
          screenSize.Height - windowSize.Height - marginy);
    }
}
