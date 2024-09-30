using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Volimit.ViewModels;

namespace Volimit
{
    public class ExitCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Debug.WriteLine("exiting....");
            CloseApp();
        }

        private void CloseApp()
        {
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainViewModel = desktop.MainWindow.DataContext as MainViewModel;
                mainViewModel.OnExit();

                desktop.Shutdown();
            }
        }
    }
}
