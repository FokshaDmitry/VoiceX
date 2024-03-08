using Linphone;
using System;
using System.Linq;
using VoiceX.DAL.Context;
using VoiceX.Services;
using Windows.Storage;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeneralSettingPage : Page
    {
        readonly Frame rootFrame = Window.Current.Content as Frame;
        private readonly AddDbContext addDbContext;
        readonly ApplicationDataContainer localSettings;
        readonly WebService webService;
        readonly BackgroundTaskService backgroundTask;
        readonly ErrorService errorService;
        public GeneralSettingPage()
        {
            this.InitializeComponent();
            AccountFild.Text = App.AccountData.Data.User_Data.Name;
            phoneNumber.Text = App.AccountData.Data.Sip_Settings.Sip_username;
            PbXText.Text = "PBX" + App.UserPbx.TrimStart('0');
            CoreService.Instance.AddOnAccountRegistrationStateChangedDelegate(OnAccountRegistrationStateChanged);
            this.SizeChanged += GeneralSettingPage_SizeChanged;
            this.Loaded += GeneralSettingPage_Loaded;
            webService = new WebService(App.userToken);
            backgroundTask = new BackgroundTaskService();
            addDbContext = new AddDbContext();
            localSettings = ApplicationData.Current.LocalSettings;
            errorService = new ErrorService(MainGrid);
        }

        private void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(CoreService.Instance.Core.DefaultAccount != null)
            {
                if (CoreService.Instance.Core.DefaultAccount.State == RegistrationState.Ok)
                {
                    Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 76, 176, 78));
                }
                else
                {
                    Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 77, 77));
                }
            }
            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("Trasnsport"))
            {
                var type = ApplicationData.Current.LocalSettings.Values["Trasnsport"].ToString();
                switch (type)
                {
                    case "Tcp":
                        Tcp.IsOn = true;
                        break;
                    case "Tls":
                        Tls.IsOn = true;
                        break;
                }
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values["Trasnsport"] = "Tcp";
                Tcp.IsOn = true;
            }
            if (App.AccountData.Data.Is_mobile == 0)
            {
                LopTop.IsOn = true;
            }
            else
            {
                SmartPhone.IsOn = true;
            }
            _ = App.AccountData.Data.Is_mobile == 0 ? LopTop.IsOn = true : SmartPhone.IsOn = true;
            LopTop.Toggled += LopTop_Toggled;
            SmartPhone.Toggled += SmartPhone_Toggled;
        }

        private void GeneralSettingPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
                
        }

        private  void Tcp_Toggled(object sender, RoutedEventArgs e)
        {
            if (Tcp.IsOn)
            {
                LoadIcone.Visibility = Visibility.Visible;
                Tls.IsOn = false;
                ReloadCore(TransportType.Tcp);
                ApplicationData.Current.LocalSettings.Values["Trasnsport"] = "Tcp";
                LoadIcone.Visibility = Visibility.Collapsed;
            }
            else if(Tls.IsOn == false)
            {
                Tcp.IsOn = true;
            }
        }
        private void Tls_Toggled(object sender, RoutedEventArgs e)
        {
            if (Tls.IsOn)
            {
                LoadIcone.Visibility = Visibility.Visible;
                Tcp.IsOn = false;
                ReloadCore(TransportType.Tls);
                ApplicationData.Current.LocalSettings.Values["Trasnsport"] = "Tls";
                LoadIcone.Visibility = Visibility.Collapsed;
            }
            else if(Tls.IsOn == false)
            {
                Tcp.IsOn = true;
            }
        }
        public void ReloadCore(TransportType type)
        {
            if (CoreService.Instance.Core.DefaultAccount != null)
            {
                if (CoreService.Instance.Core.DefaultAccount.Transport == type)
                {
                    return;
                }
                CoreService.Instance.LogOut();
            }
            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("NewAdress"))
            {
                if (ApplicationData.Current.LocalSettings.Values["NewAdress"].ToString() != "")
                {
                    App.Address = ApplicationData.Current.LocalSettings.Values["NewAdress"].ToString();
                    CoreService.Instance.LogIn(App.AccountData.Data.Sip_Settings.Sip_username.ToString(), App.AccountData.Data.Sip_Settings.Sip_secret, App.AccountData.Data.Sip_Settings.Sip_server, App.Address, type);
                }
                else
                {
                    CoreService.Instance.LogIn(App.AccountData.Data.Sip_Settings.Sip_username.ToString(), App.AccountData.Data.Sip_Settings.Sip_secret, App.AccountData.Data.Sip_Settings.Sip_server, App.Address, type);
                }
            }
            else
            {
                CoreService.Instance.LogIn(App.AccountData.Data.Sip_Settings.Sip_username.ToString(), App.AccountData.Data.Sip_Settings.Sip_secret, App.AccountData.Data.Sip_Settings.Sip_server, App.Address, type);
            }
        }
        private async void SmartPhone_Toggled(object sender, RoutedEventArgs e)
        {
            
            var swich = (ToggleSwitch)sender;
            if (swich.IsOn)
            {
                LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("mobile", App.UserPbx) == System.Net.HttpStatusCode.OK)
                {
                    LopTop.IsOn = false;
                }
                else
                {
                    SmartPhone.IsOn = false;
                    LopTop.IsOn = true;
                    LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                LopTop.IsOn = true;
            }
        }

        private async void LopTop_Toggled(object sender, RoutedEventArgs e)
        {
            var swich = (ToggleSwitch)sender;
            if (swich.IsOn)
            {
                LoadIcone.Visibility = Visibility.Visible;
                if (await webService.ChangeCallType("fix", App.UserPbx) == System.Net.HttpStatusCode.OK)
                {
                    SmartPhone.IsOn = false;
                }
                else
                {
                    SmartPhone.IsOn = true;
                    LopTop.IsOn = false;
                    LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
                LoadIcone.Visibility = Visibility.Collapsed;
            }
            else
            {
                SmartPhone.IsOn = true;
            }
        }

        
        public void OnAccountRegistrationStateChanged(Core core, Account account, RegistrationState state, string message)
        {
            if (state == RegistrationState.Ok)
            {
                Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 76, 176, 78));
            }
            else if (state == RegistrationState.Failed)
            {
                errorService.ShowError(message);
                Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 77, 77));
            }
            else
            {
                Activ.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 77, 77));
            }
        }

        private void Grid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                errorService.ShowWarningWithButton("You will not be able to undo this action!");
                errorService.Continue.Click += Continue_Click;
            }
            catch
            {

            }
        }

        private async void Continue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                localSettings.Values.Clear();
                CoreService.Instance.LogOut();
                await addDbContext.DropDatabaseAsync();
                ApplicationData.Current.LocalSettings.Values["AppState"] = "Close";
                backgroundTask.StopTask();
                rootFrame.Navigate(typeof(RegistrationPage), null, null);
                await webService.LogOut(App.UserPbx);
                while (App.appWindows.Count > 0)
                {
                    if (!App.appWindows.Select(a => a.Title).Contains(this.GetType().Name.Replace("Page", "")))
                    {
                        await App.appWindows.First().CloseAsync();
                    }
                    else
                    {
                        App.appWindows.Remove(App.appWindows.Where(a => a.Title.Contains(this.GetType().Name.Replace("Page", ""))).FirstOrDefault());
                    }
                }
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

        private void Exit_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ExitIcone.Margin = new Thickness(28, 2, 9, 4);
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void Exit_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ExitIcone.Margin = new Thickness(28, 2, 10, 4);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
    }
}
