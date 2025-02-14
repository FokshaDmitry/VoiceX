using System.Net;
using System.Windows;
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.Services;

namespace VoiceX.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        LocalStoreService localStoreService;
        RegistrationPage registrationPage;
        ProfilePage profilePage;
        readonly DispatcherTimer timer;
        readonly AddDbContext addDbContext;
        CertificateService certificateService;
        WebService webService;
        public MainWindow()
        {
            InitializeComponent();
            localStoreService = new LocalStoreService();
            registrationPage = new RegistrationPage(this);
            profilePage = new ProfilePage(this);
            addDbContext = new AddDbContext();
            webService = new WebService();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            certificateService = new CertificateService();
        }

        private void ShowWindow(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }
        private async void Timer_Tick(object sender, object e)
        {
            TimeSpan difference = DateTime.Now - App.timeOut;
            if (difference.TotalHours > 1 && !App.MyComputer)
            {
                timer.Stop();
                await addDbContext.DropDatabaseAsync();
                this.MainPage.Navigate(registrationPage);
            }
        }
        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            TrayIcon.Dispose();
            Application.Current.Shutdown();
        }

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; 
            this.Hide(); 
            this.ShowInTaskbar = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var token = await localStoreService.LoadDataAsync("token");
            var pbx = await localStoreService.LoadDataAsync("pbxCode");
            if (!String.IsNullOrEmpty(token))
            {
                if (!String.IsNullOrEmpty(pbx))
                {
                    if (certificateService.CheckCertificate("app-cert"))
                    {
                        App.AccountData = await webService.GetAccountSettings(pbx, token);
                        if(App.AccountData.ResponseCode == HttpStatusCode.OK)
                        {
                            App.userToken = token;
                            App.UserPbx = pbx;
                            this.MainPage.Content = profilePage;
                        }
                        else
                        {
                            this.MainPage.Content = registrationPage;
                        }
                    }
                    else
                    {
                        this.MainPage.Content = registrationPage;
                    }
                }
                else
                {
                    this.MainPage.Content = registrationPage;
                }
            }
            else
            {
                this.MainPage.Content = registrationPage;
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ErrorPlate.Visibility = Visibility.Collapsed;
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ErrorMessage.Text))
            {
                System.Windows.Clipboard.SetText(ErrorMessage.Text);
            }
        }
        public void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorPlate.Visibility = Visibility.Visible;
        }
        public async void Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                localStoreService.ClearIsolatedStorage();
                await addDbContext.DropDatabaseAsync();
                await webService.LogOut(App.UserPbx);
                this.MainPage.Content = registrationPage;
                //errorService.ShowWarningWithButton("You will not be able to undo this action!");
                //errorService.Continue.Click += Continue_Click;
            }
            catch
            {

            }
        }
        public void RestoreWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
            this.Activate();
        }
    }
}
