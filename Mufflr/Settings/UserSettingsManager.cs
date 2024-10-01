using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Mufflr
{
    internal class UserSettingsManager
    {
        private const string startupRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        const string autoStartupRegKeyName = "Mufflr desktop";

        private static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string settingsFolder = Path.Combine(appDataPath, "mufflr");
        private static string fileName = "settings.xml";
        private static string fullPath = Path.Combine(settingsFolder, fileName);

        public static Settings GetSettings()
        {
            var settings = TryReadFile();
            if (settings == null)
            {
                settings = new Settings();
            }

            return settings;
        }

        private static Settings? TryReadFile()
        {
            // todo: this method is ugly
            try
            {
                if (File.Exists(fullPath))
                {
                    var xml = File.ReadAllText(fullPath);
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    using (StringReader reader = new StringReader(xml))
                    {
                        var settings = (Settings?)serializer.Deserialize(reader);
                        return settings;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public static void SaveSettings(Settings settings)
        {
            if (!Directory.Exists(settingsFolder)) {
                Directory.CreateDirectory(settingsFolder);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            var writer = new StreamWriter(fullPath);
            serializer.Serialize(writer, settings);
            writer.Close();
        }

        internal static void SetAutoStartup(bool value)
        {
            var executingPath = Process.GetCurrentProcess().MainModule.FileName;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(startupRegistryKey, true);

            if (value)
            {
                rk.SetValue(autoStartupRegKeyName, executingPath);
            }
            else
            {
                rk.DeleteValue(autoStartupRegKeyName, false);
            }
        }

        internal static bool IsAutoStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(startupRegistryKey, true);
            var regValue = rk.GetValue(autoStartupRegKeyName, null);

            return (regValue != null);
        }
    }
}
