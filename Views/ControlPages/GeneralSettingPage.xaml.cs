using pj;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.Models;
using VoiceX.Services;

namespace VoiceX.Views.ControlPages
{
    /// <summary>
    /// Interaction logic for GeneralSettingPage.xaml
    /// </summary>
    public partial class GeneralSettingPage : Grid
    {
        private readonly AddDbContext addDbContext;
        readonly WebService webService;
        LocalStoreService localStoreService;
        MainWindow window;
        private bool isRunning = true;
        int timeOut;
        string ip;
        string ice;
        string stun;
        string transport;
        int transportId;
        public GeneralSettingPage(MainWindow window)
        {
            this.InitializeComponent();
            this.window = window;
            this.Loaded += GeneralSettingPage_Loaded;
            webService = new WebService();
            addDbContext = new AddDbContext();
            localStoreService = new LocalStoreService();
            timeOut = 1000;
            transportId = 1;
        }

        private async void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
        {

            ip = await localStoreService.LoadDataAsync("ip");
            ice = await localStoreService.LoadDataAsync("ice");
            stun = await localStoreService.LoadDataAsync("stun");
            transport = await localStoreService.LoadDataAsync("transport");
            int.TryParse(transport, out transportId);
            AccountFild.Text = App.AccountData?.Data.User_Data.Name;
            phoneNumber.Text = App.AccountData?.Data.Sip_Settings.Sip_username;
            PbXNumber.Text = App.UserPbx?.TrimStart('0');
            if (!String.IsNullOrEmpty(App.AccountData?.Data.Device_type))
            {
                switch (App.AccountData?.Data.Device_type)
                {
                    case "softphone":
                        if (ProfilePage.onlineToken == false)
                        {
                            AppPhone.IsChecked = true;

                            CoreService.Instance.Login(App.AccountData!.Data.Sip_Settings?.Sip_username!, App.AccountData.Data.Sip_Settings!.Sip_server, App.AccountData.Data.Sip_Settings.Sip_proxy, App.AccountData.Data.Sip_Settings.Sip_secret, transportId, stun == "0", ice == "0", ip == "1");
                            ProfilePage.onlineToken = true;
                        }
                        break;
                    case "webphone":
                        Webphone.IsChecked = true;
                        break;
                    case "mobile":
                        Softphone.IsChecked = true;
                        break;
                    case "fix":
                        Phone.IsChecked = true;
                        break;
                }
            }

            var manager = CoreService.Instance.Core.audDevManager();
            manager.refreshDevs();
            var deviceCount = manager.enumDev2();
            bool micFound = false;
            bool audFound = false;
            foreach (var device in deviceCount)
            {
                if (device != null)
                {
                    if (device.inputCount > 0)
                    {
                        micFound = true;
                    }
                    if (device.outputCount > 0)
                    {
                        audFound = true;
                    }
                }
            }
            if (!micFound)
            {
                window.ShowError("Microphone not found");
            }
            if (!audFound)
            {
                window.ShowError("Audio not found");
            }
            Exit.Click += Exit_Click;
            Online();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            GlobalWindow.Visibility = Visibility.Hidden;
            isRunning = false;
            window.Exit_Click(sender, e);
        }

        public void Online()
        {
            Task.Run(async () =>
            {
                while (isRunning)
                {
                    try
                    {
                        if (ProfilePage.onlineToken)
                        {
                            if (CoreService.Instance.getInfo() != null)
                            {
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    try
                                    {
                                        if (AppPhone.IsChecked == true)
                                        {
                                            var info = CoreService.Instance.getInfo();
                                            if (info.regIsActive)
                                            {
                                                Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 76, 176, 78));
                                                if (!info.onlineStatus)
                                                {

                                                    PresenceStatus presenceStatus = new PresenceStatus();
                                                    presenceStatus.status = pjsua_buddy_status.PJSUA_BUDDY_STATUS_ONLINE;
                                                    CoreService.Instance.setOnlineStatus(presenceStatus);
                                                }
                                                timeOut = 1000;
                                            }
                                            else
                                            {
                                                if (info.regStatus != pjsip_status_code.PJSIP_SC_OK && info.regStatus != pjsip_status_code.PJSIP_SC_TRYING)
                                                {
                                                    var errorMess = info.regStatusText;
                                                    if (errorMess == "Forbidden")
                                                    {
                                                        errorMess = this.TryFindResource("m_ForbiddenWrong").ToString() ?? errorMess;
                                                    }
                                                    if (errorMess == "Bad Gateway")
                                                    {
                                                        errorMess = this.TryFindResource("m_ConnectionWrong").ToString() ?? errorMess;
                                                    }
                                                    if (errorMess == "Request Timeout")
                                                    {
                                                        errorMess = this.TryFindResource("m_ServerWrong").ToString() ?? errorMess;
                                                    }
                                                    window.ShowError(errorMess);
                                                    window.Show();
                                                    window.WindowState = WindowState.Normal;
                                                    window.Activate();
                                                    CoreService.Instance.setRegistration(true);
                                                    if (CoreService.Instance.getInfo().regStatus != pjsip_status_code.PJSIP_SC_OK)
                                                    {
                                                        timeOut = 10000;
                                                    }
                                                }
                                                Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 77, 77));
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }
                                });
                            }
                            else
                            {
                                await Dispatcher.InvokeAsync(() => { Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 77, 77)); });
                            }
                        }
                        else
                        {
                            await Dispatcher.InvokeAsync(() => { Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 77, 77)); });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[UI] Ошибка при обновлении: {ex.Message}");
                    }
                    await Task.Delay(timeOut);

                }
            });
        }
        private async void Softphone_Click(object sender, RoutedEventArgs e)
        {
            var info = ProfilePage.onlineToken ? CoreService.Instance.getInfo() : null;
            var swich = (ToggleButton)sender;
            if ((bool)swich.IsChecked!)
            {
                window!.LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("mobile", App.UserPbx!, App.userToken!, App.fw!) == System.Net.HttpStatusCode.OK)
                {

                    if (info != null)
                    {
                        if (info.regIsActive)
                        {
                            CoreService.Instance.setRegistration(false);
                            info.onlineStatus = false;
                            ProfilePage.onlineToken = false;
                            CoreService.Instance.Logout();
                            await Task.Delay(1000);
                        }
                    }
                    App.AccountData!.Data.Device_type = "mobile";
                    Phone.IsChecked = false;
                    Webphone.IsChecked = false;
                    AppPhone.IsChecked = false;
                }
                else
                {
                    Softphone.IsChecked = false;
                    window!.LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                Softphone.IsChecked = true;
                //LopTop_Toggled(SmartPhone, new RoutedEventArgs());
            }
        }

        private async void Webphone_Click(object sender, RoutedEventArgs e)
        {
            var info = ProfilePage.onlineToken ? CoreService.Instance.getInfo() : null;
            var swich = (ToggleButton)sender;
            if ((bool)swich.IsChecked!)
            {
                window!.LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("webphone", App.UserPbx!, App.userToken!, App.fw!) == System.Net.HttpStatusCode.OK)
                {

                    if (info != null)
                    {
                        if (info.regIsActive)
                        {
                            CoreService.Instance.setRegistration(false);
                            info.onlineStatus = false;
                            ProfilePage.onlineToken = false;
                            CoreService.Instance.Logout();
                            await Task.Delay(1000);
                        }
                    }
                    App.AccountData!.Data.Device_type = "webphone";
                    Phone.IsChecked = false;
                    Softphone.IsChecked = false;
                    AppPhone.IsChecked = false;
                }
                else
                {
                    Webphone.IsChecked = false;
                    window!.LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                Webphone.IsChecked = true;
                //LopTop_Toggled(SmartPhone, new RoutedEventArgs());
            }
        }

        private async void App_Click(object sender, RoutedEventArgs e)
        {
            var swich = (ToggleButton)sender;
            if ((bool)swich.IsChecked!)
            {
                window!.LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("softphone", App.UserPbx!, App.userToken!, App.fw!) == System.Net.HttpStatusCode.OK)
                {

                    App.AccountData = await webService.GetAccountSettings(App.UserPbx!, App.userToken!, App.fw!);
                    CoreService.Instance.Login(App.AccountData!.Data.Sip_Settings?.Sip_username!, App.AccountData.Data.Sip_Settings!.Sip_server, App.AccountData.Data.Sip_Settings.Sip_proxy, App.AccountData.Data.Sip_Settings.Sip_secret, transportId, stun == "0", ice == "0", ip == "1");
                    ProfilePage.onlineToken = true;
                    App.AccountData!.Data.Device_type = "softphone";
                    Phone.IsChecked = false;
                    Softphone.IsChecked = false;
                    Webphone.IsChecked = false;
                }
                else
                {
                    AppPhone.IsChecked = false;
                    window!.LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                AppPhone.IsChecked = true;
                //LopTop_Toggled(SmartPhone, new RoutedEventArgs());
            }
        }

        private async void Phone_Click(object sender, RoutedEventArgs e)
        {
            var info = ProfilePage.onlineToken ? CoreService.Instance.getInfo() : null;
            var swich = (ToggleButton)sender;
            if ((bool)swich.IsChecked!)
            {
                window!.LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("fix", App.UserPbx!, App.userToken!, App.fw!) == System.Net.HttpStatusCode.OK)
                {

                    if (info != null)
                    {
                        if (info.regIsActive)
                        {
                            CoreService.Instance.setRegistration(false);
                            info.onlineStatus = false;
                            ProfilePage.onlineToken = false;
                            CoreService.Instance.Logout();
                            await Task.Delay(1000);
                        }
                    }
                    App.AccountData!.Data.Device_type = "fix";
                    AppPhone.IsChecked = false;
                    Softphone.IsChecked = false;
                    Webphone.IsChecked = false;
                }
                else
                {
                    Phone.IsChecked = false;
                    window!.LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                Phone.IsChecked = true;
                //LopTop_Toggled(SmartPhone, new RoutedEventArgs());
            }
        }
        private void Global_Click(object sender, RoutedEventArgs e)
        {
            GlobalWindow.Visibility = GlobalWindow.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            GlobalWindow.Visibility = Visibility.Hidden;
            window.LanguagesFild.Visibility = Visibility.Visible;
        }
    }
}
