using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.Items;
using VoiceX.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

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
        public event MoveOnPage? moveOnHistory;
        public string? Version;
        public string? SmsNumber;
        public MainWindow()
        {
            InitializeComponent();
            localStoreService = new LocalStoreService();
            addDbContext = new AddDbContext();
            webService = new WebService();
            SmsNumber = "";
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
            Version = "1.1.3.6";
        }

        private async void LanguageChanged(object? sender, EventArgs e)
        {
            LanguagesFild.Visibility = Visibility.Hidden;
            await localStoreService.SaveDataAsync("lang", App.Language.Name);
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
            Debug.WriteLine(TrayIcon.IconSource.ToString());
            if (TrayIcon.IconSource.ToString().Contains("TrayIcon"))
            {
                this.ShowInBottomRight();
            }
            else
            {
                 moveOnHistory?.Invoke();
                this.ShowInBottomRight();
            }
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
                            if (String.IsNullOrEmpty(App.FirstLoginDate))
                            {
                                try
                                {
                                    string exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                    var installDate = File.GetCreationTime(exe);
                                    App.FirstLoginDate = installDate.Date.ToString("yyyy-MM-dd");
                                }
                                catch
                                {
                                    App.FirstLoginDate = "No-info";
                                }
                            }
                            CoreService.useStunSetver = stun == "1" ? true : false;
                            CoreService.StunServer = App.AccountData.Data.Sip_Settings.Stun_server;
                            CoreService.Version = Version!;
                            var core = CoreService.Instance.Core;
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
            var language = await localStoreService.LoadDataAsync("lang");
            if (!String.IsNullOrEmpty(language))
            {
                App.Language = new CultureInfo(language);
            }
            App.LanguageChanged += LanguageChanged;

            CultureInfo currLang = App.Language;
            LanguagesList.Items.Clear();
            foreach (var lang in App.Languages)
            {
                LanguagesList.Items.Add(new LangItem(lang, lang.Equals(currLang)));
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ErrorPlate.Visibility = Visibility.Collapsed;
            PreAsk.Visibility = Visibility.Collapsed;
            LanguagesFild.Visibility = Visibility.Collapsed;
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ErrorMessage.Text))
            {
                Clipboard.SetText(ErrorMessage.Text);
            }
        }
        public void ShowSuccess()
        {
            SuccessPlate.Visibility = Visibility.Visible;
        }
        public void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorPlate.Visibility = Visibility.Visible;
            ShowInBottomRight();
        }
        public void ShowSmsBlock(string number)
        {
            SmsNumber = Regex.Replace(number, @"\D", "");
            if (SmsNumber.Length < 7)
            {
                OperatorSmsBlock.Visibility = Visibility.Visible;
            }
            else
            {
                SmsBlock.Visibility = Visibility.Visible;
            }
        }
        public void ShowLanguages()
        {
            LanguagesFild.Visibility= Visibility.Visible;
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
                await addDbContext.DropDatabaseAsync();
                var info = CoreService.Instance.getInfo();

                if (info.onlineStatus)
                {
                    CoreService.Instance.Logout();
                }
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

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var items = LanguagesList.Items;
            foreach (var item in items) 
            {
                var languge = (LangItem)item;
                if (languge.Language.IsChecked == true)
                {
                    if (languge.cultureInfo != null)
                    {
                        App.Language = languge.cultureInfo;
                    }
                }
            }
        }

        private void TextMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextMessage.Text == "Text")
            {
                TextMessage.Text = "";
                TextMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 92, 102, 189));
            }
        }

        private void TextMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextMessage.Text == "")
            {
                TextMessage.Text = "Text";
                TextMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            }
        }

        private async void SendOperatorMessage_Click(object sender, RoutedEventArgs e)
        {
            TextBorderOperator.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
            if (TextMessageOperator.Text == "Text" || String.IsNullOrEmpty(TextMessage.Text))
            {
                TextBorderOperator.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                return;
            }
            var userId = App.AccountData?.Data.User_Data.UserID;
            if (String.IsNullOrEmpty(userId))
            {
                ShowError("User ID not found");
                DefaultOperatorMessagePlate();
                return;
            }
            var res = await webService.SendOperatorSms(TextMessageOperator.Text, SmsNumber!, userId, App.UserPbx!, App.userToken!, App.fw!);
            if (res != null)
            {
                if (res.type?.ToLower() == "success")
                {
                    OperatorSmsBlock.Visibility = Visibility.Collapsed;
                    ShowSuccess();
                    DefaultOperatorMessagePlate();
                    return;
                }
                else
                {
                    if (!String.IsNullOrEmpty(res.message))
                        ShowError(res.message);
                    else
                        ShowError("Failed to send message. Responce is empty");
                }
            }
            else
            {
                ShowError("Failed to send message");
            }
            DefaultOperatorMessagePlate();
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            TextBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
            SmsType.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
            FromUserBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
            var selectItem = SmsType.SelectionBoxItem;
            string? smsType = "";
            if (selectItem != null) {
                smsType = selectItem.ToString();
                if (String.IsNullOrEmpty(TextMessage.Text) || smsType == "Select type")
                {
                    SmsType.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                    return;
                }
            }
            else
            {
                SmsType.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                return;
            }
            if (FromUser.Text == "From" || String.IsNullOrEmpty(FromUser.Text))
            {
                FromUserBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                return;
            }
            if (TextMessage.Text == "Text" || String.IsNullOrEmpty(TextMessage.Text))
            {
                TextBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                return;
            }
            if (String.IsNullOrEmpty(SmsNumber))
            {
                ShowError("Number not found");
                DefaultMessagePlate();
                return;
            }
            else
            {
                string pattern = @"^(?:972|0)?(\d+)$";
                string replacement = "972$1";
                SmsNumber = Regex.Replace(SmsNumber, pattern, replacement);
            }
            if (smsType == "SMS")
            {
                var res = await webService.SendSms(TextMessage.Text, FromUser.Text, SmsNumber!, App.UserPbx!, App.userToken!, App.fw!);
                if (res != null) {
                    if (res.type == "success")
                    {
                        SmsBlock.Visibility = Visibility.Collapsed;
                        ShowSuccess();
                        DefaultMessagePlate();
                        return;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(res.message))
                            ShowError(res.message);
                        else
                            ShowError("Failed to send message. Responce is empty");
                    }
                }
                else
                {
                    ShowError("Failed to send message");
                }

            }
            DefaultMessagePlate();
            return;
        }

        private void FromUser_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FromUser.Text == "From")
            {
                FromUser.Text = "";
                FromUser.Foreground = new SolidColorBrush(Color.FromArgb(255, 92, 102, 189));
            }
        }

        private void FromUser_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FromUser.Text == "")
            {
                FromUser.Text = "From";
                FromUser.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            }
        }

        private void ContinueSuccess_Click(object sender, RoutedEventArgs e)
        {
            SuccessPlate.Visibility = Visibility.Collapsed;
        }
        public void DefaultMessagePlate()
        {
            SmsBlock.Visibility = Visibility.Collapsed;
            TextMessage.Text = "Text";
            FromUser.Text = "From";
            TextMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            FromUser.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            SmsType.SelectedItem = null;
            TextBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
            SmsType.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
            FromUserBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
        }
        public void DefaultOperatorMessagePlate()
        {
            OperatorSmsBlock.Visibility = Visibility.Collapsed;
            TextMessageOperator.Text = "Text";
            TextMessageOperator.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            TextMessageOperator.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
        }
        private void CloseMessage_Click(object sender, RoutedEventArgs e)
        {
            DefaultMessagePlate();
        }

        private void TextMessageOperator_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextMessageOperator.Text == "Text")
            {
                TextMessageOperator.Text = "";
                TextMessageOperator.Foreground = new SolidColorBrush(Color.FromArgb(255, 92, 102, 189));
            }
        }

        private void TextMessageOperator_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextMessageOperator.Text == "")
            {
                TextMessageOperator.Text = "Text";
                TextMessageOperator.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            }
        }

        private void OperatorCloseMessage_Click(object sender, RoutedEventArgs e)
        {
            DefaultOperatorMessagePlate();
        }
    }
}
