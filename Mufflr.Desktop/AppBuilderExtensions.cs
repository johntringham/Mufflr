// NOTE: This file is meant to be included in the DesktopNotifications nuget package, but
// for some reason the nugetpackage is not up to date which meant this file wasn't working correctly
// so I've manually copied this in from the latest (as of writing) version on github.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using DesktopNotifications.FreeDesktop;
using DesktopNotifications.Windows;
using System;

namespace DesktopNotifications.Avalonia
{
    /// <summary>
    /// Extensions for <see cref="AppBuilder" />
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Setups the <see cref="INotificationManager" /> for the current platform and
        /// binds it to the service locator (<see cref="AvaloniaLocator" />).
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static AppBuilder SetupDesktopNotifications(this AppBuilder builder, out INotificationManager? manager)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var context = WindowsApplicationContext.FromCurrentProcess();
                manager = new WindowsNotificationManager(context);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var context = FreeDesktopApplicationContext.FromCurrentProcess();
                manager = new FreeDesktopNotificationManager(context);
            }
            else
            {
                //TODO: OSX once implemented/stable
                manager = null;
                return builder;
            }

            //TODO Any better way of doing this?
            manager.Initialize().GetAwaiter().GetResult();

            var manager_ = manager;
            builder.AfterSetup(b =>
            {
                if (b.Instance?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
                {
                    lifetime.Exit += (s, e) => { manager_.Dispose(); };
                }
            });

            return builder;
        }
    }
}