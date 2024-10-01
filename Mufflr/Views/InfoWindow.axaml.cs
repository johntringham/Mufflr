using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Reflection;

namespace Mufflr;

public partial class InfoWindow : Window
{
    public InfoWindow()
    {
        InitializeComponent();
        this.VersionDisplay.Text = AssemblyProductVersion;
    }

    private static string AssemblyProductVersion
    {
        get
        {
            return " v" + Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}