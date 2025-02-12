using pj;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.Services;
using Windows.UI.Notifications;

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
        public GeneralSettingPage(MainWindow window)
        {
            this.InitializeComponent();
            this.window = window;
            this.Loaded += GeneralSettingPage_Loaded;
            webService = new WebService();
            addDbContext = new AddDbContext();
            localStoreService = new LocalStoreService();
        }

        private async void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            AccountFild.Text = App.AccountData.Data.User_Data.Name;
            phoneNumber.Text = App.AccountData.Data.Sip_Settings.Sip_username;
            PbXText.Text = "PBX" + App.UserPbx.TrimStart('0');
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
            if (App.AccountData.Data.Is_mobile == 0)
            {
                LopTop.IsChecked = true;
            }
            else
            {
                SmartPhone.IsChecked = true;
            }
            _ = App.AccountData.Data.Is_mobile == 0 ? LopTop.IsChecked = true : SmartPhone.IsChecked = true;
            LopTop.Checked += LopTop_Toggled;
            SmartPhone.Checked += SmartPhone_Toggled;
            Exit.Click += window.Exit_Click;
            Online();
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
                            if (CoreService.Instance.getInfo().regIsActive)
                            {
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 76, 176, 78));
                                });
                            }
                            else
                            {
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 77, 77));
                                });
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
                    await Task.Delay(1000);
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
                if (await webService.ChangeCallType("mobile", App.UserPbx, App.userToken) == System.Net.HttpStatusCode.OK)
                {
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
            }
        }

        private async void LopTop_Toggled(object sender, RoutedEventArgs e)
        {
            var swich = (ToggleButton)sender;
            if ((bool)swich.IsChecked!)
            {
                LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("fix", App.UserPbx, App.userToken) == System.Net.HttpStatusCode.OK)
                {
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
            }
        }

        private void Grid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        

        private async void Continue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                localStoreService.ClearIsolatedStorage();
                await addDbContext.DropDatabaseAsync();
                await webService.LogOut(App.UserPbx);
            }
            catch
            {

            }
        }

        private void Exit_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ExitIcone.Margin = new Thickness(28, 2, 9, 4);
        }

        private void Exit_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ExitIcone.Margin = new Thickness(28, 2, 10, 4);
        }
    }
}
