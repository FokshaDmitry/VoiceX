using pj;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.Services;

namespace VoiceX.Views
{
    public partial class MainWindow : Window
    {
        
        LocalStoreService localStoreService;
        RegistrationPage? registrationPage;
        ProfilePage? profilePage;
        readonly DispatcherTimer timer;
        readonly AddDbContext addDbContext;
        CertificateService certificateService;
        WebService webService;
        public delegate void MoveOnPage();
        public event MoveOnPage? moveOnDialpad;
        public event MoveOnPage? moveOnContact;
        public CoreService Core { get; } = CoreService.Instance;
        Endpoint core;
        public MainWindow()
        {
            InitializeComponent();
            localStoreService = new LocalStoreService();
            addDbContext = new AddDbContext();
            webService = new WebService();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            if (!App.MyComputer)
            {
                timer.Tick += Timer_Tick!;
                timer.Start();
            }
            certificateService = new CertificateService();
        }

        private void ShowWindow(object sender, RoutedEventArgs e)
        {
            this.ShowInBottomRight();
        }
        private async void Timer_Tick(object sender, object e)
        {
            TimeSpan difference = DateTime.Now - App.timeOut;
            if (difference.TotalHours > 1)
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
            this.ShowInBottomRight();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; 
            this.Hide(); 
            this.ShowInTaskbar = false;
        }
        public void ShowInBottomRight()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            double offsetX = 10;
            double offsetY = 50; 

            this.Left = screenWidth - this.Width - offsetX;
            this.Top = screenHeight - this.Height - offsetY;

            this.Show();
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
            this.Activate();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            double offsetX = 10;
            double offsetY = 50; 
            this.Left = screenWidth - this.Width - offsetX;
            this.Top = screenHeight - this.Height - offsetY;
            var token = await localStoreService.LoadDataAsync("token");
            var pbx = await localStoreService.LoadDataAsync("pbxCode");
            var fw = await localStoreService.LoadDataAsync("fw");
            string stun = await localStoreService.LoadDataAsync("stun");
            if (!String.IsNullOrEmpty(token))
            {
                if (!String.IsNullOrEmpty(pbx))
                {
                    if (certificateService.CheckCertificate("app-cert"))
                    {
                        LoadIcone.Visibility = Visibility.Visible;
                        App.AccountData = new Models.Account_data();
                        App.AccountData = await webService.GetAccountSettings(pbx, token, fw);
                        Debug.WriteLine(App.AccountData.ResponseMessage);
                        LoadIcone.Visibility = Visibility.Collapsed;
                        if(App.AccountData.ResponseCode == HttpStatusCode.OK)
                        {
                            App.userToken = token;
                            App.UserPbx = pbx;
                            App.fw = fw;
                            CoreService.StunServer = App.AccountData.Data.Sip_Settings.Stun_server;
                            core = CoreService.Instance.Core;
                            profilePage = new ProfilePage(this);
                            this.MainPage.Content = profilePage;
                        }
                        else
                        {
                            ShowError($"Server error: {App.AccountData.ResponseCode.ToString()}\n Message: {App.AccountData.ResponseMessage}");
                            registrationPage = new RegistrationPage(this);
                            this.MainPage.Content = registrationPage;
                        }
                    }
                    else
                    {
                        registrationPage = new RegistrationPage(this);
                        this.MainPage.Content = registrationPage;
                        ShowError("App error: Certificate not found");
                    }
                }
                else
                {
                    registrationPage = new RegistrationPage(this);
                    this.MainPage.Content = registrationPage;
                }
            }
            else
            {
                registrationPage = new RegistrationPage(this);
                this.MainPage.Content = registrationPage;
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ErrorPlate.Visibility = Visibility.Collapsed;
            PreAsk.Visibility = Visibility.Collapsed;
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
            ShowInBottomRight();
        }
        
        public void Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PreAsk.Visibility = Visibility.Visible;
            }
            catch
            {

            }
        }

        private async void Continue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadIcone.Visibility = Visibility.Visible;
                PreAsk.Visibility = Visibility.Hidden;
                this.MainPage.Content = new RegistrationPage(this);
                localStoreService.ClearIsolatedStorage();
                await webService.LogOut(App.UserPbx!, App.userToken!, App.fw!);
                CoreService.Instance.Logout();
                await addDbContext.DropDatabaseAsync();
                LoadIcone.Visibility = Visibility.Hidden;
            }
            catch
            {

            }
        }
        public void RestoreWindow()
        {
            this.ShowInBottomRight();
        }

        private void Dialpad_Click(object sender, RoutedEventArgs e)
        {
            this.ShowInBottomRight();
            moveOnDialpad?.Invoke();
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            this.ShowInBottomRight();
            moveOnContact?.Invoke();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://x-cloud.info/",
                UseShellExecute = true
            });
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
    }
}
