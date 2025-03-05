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
        private void AdditionPage_Loaded(object sender, RoutedEventArgs e)
        {
            Microphones.Items.Clear();
            Audio.Items.Clear();
            var manager = CoreService.Instance.Core.audDevManager();
            manager.refreshDevs();
            var deviceCount = manager.enumDev2();

            
            AudioDevInfo idaud = new AudioDevInfo();
            AudioDevInfo idmic = new AudioDevInfo();
            try
            {
                idaud = manager.getDevInfo(manager.getPlaybackDev());
                idmic = manager.getDevInfo(manager.getCaptureDev());
            }
            catch
            {

            }
            foreach (var device in deviceCount)
            {
                if (device != null)
                {
                    if (device.inputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Микрофон (ввод): {device.name}");
                        
                        Microphones.Items.Add(new DeviceItem(device) { IsSelected = device.caps == idmic.caps });
                    }
                    if (device.outputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Динамик (вывод): {device.name}");
                        Audio.Items.Add(new DeviceItem(device) { IsSelected = device.caps == idaud.caps });
                    }
                }
            }
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
                    manager.setCaptureDev((int)selectItem.AudioDevInfo.caps);
                    await localStoreService.SaveDataAsync("micro", selectItem.AudioDevInfo.caps.ToString());
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
                    manager.setCaptureDev((int)selectItem.AudioDevInfo.caps);
                    await localStoreService.SaveDataAsync("audio", selectItem.AudioDevInfo.caps.ToString());
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
