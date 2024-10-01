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
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return " v" + fvi.FileVersion;
        }
    }
}