using Microsoft.VisualBasic.Devices;
using pj;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VoiceX.Items;
using VoiceX.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
            Microphones.Items.Clear();
            Audio.Items.Clear();
            var manager = CoreService.Instance.Core.audDevManager();
            manager.refreshDevs();
            var deviceCount = manager.enumDev2();
            int mic = 0;
            int aud = 0;
            if (!String.IsNullOrEmpty(micro))
            {
                int.TryParse(micro, out mic);
            }
            if (!String.IsNullOrEmpty(audio))
            {
                int.TryParse(audio, out aud);
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
                    }
                    index++;
                }
            }
            Microphones.SelectionChanged += Microphones_SelectionChanged;
            Audio.SelectionChanged += Audio_SelectionChanged;
        }
        private void Include_Toggled(object sender, RoutedEventArgs e)
        {
            
        }

        private void AdditionPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
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
            await CoreService.Instance.UseProxy(Proxy.IsChecked == true ? App.AccountData?.Data.Sip_Settings.Sip_proxy : App.AccountData?.Data.Sip_Settings.Sip_server);
            ProfilePage.onlineToken = true;
            await localStoreService.SaveDataAsync("proxy", Proxy.IsChecked == true ? "1" : "0");
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
            var flag = (bool)Ice.IsChecked!;
            ProfilePage.onlineToken = false;
            await CoreService.Instance.UseIpRewrite(flag);
            ProfilePage.onlineToken = true;
            await localStoreService.SaveDataAsync("ip", flag ? "1": "0");
        }
    }
}
