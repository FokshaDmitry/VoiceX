using Microsoft.Win32;

namespace VoiceX.Services
{
    public class StartupService
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "VoiceX.exe"; // Имя ключа в реестре

        public void EnableAutoStart()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true)!)
            {
                key.SetValue(AppName, $"\"{appPath}\"");
            }
        }

        public void DisableAutoStart()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true)!)
            {
                if (key.GetValue(AppName) != null)
                    key.DeleteValue(AppName);
            }
        }

        public bool IsAutoStartEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false)!)
            {
                return key.GetValue(AppName) != null;
            }
        }
    }
}
