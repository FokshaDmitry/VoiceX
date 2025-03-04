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
        public AdditionPage()
        {
            this.InitializeComponent();
            this.Loaded += AdditionPage_Loaded;
        }
        private void AdditionPage_Loaded(object sender, RoutedEventArgs e)
        {
            Microphones.Items.Clear();
            Audio.Items.Clear();
            var audioManager = CoreService.Instance.Core.audDevManager();
            var deviceCount = audioManager.enumDev2(); 
            foreach (var device in deviceCount)
            {
                if (device != null)
                { 
                    if (device.inputCount > 0 && device.outputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Двустороннее устройство (гарнитура): {device.name}");
                        Microphones.Items.Add(new DeviceItem(device));
                        Audio.Items.Add(new DeviceItem(device));
                    }
                    else if (device.inputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Микрофон (ввод): {device.name}");
                        Microphones.Items.Add(new DeviceItem(device));
                    }
                    else if (device.outputCount > 0)
                    {
                        Debug.WriteLine($"[SIP] ID: Динамик (вывод): {device.name}");
                        Audio.Items.Add(new DeviceItem(device));
                    }
                    else
                    {
                        Debug.WriteLine($"[SIP] ID: Неизвестное устройство: {device.name}");
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

        private void Microphones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = (ComboBox)sender;
                var selectItem = (DeviceItem)item.SelectedItem;
                var manager = CoreService.Instance.Core.audDevManager();
                manager.setCaptureDev((int)selectItem.AudioDevInfo.caps);
            }
            catch
            {

            }
        }

        private void Audio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = (ComboBox)sender;
                var selectItem = (DeviceItem)item.SelectedItem;
                var manager = CoreService.Instance.Core.audDevManager();
                manager.setCaptureDev((int)selectItem.AudioDevInfo.caps);
            }
            catch
            {

            }
        }
    }
}
