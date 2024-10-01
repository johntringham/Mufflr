using Avalonia;
using Avalonia.Controls;
using System;

namespace Mufflr.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //this.ShowInTaskbar = false;
        this.CanResize = false;

        this.Closing += (s, e) =>
        {
            ((Window)s).Hide();
            e.Cancel = true;
        };

        //this.Deactivated += (s, e) =>
        //{
        //    ((Window)s).Hide();
        //};

        this.Loaded += this.OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs args)
    {
        PixelSize screenSize = Screens.Primary.WorkingArea.Size;
        PixelSize windowSize = PixelSize.FromSize(ClientSize, Screens.Primary.Scaling);

        var margin = 5;

        Position = new PixelPoint(
          screenSize.Width - windowSize.Width - margin,
          screenSize.Height - windowSize.Height - margin);
    }
}
