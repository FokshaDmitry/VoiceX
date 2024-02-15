using Linphone;
using System;
using System.Linq;
using VoiceX.Items;
using VoiceX.Services;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            Port.Text = App.Port.ToString();
            Microphones.Items.Clear();
            Audio.Items.Clear();
            CoreService.Instance.Core.ReloadSoundDevices();
            foreach (var device in CoreService.Instance.Core?.ExtendedAudioDevices)
            {
                if (device != null)
                {
                    if (device.Capabilities == AudioDeviceCapabilities.CapabilityRecord)
                    {
                        if (device.Id == CoreService.Instance.Core.DefaultInputAudioDevice.Id)
                        {
                            Microphones.PlaceholderText = device.DeviceName.Replace("?", "").TrimStart(' ');
                        }
                        Microphones.Items.Add(new DeviceItem(device));
                    }
                    if (device.Capabilities == AudioDeviceCapabilities.CapabilityPlay)
                    {
                        if (device.Id == CoreService.Instance.Core.DefaultOutputAudioDevice.Id)
                        {
                            Audio.PlaceholderText = device.DeviceName.Replace("?", "").TrimStart(' ');
                        }
                        Audio.Items.Add(new DeviceItem(device));
                    }
                }
            }

            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("NewAdress"))
            {
                if (ApplicationData.Current.LocalSettings.Values["NewAdress"].ToString() != "")
                {
                    Include.IsOn = true;
                    NewAdress.Text = ApplicationData.Current.LocalSettings.Values["NewAdress"].ToString();
                    NewAdress.IsEnabled = false;
                }
                else
                {
                    Include.IsOn = false;
                    NewAdress.Text = "";
                    NewAdress.IsEnabled = true;
                }
            }
        }
        private void Include_Toggled(object sender, RoutedEventArgs e)
        {
            if (Include.IsOn)
            {
                if (!String.IsNullOrEmpty(NewAdress.Text))
                {
                    NewAdress.IsEnabled = false;
                    ReloadCore(true);
                }
            }
            else
            {
                NewAdress.IsEnabled = true;
                ReloadCore(false);
            }
        }
        public void ReloadCore(bool flag)
        {
            var type = ApplicationData.Current.LocalSettings.Values["Trasnsport"].ToString();
            TransportType transportType = new TransportType();
            switch (type)
            {
                case "Tcp":
                    transportType = TransportType.Tcp;
                    break;
                case "Udp":
                    transportType = TransportType.Udp;
                    break;
                case "Tls":
                    transportType = TransportType.Tls;
                    break;
            }
            if (flag)
            {
                ApplicationData.Current.LocalSettings.Values["NewAdress"] = NewAdress.Text;
                CoreService.NatIgnore = false;
                CoreService.Instance.LogOut();
                CoreService.Instance.CoreStart(CoreApplication.GetCurrentView().CoreWindow.Dispatcher);
                CoreService.Instance.LogIn(App.AccountData.Data.Sip_Settings.Sip_username.ToString(), App.AccountData.Data.Sip_Settings.Sip_secret, App.AccountData.Data.Sip_Settings.Sip_server, NewAdress.Text, transportType, App.Port);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values["NewAdress"] = "";
                CoreService.NatIgnore = true;
                CoreService.Instance.LogOut(); 
                CoreService.Instance.CoreStart(CoreApplication.GetCurrentView().CoreWindow.Dispatcher);
                CoreService.Instance.LogIn(App.AccountData.Data.Sip_Settings.Sip_username.ToString(), App.AccountData.Data.Sip_Settings.Sip_secret, App.AccountData.Data.Sip_Settings.Sip_server, "sip:rsip.x-cloud.info", transportType, App.Port);
            }
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
                CoreService.Instance.Core.DefaultOutputAudioDevice = selectItem.AudioDevice;
                ApplicationData.Current.LocalSettings.Values["MicrophoneDevice"] = selectItem.AudioDevice.Id;
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
                CoreService.Instance.Core.DefaultOutputAudioDevice = selectItem.AudioDevice;
                ApplicationData.Current.LocalSettings.Values["AudioDevice"] = selectItem.AudioDevice.Id;
            }
            catch
            {

            }
        }
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void ChangePort_Click(object sender, RoutedEventArgs e)
        {
            if (Port.IsReadOnly)
            {
                Port.IsReadOnly = false;
                Pencil.Visibility = Visibility.Collapsed;
                Check.Visibility = Visibility.Visible;
                Port.Focus(FocusState.Keyboard);
            }
            else
            {
                if (!String.IsNullOrEmpty(Port.Text))
                {
                    Pencil.Visibility = Visibility.Visible;
                    Check.Visibility = Visibility.Collapsed;
                    string port = Port.Text;
                    if (port.All(char.IsDigit))
                    {
                        try
                        {
                            App.Port = Convert.ToInt32(port);
                        }
                        catch 
                        {
                            Port.IsReadOnly = true;
                            return;
                        }
                        ApplicationData.Current.LocalSettings.Values["Port"] = port;
                        ReloadCore(false);
                    }
                    else
                    {
                        Port.IsReadOnly = true;
                        return;
                    }
                }
                Port.IsReadOnly = true;
            }
        }
    }
}
