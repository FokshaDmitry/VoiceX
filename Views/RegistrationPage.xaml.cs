using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VoiceX.Services;
using Windows.Networking.NetworkOperators;
using Windows.Security.Cryptography.Certificates;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Net;

namespace VoiceX.Views
{
    /// <summary>
    /// Interaction logic for RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Window
    {
        WebService webService;
        StorageFile certificateFile;
        readonly ErrorService errorService;
        public RegistrationPage()
        {
            InitializeComponent();
            webService = new WebService("");
            App.AccountData = new Models.Account_data();
            LoadIcone.Visibility = Visibility.Collapsed;
            errorService = new ErrorService(MainGrid);
        }
        private async void Button_Send(object sender, RoutedEventArgs e)
        {
            var pbxCode = RegistrationForm1.Text + RegistrationForm2.Text + RegistrationForm3.Text + RegistrationForm4.Text + RegistrationForm5.Text + RegistrationForm6.Text;
            await NetworkLogin(pbxCode);
        }
        public async Task NetworkLogin(string pbxCode)
        {
            pbxCode.Replace(" ", "");
            if (String.IsNullOrEmpty(pbxCode))
            {
                errorService.ShowWarning("Registration fild is empty");
                return;
            }
            if (Regex.IsMatch(pbxCode, "[^0-9]"))
            {
                errorService.ShowWarning("PBX code must contain only numbers");
                return;
            }
            if (pbxCode.Where(char.IsDigit).Count() != 6)
            {
                errorService.ShowWarning("PBX code must contain six numbers");
                return;
            }
            LoadIcone.Visibility = Visibility.Visible;
            try
            {
                // first we need get default certificate in Assets folder
                certificateFile = await Package.Current.InstalledLocation.GetFileAsync(@"default-windowsrsa.p12");
                IBuffer buffer = await FileIO.ReadBufferAsync(certificateFile);
                string certData = CryptographicBuffer.EncodeToBase64String(buffer);
                // inport selfsign
                await CertificateEnrollmentManager.UserCertificateEnrollmentManager.ImportPfxDataAsync(certData, "r7Z33th35XTCfym6", ExportOption.NotExportable, KeyProtectionLevel.NoConsent, InstallOptions.None, "default-windowsrsa");
                var cert = await webService.GetCertificateAsync(pbxCode, "windows");
                if (String.IsNullOrEmpty(cert.Error))
                {
                    try
                    {
                        // if we have certificate 
                        //if we have certificate in certificate store and we have certificate hash
                        if (CertificateStores.FindAllAsync().GetAwaiter().GetResult().FirstOrDefault(c => c.FriendlyName == "app-cert") == null)
                        {
                            // Get cert
                            await CertificateEnrollmentManager.UserCertificateEnrollmentManager.ImportPfxDataAsync(cert.P12, "r7Z33th35XTCfym6", ExportOption.NotExportable, KeyProtectionLevel.NoConsent, InstallOptions.None, "app-cert");
                        }
                        App.userToken = cert.App_token;
                        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                        // Save user token
                        localSettings.Values["pbxCode"] = pbxCode.Substring(0, 3);
                        localSettings.Values["token"] = cert.App_token;
                        // Get user data
                        webService = new WebService(App.userToken);
                        App.AccountData = await webService.GetAccountSettings(pbxCode.Substring(0, 3));
                    }
                    catch (Exception ex)
                    {
                        errorService.ShowError($"App wrong: Convert, Message: {ex.Message}");
                        LoadIcone.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                else
                {
                    errorService.ShowError($"Token error: {cert.Error}");
                    LoadIcone.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                errorService.ShowError($"Server wrong: {ex.Message}, Message: {App.AccountData.ResponseMessage}");
                LoadIcone.Visibility = Visibility.Collapsed;
                return;
            }
            //App.accountData = await webService.RegistrationAccount(pbxCode, App.userToken);
            if (App.AccountData.ResponseCode == HttpStatusCode.OK)
            {
                try
                {
                    App.UserPbx = $"{pbxCode.Substring(0, 3)}";
                    ApplicationData.Current.LocalSettings.Values["AppState"] = "Open";
                }
                catch (Exception ex)
                {
                    errorService.ShowError($"App wrong: {ex.Message}, \n Message: {App.AccountData.ResponseMessage}");
                    LoadIcone.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                errorService.ShowError("Server wrong: " + App.AccountData.ResponseMessage + "Responce Code:" + App.AccountData.ResponseCode.ToString());
                LoadIcone.Visibility = Visibility.Collapsed;
            }
        }
        private void Select_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var chek = (CheckBox)sender;
            if ((bool)chek.IsChecked)
            {
                ApplicationData.Current.LocalSettings.Values["MyComputer"] = "On";
                App.MyComputer = true;
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values["MyComputer"] = "Off";
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
                    default:
                        break;
                }
            }
        }
    }
}
