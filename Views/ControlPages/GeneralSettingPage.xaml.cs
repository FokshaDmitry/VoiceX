using pj;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using VoiceX.DAL.Context;
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
        public GeneralSettingPage(MainWindow window)
        {
            this.InitializeComponent();
            this.window = window;
            this.Loaded += GeneralSettingPage_Loaded;
            webService = new WebService();
            addDbContext = new AddDbContext();
            localStoreService = new LocalStoreService();
            timeOut = 1000;
        }

        private async void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            AccountFild.Text = App.AccountData?.Data.User_Data.Name;
            phoneNumber.Text = App.AccountData?.Data.Sip_Settings.Sip_username;
            PbXText.Text = "PBX" + App.UserPbx?.TrimStart('0');
            var type = await localStoreService.LoadDataAsync("Trasnsport");
            if (!String.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case "Tcp":
                        Tcp.IsChecked = true;
                        break;
                    case "Tls":
                        Tls.IsChecked = true;
                        break;
                }
            }
            else
            {
                await localStoreService.SaveDataAsync("Trasnsport", "Tcp");
                Tcp.IsChecked = true;
            }
            if (App.AccountData?.Data.Is_mobile == 0)
            {
                LopTop.IsChecked = true;
            }
            else
            {
                SmartPhone.IsChecked = true;
            }
            _ = App.AccountData?.Data.Is_mobile == 0 ? LopTop.IsChecked = true : SmartPhone.IsChecked = true;
            LopTop.Click += LopTop_Toggled;
            SmartPhone.Click += SmartPhone_Toggled;
            Exit.Click += Exit_Click;
            Online();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
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
                LoadIcone.Visibility = Visibility.Visible;
                Tls.IsChecked = false;
                await localStoreService.SaveDataAsync("Trasnsport", "Tcp");
                LoadIcone.Visibility = Visibility.Collapsed;
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
                LoadIcone.Visibility = Visibility.Visible;
                Tcp.IsChecked = false;
                await localStoreService.SaveDataAsync("Trasnsport", "Tls");
                LoadIcone.Visibility = Visibility.Collapsed;
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
                LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("mobile", App.UserPbx!, App.userToken!) == System.Net.HttpStatusCode.OK)
                {
                    CoreService.Instance.setRegistration(true);
                    LopTop.IsChecked = false;
                }
                else
                {
                    SmartPhone.IsChecked = false;
                    LopTop.IsChecked = true;
                    LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                LopTop.IsChecked = true;
                LopTop_Toggled(LopTop, new RoutedEventArgs());
            }
        }

        private async void LopTop_Toggled(object sender, RoutedEventArgs e)
        {
            var swich = (ToggleButton)sender;
            if ((bool)swich.IsChecked!)
            {
                LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("fix", App.UserPbx!, App.userToken!) == System.Net.HttpStatusCode.OK)
                {
                    CoreService.Instance.setRegistration(false);
                    await Task.Delay(1000);
                    SmartPhone.IsChecked = false;
                }
                else
                {
                    SmartPhone.IsChecked = true;
                    LopTop.IsChecked = false;
                    LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                SmartPhone.IsChecked = true;
                LopTop_Toggled(SmartPhone, new RoutedEventArgs());
            }
        }

        private void Grid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
    }
}
