using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VoiceX.Items;
using VoiceX.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ControlPages
{
    public enum StartupStatus
    {
        NotInStartup,
        Enabled,
        Disabled
    }
    public sealed partial class AdditionPage : Page
    {
        LocalStoreService localStoreService;
        public AdditionPage()
        {
            this.InitializeComponent(); 
            this.Loaded += AdditionPage_Loaded;
            localStoreService = new LocalStoreService();
        }
        private async void AdditionPage_Loaded(object sender, RoutedEventArgs e)
        {
            string micro = await localStoreService.LoadDataAsync("micro");
            string audio = await localStoreService.LoadDataAsync("audio");
            string ring = await localStoreService.LoadDataAsync("ring");
            Microphones.Items.Clear();
            Audio.Items.Clear();
            var manager = CoreService.Instance.Core.audDevManager();
            manager.refreshDevs();
            var deviceCount = manager.enumDev2();
            int mic = 0;
            int aud = 0;
            int rng = 0;
            if (!String.IsNullOrEmpty(micro))
            {
                int.TryParse(micro, out mic);
            }
            if (!String.IsNullOrEmpty(audio))
            {
                int.TryParse(audio, out aud);
            }
            if (!String.IsNullOrEmpty(ring))
            {
                int.TryParse(ring, out rng);
            }
            int index = 0;
            foreach (var device in deviceCount)
            {
                if (device != null)
                {
                    if (device.inputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Микрофон (ввод): {device.name}");
                        
                        Microphones.Items.Add(new DeviceItem(device.name, index) { IsSelected = index == mic });
                    }
                    if (device.outputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Динамик (вывод): {device.name}");
                        Audio.Items.Add(new DeviceItem(device.name, index) { IsSelected = index == aud });
                        Ringtone.Items.Add(new DeviceItem(device.name, index) { IsSelected = index == rng });
                    }
                    index++;
                }
            }
            var stun = await localStoreService.LoadDataAsync("stun");
            var ice = await localStoreService.LoadDataAsync("ice");
            var ip = await localStoreService.LoadDataAsync("ip");
            if (!String.IsNullOrEmpty(stun))
            {
                if (stun == "1")
                {
                    Proxy.IsChecked = true;
                }
            }
            if (!String.IsNullOrEmpty(ice))
            {
                if (ice == "1")
                {
                    Ice.IsChecked = true;
                }
            }
            if (!String.IsNullOrEmpty(ip))
            {
                if (ip == "1")
                {
                    Ip.IsChecked = true;
                }
            }
            Startup.IsChecked = IsInStartup() == StartupStatus.Enabled ? true : false;
            Microphones.SelectionChanged += Microphones_SelectionChanged;
            Audio.SelectionChanged += Audio_SelectionChanged;
            Ringtone.SelectionChanged += Ringtone_SelectionChanged;
        }
        private void Include_Toggled(object sender, RoutedEventArgs e)
        {
            
        }
        private async void Ringtone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = (ComboBox)sender;
                var selectItem = (DeviceItem)item.SelectedItem;
                if (selectItem != null)
                {
                    await localStoreService.SaveDataAsync("ring", selectItem.caps.ToString());
                }
            }
            catch
            {

            }
        }
        private async void Microphones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = (ComboBox)sender;
                var selectItem = (DeviceItem)item.SelectedItem;
                if (selectItem != null)
                {
                    var manager = CoreService.Instance.Core.audDevManager();
                    manager.setCaptureDev((int)selectItem.caps);
                    await localStoreService.SaveDataAsync("micro", selectItem.caps.ToString());
                }
            }
            catch
            {

            }
        }

        private async void Audio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = (ComboBox)sender;
                var selectItem = (DeviceItem)item.SelectedItem;
                if (selectItem != null)
                {
                    var manager = CoreService.Instance.Core.audDevManager();
                    manager.setPlaybackDev((int)selectItem.caps);
                    await localStoreService.SaveDataAsync("audio", selectItem.caps.ToString());
                }
            }
            catch
            {

            }
        }

        private async void Proxy_Checked(object sender, RoutedEventArgs e)
        {
            ProfilePage.onlineToken = false;
            //await CoreService.Instance.UseProxy(Proxy.IsChecked == true ? App.AccountData?.Data.Sip_Settings.Sip_proxy : App.AccountData?.Data.Sip_Settings.Sip_server);
            ProfilePage.onlineToken = true;
            await localStoreService.SaveDataAsync("stun", Proxy.IsChecked == true ? "1" : "0");
        }

        private async void Ice_Checked(object sender, RoutedEventArgs e)
        {
            bool flag = (bool)Ice.IsChecked!;
            ProfilePage.onlineToken = false;
            await CoreService.Instance.UseIceEnabled(flag);
            ProfilePage.onlineToken = true;
            await localStoreService.SaveDataAsync("ice", flag ? "1" : "0");
        }

        private async void Ip_Checked(object sender, RoutedEventArgs e)
        {
            var flag = (bool)Ip.IsChecked!;
            ProfilePage.onlineToken = false;
            await CoreService.Instance.UseIpRewrite(flag);
            ProfilePage.onlineToken = true;
            await localStoreService.SaveDataAsync("ip", flag ? "1": "0");
        }
        public StartupStatus IsInStartup()
        {
            RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            RegistryKey approvedKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", false);

            bool inRun = runKey?.GetValue("VoiceX") != null;
            byte[] statusBytes = approvedKey?.GetValue("VoiceX") as byte[];
            
            if (!inRun)
                return StartupStatus.Disabled;

            if (statusBytes != null && statusBytes.Length > 0)
            {
                byte status = statusBytes[0];

                return status switch
                {
                    0x02 => StartupStatus.Enabled,
                    0x03 => StartupStatus.NotInStartup,
                    _ => StartupStatus.NotInStartup // по умолчанию считаем включённым
                };
            }

            return StartupStatus.NotInStartup;
        }
        public void AddToStartupWithEnable()
        {
            // Устанавливаем флаг "Enabled" в StartupApproved
            using (RegistryKey approvedKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", true))
            {
                byte[] enabledValue = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 6 байт: статус + даты (можно нули)

                approvedKey.SetValue("VoiceX", enabledValue, RegistryValueKind.Binary);
            }
        }
        public static void AddToStartup()
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly();
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            rk.SetValue("VoiceX", $"\"{path.Location}\"");
        }
        public static void RemoveFromStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (rk.GetValue("VoiceX") != null)
            {
                rk.DeleteValue("VoiceX", false);
            }
        }
        private void Startup_Click(object sender, RoutedEventArgs e)
        {
            switch (IsInStartup())
            {
                case StartupStatus.NotInStartup:
                    AddToStartupWithEnable();
                    break;
                case StartupStatus.Enabled:
                    RemoveFromStartup();
                    break;
                case StartupStatus.Disabled:
                    AddToStartup();
                    break;
                default:
                    break;
            }
        }
    }
}
