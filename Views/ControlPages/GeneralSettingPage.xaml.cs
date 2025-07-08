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
            transportId = 0;
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
            PbXText.Text = PbXText.Text + App.UserPbx?.TrimStart('0');
            var type = await localStoreService.LoadDataAsync("transport");
            if (!String.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case "0":
                        Tls.IsChecked = true;
                        break;
                    case "1":
                        Tcp.IsChecked = true;
                        break;
                }
            }
            else
            {
                await localStoreService.SaveDataAsync("transport", "0");
                Tls.IsChecked = true;
            }
            if (App.AccountData?.Data.Device_type == "softphone")
            {
                SmartPhone.IsChecked = true;
                LopTop.IsChecked = false;

                CoreService.Instance.Login(App.AccountData!.Data.Sip_Settings?.Sip_username!, App.AccountData.Data.Sip_Settings!.Sip_server, App.AccountData.Data.Sip_Settings.Sip_proxy, App.AccountData.Data.Sip_Settings.Sip_secret, transportId, stun == "1", ice == "1", ip == "1");
                ProfilePage.onlineToken = true;
            }
            else
            {
                LopTop.IsChecked = true;
                SmartPhone.IsChecked = false;
            }
            Exit.Click += Exit_Click;
            Online();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            GlobalWindow.Visibility = Visibility.Hidden;
            isRunning = false;
            window.Exit_Click(sender, e );
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
                                        if (SmartPhone.IsChecked == true)
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
                                                    window.ShowError(info.regStatusText);
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

        private async void Tcp_Toggled(object sender, RoutedEventArgs e)
        {
            if ((bool)Tcp.IsChecked!)
            {
                window!.LoadIcone.Visibility = Visibility.Visible;
                Tls.IsChecked = false;
                ProfilePage.onlineToken = false;
                await localStoreService.SaveDataAsync("transport", "1");
                await CoreService.Instance.ChangeTransport(1);
                ProfilePage.onlineToken = true;
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else if (Tls.IsChecked == false)
            {
                Tcp.IsChecked = true;
            }
        }
        private async void Tls_Toggled(object sender, RoutedEventArgs e)
        {
            if ((bool)Tls.IsChecked!)
            {
                window!.LoadIcone.Visibility = Visibility.Visible;
                Tcp.IsChecked = false;
                ProfilePage.onlineToken = false;
                await localStoreService.SaveDataAsync("transport", "0");
                await CoreService.Instance.ChangeTransport(0);
                ProfilePage.onlineToken = true;
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else if (Tls.IsChecked == false)
            {
                Tcp.IsChecked = true; 
            }
        }
        
        private async void SmartPhone_Toggled(object sender, RoutedEventArgs e)
        {
            var swich = (ToggleButton)sender;
            if ((bool)swich.IsChecked!)
            {
                window!.LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("softphone", App.UserPbx!, App.userToken!, App.fw!) == System.Net.HttpStatusCode.OK)
                {
                    App.AccountData = await webService.GetAccountSettings(App.UserPbx!, App.userToken!, App.fw!);
                    CoreService.Instance.Login(App.AccountData!.Data.Sip_Settings?.Sip_username!, App.AccountData.Data.Sip_Settings!.Sip_server, App.AccountData.Data.Sip_Settings.Sip_proxy, App.AccountData.Data.Sip_Settings.Sip_secret, transportId, stun == "1", ice == "1", ip == "1");
                    ProfilePage.onlineToken = true;
                    LopTop.IsChecked = false;
                    App.AccountData!.Data.Device_type = "softphone";
                }
                else
                {
                    SmartPhone.IsChecked = false;
                    LopTop.IsChecked = true;
                    window!.LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                LopTop.IsChecked = true;
                LopTop_Toggled(LopTop, new RoutedEventArgs());
            }
        }

        private async void LopTop_Toggled(object sender, RoutedEventArgs e)
        {
            var info = CoreService.Instance.getInfo();
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
                    SmartPhone.IsChecked = false;
                }
                else
                {
                    SmartPhone.IsChecked = true;
                    LopTop.IsChecked = false;
                    window!.LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                window!.LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                SmartPhone.IsChecked = true;
                LopTop_Toggled(SmartPhone, new RoutedEventArgs());
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
