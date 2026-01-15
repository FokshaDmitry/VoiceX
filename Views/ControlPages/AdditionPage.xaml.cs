using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VoiceX.Enums;
using VoiceX.Items;
using VoiceX.Services;
using Windows.ApplicationModel;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ControlPages
{
    
    public sealed partial class AdditionPage : Page
    {
        LocalStoreService localStoreService;
        string transport;
        public AdditionPage()
        {
            this.InitializeComponent(); 
            this.Loaded += AdditionPage_Loaded;
            localStoreService = new LocalStoreService();
            transport = "0";
        }
        private async void AdditionPage_Loaded(object sender, RoutedEventArgs e)
        {
            Version.Text = ProfilePage.window?.Version;
            string micro = await localStoreService.LoadDataAsync("micro");
            string audio = await localStoreService.LoadDataAsync("audio");
            string ring = await localStoreService.LoadDataAsync("ring");
            Microphones.Items.Clear();
            Audio.Items.Clear();
            Ringtone.Items.Clear();
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
            bool micFind = false;
            bool audioFind = false;
            foreach (var device in deviceCount)
            {
                if (device != null)
                {
                    if (device.inputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Микрофон (ввод): {device.name}");
                        
                        Microphones.Items.Add(new DeviceItem(device.name, index) { IsSelected = index == mic });
                        micFind = true;
                    }
                    if (device.outputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Динамик (вывод): {device.name}");
                        Audio.Items.Add(new DeviceItem(device.name, index) { IsSelected = index == aud });
                        Ringtone.Items.Add(new DeviceItem(device.name, index) { IsSelected = index == rng });
                        audioFind = true;
                    }
                    index++;
                }
            }
            if (!micFind)
            {
                ProfilePage.window?.ShowError("Microphone not found");
            }
            if (!audioFind)
            {
                ProfilePage.window?.ShowError("Audio not found");
            }
            var stun = await localStoreService.LoadDataAsync("stun");
            var ice = await localStoreService.LoadDataAsync("ice");
            var ip = await localStoreService.LoadDataAsync("ip");
            if (!String.IsNullOrEmpty(stun))
            {
                Proxy.IsChecked = stun == "1" ? true : false;
            }
            if (!String.IsNullOrEmpty(ice))
            {
                Ice.IsChecked = ice == "1" ? true : false;
            }
            if (!String.IsNullOrEmpty(ip))
            {
                Ip.IsChecked = ip == "1" ? true : false;
            }
            else
            {
                Ip.IsChecked = true;
            }
            Startup.IsChecked = await IsInStartup() == StartupStatus.Enabled ? true : false;
            Microphones.SelectionChanged += Microphones_SelectionChanged;
            Audio.SelectionChanged += Audio_SelectionChanged;
            Ringtone.SelectionChanged += Ringtone_SelectionChanged;
            transport = await localStoreService.LoadDataAsync("transport");
            switch (transport) 
            {
                case "0":
                    udp.IsChecked = true;
                    break;
                case "1":
                    tcp.IsChecked = true;
                    break;
                default:
                    udp.IsChecked = true;
                    break;
            }

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
            CoreService.useStunSetver = Proxy.IsChecked == true;
            CoreService.Instance.reloadCore();
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
        public async Task<StartupStatus> IsInStartup()
        {
            if (App.IsMSIX)
            {
                var startupTask = await StartupTask.GetAsync("VoiceXAppStartup");
                switch (startupTask.State)
                {
                    case StartupTaskState.Enabled:
                        return StartupStatus.Enabled;

                    case StartupTaskState.Disabled:
                        return StartupStatus.Disabled;
                }
                return StartupStatus.NotInStartup;
            }
            else
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
        }
        public async Task AddToStartup()
        {
            if (App.IsMSIX)
            {
                var startupTask = await StartupTask.GetAsync("VoiceXAppStartup");
                var newState = await startupTask.RequestEnableAsync();
                if (newState == StartupTaskState.Enabled)
                {

                }
                else
                {
                    Startup.IsChecked = false;
                }
            }
            else
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.SetValue("VoiceX", $"\"{exePath + "Voice.exe"}\"");
            }
        }
        public async Task RemoveFromStartup()
        {
            if (App.IsMSIX)
            {
                var startupTask = await StartupTask.GetAsync("VoiceXAppStartup");
                startupTask.Disable();
            }
            else
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (rk.GetValue("VoiceX") != null)
                {
                    rk.DeleteValue("VoiceX", false);
                }

            }
        }
        public async Task AddToStartupWithEnable()
        {
            if (App.IsMSIX)
            {
                await Launcher.LaunchUriAsync(new Uri("ms-settings:startupapps"));
            }
            else
            {
                using (RegistryKey approvedKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", true))
                {
                    byte[] enabledValue = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 6 байт: статус + даты (можно нули)

                    approvedKey.SetValue("VoiceX", enabledValue, RegistryValueKind.Binary);
                }
            }
        }
        private async void Startup_Click(object sender, RoutedEventArgs e)
        {
            var status = await IsInStartup();
            switch (status)
            {
                case StartupStatus.NotInStartup:
                    break;
                case StartupStatus.Enabled:
                    await RemoveFromStartup();
                    break;
                case StartupStatus.Disabled:
                    await AddToStartup();
                    break;
                default:
                    break;
            }
        }

        private async void tcp_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)tcp.IsChecked!)
            {
                ProfilePage.window!.LoadIcone.Visibility = Visibility.Visible;
                ProfilePage.onlineToken = false;
                await localStoreService.SaveDataAsync("transport", "1");
                await CoreService.Instance.ChangeTransport(0, App.AccountData?.Data.Sip_Settings.Sip_proxy!, App.AccountData!.Data.Sip_Settings?.Sip_username!, App.AccountData.Data.Sip_Settings!.Sip_server);
                ProfilePage.onlineToken = true;
                ProfilePage.window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            if ((bool)udp.IsChecked!)
            {
                ProfilePage.window!.LoadIcone.Visibility = Visibility.Visible;
                ProfilePage.onlineToken = false;
                await localStoreService.SaveDataAsync("transport", "0");
                await CoreService.Instance.ChangeTransport(1, App.AccountData?.Data.Sip_Settings.Sip_proxy!, App.AccountData!.Data.Sip_Settings?.Sip_username!, App.AccountData.Data.Sip_Settings!.Sip_server);
                ProfilePage.onlineToken = true;
                ProfilePage.window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
        }

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            ProfilePage.window?.Topmost = Pin.IsChecked == true ? true : false;
        }
    }
}
