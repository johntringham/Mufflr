using Avalonia.Controls;
using Avalonia.Interactivity;
using Volimit.ViewModels;

namespace Volimit.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        var viewModel = (MainViewModel)this.DataContext;

        viewModel.OnUnloaded();
    }
}
