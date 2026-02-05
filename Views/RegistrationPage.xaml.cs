using pj;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VoiceX.Services;

namespace VoiceX.Views
{
    /// <summary>
    /// Interaction logic for RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        WebService webService;
        CertificateService certificateService;
        LocalStoreService localStoreService;
        MainWindow window;
        ProfilePage profilePage;
        public CoreService Core { get; } = CoreService.Instance;
        Endpoint core;
        public RegistrationPage(MainWindow mainWindow)
        {
            InitializeComponent();
            webService = new WebService();
            window = mainWindow;
            profilePage = new ProfilePage(mainWindow);
            App.AccountData = new Models.Account_data();
            certificateService = new CertificateService();
            localStoreService = new LocalStoreService();
        }
        private async void Button_Send(object sender, RoutedEventArgs e)
        {
            var pbxCode = RegistrationForm1.Text + RegistrationForm2.Text + RegistrationForm3.Text + RegistrationForm4.Text + RegistrationForm5.Text + RegistrationForm6.Text + RegistrationForm7.Text + RegistrationForm8.Text;
            await NetworkLogin(pbxCode);
        }
        public async Task NetworkLogin(string pbxCode)
        {
            pbxCode.Replace(" ", "");
            if (String.IsNullOrEmpty(pbxCode))
            {
                window.ShowError("Registration fild is empty");
                return;
            }
            if (Regex.IsMatch(pbxCode, "[^0-9]"))
            {
                window.ShowError("PBX code must contain only numbers");
                return;
            }
            if (pbxCode.Where(char.IsDigit).Count() != 8)
            {
                window.ShowError("PBX code must contain six numbers");
                return;
            }
            window.LoadIcone.Visibility = Visibility.Visible;
            try
            {
                // inport selfsign
                if (!certificateService.CheckCertificate("default-windowsrsa"))
                {
                    certificateService.SaveCertificate(AppDomain.CurrentDomain.BaseDirectory + "\\default-windowsrsa.p12", "r7Z33th35XTCfym6", "default-windowsrsa");
                }
                var cert = await webService.GetCertificateAsync(pbxCode, "windows");
                if (cert.ResponseCode == HttpStatusCode.OK)
                {
                    try
                    {
                        // if we have certificate 
                        //if we have certificate in certificate store and we have certificate hash
                        if (!certificateService.CheckCertificate("app-cert"))
                        {
                            certificateService.SaveCertificate(cert.P12l!, cert.Key!, "app-cert", "r7Z33th35XTCfym6");
                        }
                        App.userToken = cert.App_token;
                        await localStoreService.SaveDataAsync("pbxCode", pbxCode.Substring(0, 3));
                        await localStoreService.SaveDataAsync("token", cert.App_token!);
                        await localStoreService.SaveDataAsync("fw", pbxCode.Substring(3, 2));
                        await localStoreService.SaveDataAsync("ip", "1");
                        await localStoreService.SaveDataAsync("ice", "0");
                        App.fw = pbxCode.Substring(3, 2);
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
                        webService = new WebService();
                        App.AccountData = await webService.GetAccountSettings(pbxCode.Substring(0, 3), App.userToken!, App.fw!);
                        core = CoreService.Instance.Core;
                    }
                    catch (Exception ex)
                    {
                        window.ShowError($"App wrong: Convert, Message: {ex.Message}");
                        window.LoadIcone.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                else
                {
                    var error = cert.Error;
                    var errorCode = cert.ResponseCode.ToString();
                    if (errorCode == "10003" || errorCode == "0")
                    {
                        error = this.TryFindResource("m_WrongCode").ToString() ?? "Wrong PBX code";
                    }
                    window.ShowError(error);
                    window.LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                window.ShowError($"Server wrong: {ex.Message}, Message: {App.AccountData!.ResponseMessage}");
                window.LoadIcone.Visibility = Visibility.Collapsed;
                return;
            }
            if (App.AccountData.ResponseCode == HttpStatusCode.OK)
            {
                try
                {
                    App.UserPbx = $"{pbxCode.Substring(0, 3)}";
                    window.LoadIcone.Visibility = Visibility.Collapsed;
                    window.MainPage.Navigate(profilePage);
                }
                catch (Exception ex)
                {
                    window.ShowError($"App wrong: {ex.Message}, \n Message: {App.AccountData.ResponseMessage}");
                    window.LoadIcone.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                window.ShowError("Server wrong: " + App.AccountData.ResponseMessage + "Responce Code:" + App.AccountData.ResponseCode.ToString());
                window.LoadIcone.Visibility = Visibility.Collapsed;
            }
        }
        private async void Select_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var chek = (CheckBox)sender;
            if ((bool)chek.IsChecked!)
            {
                await localStoreService.SaveDataAsync("MyComputer", "On");
                App.MyComputer = true;
            }
            else
            {
                await localStoreService.SaveDataAsync("MyComputer", "Off");
                App.MyComputer = false;
            }
        }

        private void RegistrationForm1_TextChanged(object text, TextChangedEventArgs e)
        {
            var sender = (TextBox)text;
            if (!String.IsNullOrEmpty(sender.Text))
            {
                if (sender.Text.Length > 1 && sender.Text.All(char.IsDigit))
                {
                    sender.Text = sender.Text.Last().ToString();
                }
                if (!RegistrationForm1.Text.All(char.IsDigit))
                {
                    sender.Text = "";
                    return;
                }
                switch (sender.Name)
                {
                    case "RegistrationForm1":
                        RegistrationForm2.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm2":
                        RegistrationForm3.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm3":
                        RegistrationForm4.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm4":
                        RegistrationForm5.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm5":
                        RegistrationForm6.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm6":
                        RegistrationForm7.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm7":
                        RegistrationForm8.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm8":
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (sender.Name)
                {
                    case "RegistrationForm1":
                        break;
                    case "RegistrationForm2":
                        RegistrationForm1.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm3":
                        RegistrationForm2.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm4":
                        RegistrationForm3.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm5":
                        RegistrationForm4.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm6":
                        RegistrationForm5.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm7":
                        RegistrationForm6.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm8":
                        RegistrationForm7.Focus();
                        sender.Select(sender.Text.Length, 0);
                        break;
                    default:
                        break;
                }
            }
        }

        private void RegistrationForm1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                var key = (System.Windows.Controls.TextBox)sender;
                switch (key.Name)
                {
                    case "RegistrationForm1":
                        break;
                    case "RegistrationForm2":
                        RegistrationForm1.Focus();
                        break;
                    case "RegistrationForm3":
                        RegistrationForm2.Focus();
                        break;
                    case "RegistrationForm4":
                        RegistrationForm3.Focus();
                        break;
                    case "RegistrationForm5":
                        RegistrationForm4.Focus();
                        break;
                    case "RegistrationForm6":
                        RegistrationForm5.Focus();
                        break;
                    case "RegistrationForm7":
                        RegistrationForm6.Focus();
                        break;
                    case "RegistrationForm8":
                        RegistrationForm7.Focus();
                        break;
                    default:
                        break;
                }
            }
        }
        public static void SaveCertificate(string path, string password)
        {
            try
            {
                // Загружаем сертификат из файла
                X509Certificate2 cert = new X509Certificate2(path, password, X509KeyStorageFlags.PersistKeySet);

                // Открываем хранилище и добавляем сертификат
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    Debug.WriteLine("Сертификат успешно добавлен в хранилище.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении сертификата: {ex.Message}");
            }
        }
    }
}
