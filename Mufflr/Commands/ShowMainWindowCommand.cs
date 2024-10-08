﻿using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mufflr
{
    internal class ShowMainWindowCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;

                mainWindow.Show();
                mainWindow.Activate();
                mainWindow.Focus();
            }
        }
    }
}
