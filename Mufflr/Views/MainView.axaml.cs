using Avalonia.Controls;
using Avalonia.Interactivity;
using Mufflr.ViewModels;

namespace Mufflr.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
    }
}
