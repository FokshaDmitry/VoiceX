using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VoiceX.DAL.Context;
using VoiceX.Services;
using VoiceX.Views.ControlPages;
using Windows.ApplicationModel;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegistrationPage : Page
    {
        readonly Frame rootFrame = Window.Current.Content as Frame;
        WebService webService;
        StorageFile certificateFile;
        readonly ErrorService errorService;
        public RegistrationPage()
        {
            this.InitializeComponent();
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
                    rootFrame.Navigate(typeof(ProfilePage), null, null);
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
        private void Select_Click(object sender, RoutedEventArgs e)
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
        private void RegistrationForm1_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
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
                        RegistrationForm2.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm2":
                        RegistrationForm3.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm3":
                        RegistrationForm4.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm4":
                        RegistrationForm5.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm5":
                        RegistrationForm6.Focus(FocusState.Programmatic);
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
                        RegistrationForm1.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm3":
                        RegistrationForm2.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm4":
                        RegistrationForm3.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm5":
                        RegistrationForm4.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    case "RegistrationForm6":
                        RegistrationForm5.Focus(FocusState.Programmatic);
                        sender.Select(sender.Text.Length, 0);
                        break;
                    default:
                        break;
                }
            }

        }

        private void RegistrationForm1_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.Back)
            {
                var key = (TextBox)sender;
                switch (key.Name)
                {
                    case "RegistrationForm1":
                        break;
                    case "RegistrationForm2":
                        RegistrationForm1.Focus(FocusState.Keyboard);
                        break;
                    case "RegistrationForm3":
                        RegistrationForm2.Focus(FocusState.Keyboard);
                        break;
                    case "RegistrationForm4":
                        RegistrationForm3.Focus(FocusState.Keyboard);
                        break;
                    case "RegistrationForm5":
                        RegistrationForm4.Focus(FocusState.Keyboard);
                        break;
                    case "RegistrationForm6":
                        RegistrationForm5.Focus(FocusState.Keyboard);
                        break;
                    default:
                        break;
                }
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
    }
}
