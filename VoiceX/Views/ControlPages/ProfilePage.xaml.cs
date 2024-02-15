using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using VoiceX.DAL.Context;
using System;
using VoiceX.Services;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.UI.Xaml.Media;
using Windows.UI.WindowManagement;
using VoiceX.Models;
using Windows.UI;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Foundation.Collections;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Core;
using VoiceX.Items;
using Windows.UI.Xaml.Media.Animation;
using System.Text.RegularExpressions;
using VoiceX.Views.PhonePages;
using Windows.Networking.PushNotifications;
using Microsoft.WindowsAzure.Messaging;
using System.Text;
using System.ServiceModel.Channels;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary
    public sealed partial class ProfilePage : Page
    {
        public static List<Regex_note> regexNotes;
        public static AppWindow appWindowCall;
        readonly WebService webService;
        readonly DispatcherTimer timer;
        readonly Frame rootFrame = Window.Current.Content as Frame;
        readonly AddDbContext addDbContext;
        readonly ApplicationDataContainer localSettings;
        public static Get_pauses getPauses;
        readonly ErrorService errorService;
        private readonly DialpadPage phonePage;

        //Data params
        readonly BackgroundTaskService backgroundTaskService;
        public ProfilePage()
        {
            this.InitializeComponent();

            webService = new WebService(App.userToken);
            //Core listener
            phonePage = new DialpadPage();
            CoreService.Instance.Core.Listener.OnCallStateChanged = phonePage.OnCallStateChanged;
            //Context
            addDbContext = new AddDbContext();
            //Active session
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            localSettings = ApplicationData.Current.LocalSettings;
            regexNotes = new List<Regex_note>();
            errorService = new ErrorService(ControlMainGrid);
            backgroundTaskService = new BackgroundTaskService();
            this.SizeChanged += ControlPage_SizeChanged;
        }

        private void ControlPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {


        }
        private async void ControlPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["AppState"] = "Open";
            //Include Descktop extension (Systray, ClickToCall, Virtual Printer)
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0) && App.FullTrustProcess)
            {
                App.AppServiceConnected += AppServiceConnected;
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                App.FullTrustProcess = false;
            }
            try
            {
                //User RegEx
                if (ApplicationData.Current.LocalSettings.Values["regexs"] != null)
                {
                    regexNotes = JsonConvert.DeserializeObject<List<Regex_note>>(ApplicationData.Current.LocalSettings.Values["regexs"].ToString());
                }
            }
            catch
            {
                regexNotes = new List<Regex_note>();
            }
            // Include Auto Answer List
            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("AACallList"))
            {
                if (DialpadPage.AutoAnswerNumbers != null)
                {
                    DialpadPage.AutoAnswerNumbers = JsonConvert.DeserializeObject<List<string>>(ApplicationData.Current.LocalSettings.Values["AACallList"].ToString());
                }
                else
                {
                    DialpadPage.AutoAnswerNumbers = new List<string>();
                    DialpadPage.AutoAnswerNumbers = JsonConvert.DeserializeObject<List<string>>(ApplicationData.Current.LocalSettings.Values["AACallList"].ToString());
                }
            }
            var result = await backgroundTaskService.StartAsync();
            if (!String.IsNullOrEmpty(result))
            {
                errorService.ShowError(result);
            }
            //general setting content
            ContentControl.Content = new GeneralSettingPage();
            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("MyComputer"))
            {
                if (!String.IsNullOrEmpty(ApplicationData.Current.LocalSettings.Values["MyComputer"].ToString()))
                {
                    //if my computer true app work only one hour if it is not used
                    App.MyComputer = ApplicationData.Current.LocalSettings.Values["MyComputer"].ToString() == "On";
                }
                else
                {
                    App.MyComputer = false;
                }
            }
            else
            {
                App.MyComputer = false;
            }
            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("MicrophoneDevice"))
            {
                foreach (var device in CoreService.Instance.Core?.ExtendedAudioDevices)
                {
                    if (device.Id == ApplicationData.Current.LocalSettings.Values["MicrophoneDevice"].ToString())
                    {
                        CoreService.Instance.Core.DefaultInputAudioDevice = device;
                    }
                }
            }
            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("AudioDevice"))
            {
                foreach (var device in CoreService.Instance.Core?.ExtendedAudioDevices)
                {
                    if (device.Id == ApplicationData.Current.LocalSettings.Values["AudioDevice"].ToString())
                    {
                        CoreService.Instance.Core.DefaultOutputAudioDevice = device;
                    }
                }
            }
            if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("Port"))
            {
                try
                {
                    App.Port = Convert.ToInt32(ApplicationData.Current.LocalSettings.Values["Port"]);
                }
                catch
                {
                    App.Port = 5060;
                }
            }
            else
            {
                App.Port = 5060;
            }
            if (!String.IsNullOrEmpty(App.AccountData.Data.Sip_Settings.Sip_username))
            {
                InitNotificationsAsync(App.AccountData.Data.Sip_Settings.Sip_username);
            }
        }

        private async void InitNotificationsAsync(string Token)
        {
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            try
            {
                var hub = new NotificationHub("VoiceXAppHub", "Endpoint=sb://VoiceXNotifications.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=dZxQhKhW8xDOjr44QFsRQKmciQuctGjpXhAxzZTgQJ0=");
                var result = await hub.RegisterNativeAsync(channel.Uri, new string[] { Token });
                if (String.IsNullOrEmpty(result.RegistrationId))
                {
                    errorService.ShowError("Registration hub filed");
                }
            }
            catch
            {
                errorService.ShowError("Registration hub filed");
            }
        }
        private void AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            //Request from Descktop extension
            e.AppServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
        }
        //HotKey, Fax, Systray receve
        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            AppServiceDeferral messageDeferral = args.GetDeferral();
            if (args != null && args.Request != null && args.Request.Message != null && args.Request.Message.Keys != null)
            {
                var select = args.Request.Message.FirstOrDefault().Key.ToString();
                switch (select)
                {
                    case "ID": // Click to call event
                        select = args.Request.Message.FirstOrDefault().Value.ToString();
                        if (!String.IsNullOrEmpty(select))
                        {
                            if (!Regex.IsMatch(select, "[^0-9]"))
                            {
                                try
                                {
                                    foreach (var regex in regexNotes.Where(r => r.Check))
                                    {
                                        select = select.Replace(regex.Search, regex.Replace);
                                    }
                                    var result = await webService.ClickToCall(select, App.AccountData.Data.User_Data.CompanyID, App.AccountData.Data.User_Data.UserID, App.UserPbx);
                                    if (!result.Contains("success"))
                                    {
                                        var builder = new ToastContentBuilder()
                                        .AddText("Error Call To VoiceX", hintMaxLines: 2)
                                        .AddText(select, hintMaxLines: 1)
                                        .AddText(result);
                                        builder.Show();
                                    }
                                }
                                catch
                                {
                                    break;
                                }
                            }
                            else
                            {
                                errorService.ShowWarning("Number contains invalid symbols");
                            }

                        }
                        else
                        {
                            errorService.ShowWarning("Number is empty");
                        }
                        break;
                    case "exit": // close app
                        CoreApplication.Exit();
                        break;
                    case "dialpad":
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback: () => _ = App.OpenWindow(typeof(DialpadPage), ""));
                        break;
                    case "clients":
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _ = App.OpenWindow(typeof(ClientsPage), ""));
                        break;
                    case "path": // sent file
                        if (!String.IsNullOrEmpty(args.Request.Message.FirstOrDefault().Value.ToString()))
                        {
                            byte[] mass;
                            try
                            {
                                mass = Convert.FromBase64String(args.Request.Message.FirstOrDefault().Value.ToString());
                            }
                            catch
                            {
                                break;
                            }
                            if (FaxPage.Files != null)
                            {
                                var date = DateTime.Now;
                                FaxPage.Files.Add($"File: {date.ToString("dd.MM") + "/" + date.ToString("T")};{mass.Length / 1024}", mass);
                            }
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _ = App.OpenWindow(typeof(FaxPage), ""));
                        }
                        break;
                    case "call":
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _ = App.OpenWindow(typeof(DialpadPage), ""));
                        break;
                    default:
                        break;

                }
                await args.Request.SendResponseAsync(new ValueSet());
                args.Request.Message.Clear();
                messageDeferral.Complete();
            }
            // we no longer need the connection
            try
            {
                App.AppServiceDeferral.Complete();
                App.Connection = null;
            }
            catch
            {
                App.Connection = null;
                return;
            }
        }
        //Timer out off app. Defolt one hour
        private async void Timer_Tick(object sender, object e)
        {
            TimeSpan difference = DateTime.Now - App.timeOut;
            if (difference.TotalHours > 1 && !App.MyComputer)
            {
                localSettings.Values.Clear();
                CoreService.Instance.LogOut();
                timer.Stop();
                await addDbContext.DropDatabaseAsync();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    rootFrame.Navigate(typeof(RegistrationPage), null, null);
                });
            }
        }
        #region Navigete Button
        private async void Navigate_Click(object sender, RoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Contacts":
                    await App.OpenWindow(typeof(ClientsPage), "");
                    break;
                case "Phone":
                    await App.OpenWindow(typeof(DialpadPage), "");
                    break;
                case "History":
                    await App.OpenWindow(typeof(HistoryPage), "");
                    break;
                case "Fax":
                    await App.OpenWindow(typeof(FaxPage), "");
                    break;
                case "HotKeys":
                    await App.OpenWindow(typeof(HotKeyPage), "");
                    break;
            }
        }
        #endregion
        //General Page Navigate
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 253, 254, 255));

            if (filter.Name == "General")
            {
                GeneralCheck.Background = blueLine;
                C2CCheck.Background = whiteLine;
                AdditionChek.Background = whiteLine;

                ContentControl.Navigate(typeof(GeneralSettingPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if (filter.Name == "C2C")
            {
                var NTrasform = ContentControl.Content.ToString() == "VoiceX.Views.ControlPages.GeneralSettingPage" ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
                GeneralCheck.Background = whiteLine;
                C2CCheck.Background = blueLine;
                AdditionChek.Background = whiteLine;
                ContentControl.Navigate(typeof(ClickToCallPage), "", new SlideNavigationTransitionInfo() { Effect = NTrasform });
            }
            else if (filter.Name == "Addition")
            {
                GeneralCheck.Background = whiteLine;
                C2CCheck.Background = whiteLine;
                AdditionChek.Background = blueLine;
                ContentControl.Navigate(typeof(AdditionPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
        private void Navigate_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top - 1, 0, 0);
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void Navigate_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top + 1, 0, 0);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Margin.Bottom == -50)
            {
                Menu.Margin = new Thickness(0, 0, 0, 0);
                Butter.Visibility = Visibility.Collapsed;
                Cross.Visibility = Visibility.Visible;
            }
            else
            {
                Menu.Margin = new Thickness(0, 0, 0, -50);
                Butter.Visibility = Visibility.Visible;
                Cross.Visibility = Visibility.Collapsed;
            }
        }
        private void ControlPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            PausesFild.Visibility = Visibility.Collapsed;
        }

        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            if (getPauses == null)
            {
                getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                getPauses.ResponseData.Pauses = new List<Pause>();
                getPauses = await webService.GetPauses(App.AccountData.Data.Sip_Settings.Sip_username, App.UserPbx);
                if (getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, getPauses.ResponseData.Pause_active == 0));
                    foreach (var pause in getPauses.ResponseData.Pauses)
                    {
                        PauseList.Items.Add(new PauseItem(pause, pause.Id == getPauses.ResponseData.Pause_active));
                    }
                }
                else
                {
                    errorService.ShowWarning(getPauses.ResponseMessage);
                }
            }
            else
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, getPauses.ResponseData.Pause_active == 0));
                foreach (var pause in getPauses.ResponseData.Pauses)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == getPauses.ResponseData.Pause_active));
                }
            }
            PausesFild.Visibility = Visibility.Visible;
        }

        private void PauseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            foreach (var item in list.Items)
            {
                var pause = (PauseItem)item;
                pause.SelectChange(pause == list.SelectedItem);
            }

        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pause = (PauseItem)PauseList.SelectedItem;
                if (pause != null)
                {
                    int id = pause.pause.Id;
                    if (getPauses.ResponseData.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData.Data.Sip_Settings.Sip_username, id, App.UserPbx);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            getPauses.ResponseData.Pause_active = id;
                        }
                        else
                        {
                            errorService.ShowWarning(result.ResponseMessage);
                            PauseList.SelectedIndex = -1;
                        }
                    }
                }

            }
            catch
            {
                PausesFild.Visibility = Visibility.Collapsed;
                return;
            }
            PausesFild.Visibility = Visibility.Collapsed;
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