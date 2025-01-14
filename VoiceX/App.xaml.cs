using System;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.AppService;
using Linphone;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.WindowManagement;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Generic;
using System.Net;
using VoiceX.Views.PhonePages;
using VoiceX.Views.ControlPages;
using System.IO;
using Windows.Security.Cryptography.Certificates;
using System.Linq;
using System.Diagnostics;
using Windows.UI.Xaml.Hosting;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.Core.Preview;
using System.Text;

namespace VoiceX
{
    sealed partial class App : Application
    {
        public CoreService Core { get; } = CoreService.Instance;
        public static Account_data AccountData { get; set; } // account datd activ user
        public static Size Size { get; set; }
        public static string UserPbx { get; set; }
        public static CoreCursor Hand { get; set; }
        public static CoreCursor Arrow { get; set; }
        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        public static event EventHandler<AppServiceTriggerDetails> AppServiceConnected;
        public static bool FullTrustProcess; // flag if descktop exetration open 
        public static List<AppWindow> appWindows { get; set; } // list antive windows
        public static DateTime timeOut { get; set; }
        public static bool MyComputer;
        public static string userToken;
        WebService webService;
        private bool firstWindow;
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            AccountData = new Account_data();
            Size = new Size(245, 420);
            appWindows = new List<AppWindow>();
            FullTrustProcess = true;
            timeOut = new DateTime();
            timeOut = DateTime.Now;
            MyComputer = true;
            Hand = new CoreCursor(CoreCursorType.Hand, 1);
            Arrow = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = (Frame)Window.Current.Content;
            CoreService.NatIgnore = ApplicationData.Current.LocalSettings.Values.Keys.Contains("NewAdress") && ApplicationData.Current.LocalSettings.Values["NewAdress"].ToString() != "";
            Core.CoreStart(CoreApplication.GetCurrentView().CoreWindow.Dispatcher);
            
            //Core.AddOnLog(OnLoggin);

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    _ = await LoginCore() ? rootFrame.Navigate(typeof(ProfilePage), e.Arguments) : rootFrame.Navigate(typeof(RegistrationPage), e.Arguments);
                    firstWindow = true;
                }
                SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += App_CloseRequested;
                // Ensure the current window is active
                Window.Current.Activate();
                firstWindow = true;
                SizeActivate();
            }
        }

        private void App_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            firstWindow = false;
        }

        //Event for first window
        public async static Task OpenWindow(Type Page, String Params)
        {
            timeOut = DateTime.Now;
            var Name = Page.Name.Replace("Page", "");
            if (appWindows.Select(s => s.Title).Contains(Name))
            {
                await appWindows.Where(s => s.Title.Contains(Name)).FirstOrDefault().TryShowAsync();
                return;
            }
            else
            {
                try
                {
                    AppWindow appWindow = await AppWindow.TryCreateAsync();
                    Frame OpenPage1 = new Frame
                    {
                        Name = Page.Name
                    };
                    OpenPage1.Navigate(Page, Params);
                    ElementCompositionPreview.SetAppWindowContent(appWindow, OpenPage1);
                    appWindow.RequestMoveAdjacentToCurrentView();
                    appWindow.Title = Name;
                    appWindow.TitleBar.BackgroundColor = Colors.WhiteSmoke;
                    appWindow.TitleBar.InactiveBackgroundColor = Colors.WhiteSmoke;
                    appWindow.TitleBar.ButtonHoverForegroundColor = Colors.DarkGray;
                    appWindow.TitleBar.ButtonHoverBackgroundColor = Colors.WhiteSmoke;
                    appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.WhiteSmoke;
                    appWindow.TitleBar.ButtonPressedBackgroundColor = Colors.WhiteSmoke;
                    appWindow.TitleBar.ButtonBackgroundColor = Colors.WhiteSmoke;
                    WindowManagementPreview.SetPreferredMinSize(appWindow, App.Size);
                    await appWindow.TryShowAsync();
                    appWindow.Closed += AppWindow_Closed;
                    if (Page.Name != "ClientCard")
                    {
                        appWindows.Add(appWindow);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.InnerException);
                    return;
                }
            }
        }

        private static void AppWindow_Closed(AppWindow sender, AppWindowClosedEventArgs args)
        {
            try
            {
                if (appWindows.Select(a => a.Title).Contains(sender.Title))
                {
                    var tmp = appWindows.First(a => a.Title == sender.Title);
                    appWindows.Remove(tmp);
                }
            }
            catch
            {

            }
        }
        // change rington
        public async Task MoveFile()
        {
            StorageFolder assetsFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets\\Ring");
            StorageFile file = await assetsFolder.GetFileAsync("ringtone.mkv");
            StorageFolder destinationFolder = await Package.Current.InstalledLocation.GetFolderAsync("share\\sounds\\linphone\\rings");

            await file.CopyAsync(destinationFolder, "ringtone.mkv", NameCollisionOption.ReplaceExisting);
        }
        // size and color window
        public static void SizeActivate()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(Size);
            ApplicationView.PreferredLaunchViewSize = Size;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().TitleBar.BackgroundColor = Colors.WhiteSmoke;
            ApplicationView.GetForCurrentView().TitleBar.InactiveBackgroundColor = Colors.WhiteSmoke;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Colors.DarkGray;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverBackgroundColor = Colors.WhiteSmoke;
            ApplicationView.GetForCurrentView().TitleBar.ButtonInactiveBackgroundColor = Colors.WhiteSmoke;
            ApplicationView.GetForCurrentView().TitleBar.ButtonPressedBackgroundColor = Colors.WhiteSmoke;
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.WhiteSmoke;
            ApplicationView.GetForCurrentView().Title = "Profile";
        }

        public async Task<bool> LoginCore()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.Keys.Contains("pbxCode") && localSettings.Values.Keys.Contains("token"))
            {
                UserPbx = localSettings.Values["pbxCode"].ToString();
                userToken = localSettings.Values["token"].ToString();
                if (!String.IsNullOrEmpty(UserPbx) && !String.IsNullOrEmpty(userToken))
                {
                    if (CertificateStores.FindAllAsync().GetAwaiter().GetResult().FirstOrDefault(c => c.FriendlyName == "app-cert") != null)
                    {
                        webService = new WebService(userToken);
                        AccountData = await webService.GetAccountSettings(UserPbx);
                        return AccountData.ResponseCode == HttpStatusCode.OK;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }

        }
        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            if (!appWindows.Select(a => a.Title).Contains("Profile"))
            {
                Frame rootFrame = (Frame)Window.Current.Content;
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
                if (rootFrame.Content == null)
                {
                    _ = await LoginCore() ? rootFrame.Navigate(typeof(ProfilePage)) : rootFrame.Navigate(typeof(RegistrationPage));

                }
                // Ensure the current window is active
                Window.Current.Activate();
                SizeActivate();
            }
            var file = (StorageFile)args.Files[0];
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                var prop = await file.GetBasicPropertiesAsync();
                var name = file.Name + ";" + Math.Round((decimal)prop.Size / 1000, 1).ToString();
                byte[] result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int)stream.Length);
                if (!FaxPage.Files.Keys.Contains(name))
                {
                    FaxPage.Files.Add(name, result);
                }
                else
                {
                    int n = 1;
                    while (FaxPage.Files.Keys.Contains($"({n})" + name))
                    {
                        n++;
                    }

                    FaxPage.Files.Add($"({n})" + name, result);
                }
            }
            if (!appWindows.Select(a => a.Title).Contains("Fax"))
            {
                await OpenWindow(typeof(FaxPage), "");
            }
        }
        // event, if app open for windows notification
        protected async override void OnActivated(IActivatedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["AppState"] = "Open";
            Frame rootFrame = (Frame)Window.Current.Content;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            if (Core.CheckCoreOnNull())
            {
                if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("NewAdress") && ApplicationData.Current.LocalSettings.Values["NewAdress"].ToString() != "")
                {
                    CoreService.NatIgnore = true;
                }
                else
                {
                    CoreService.NatIgnore = false;

                }
                Core.CoreStart(CoreApplication.GetCurrentView().CoreWindow.Dispatcher);

                _ = await LoginCore() ? rootFrame.Navigate(typeof(ProfilePage)) : rootFrame.Navigate(typeof(RegistrationPage));

                Window.Current.Activate();
                SizeActivate();
            }
            
            if (e is ProtocolActivatedEventArgs protocol)
            {
                string phone = "";
                if (protocol.Uri.AbsoluteUri.Contains("voicexapp"))
                {
                    phone = protocol.Uri.Host;
                }
                else
                {
                    phone = protocol.Uri.AbsolutePath;
                }

                if (!String.IsNullOrEmpty(phone))
                {
                    //Debug.WriteLine(Core.Core.DefaultAccount.ToString());
                    while (Core.Core.DefaultAccount == null)
                    {
                         await Task.Delay(1000);
                    }
                    if (DialpadPage.currentCall == null)
                    {
                        
                        if (!App.appWindows.Select(s => s.Title).Contains("Dialpad"))
                        {
                            await App.OpenWindow(typeof(DialpadPage), phone);
                        }
                        else
                        {
                            try
                            {
                                try
                                {
                                    await CoreService.Instance.OpenMicrophonePopup();
                                }
                                catch
                                {
                                    return;
                                }
                                foreach (var regex in ProfilePage.regexNotes.Where(r => r.Check))
                                {
                                    phone = phone.Replace(regex.Search, regex.Replace);
                                }
                                CoreService.Instance.Call(phone);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }

            if (e is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                ToastArguments args = ToastArguments.Parse(toastActivationArgs.Argument);
                if (args.ToString() == "action=AnswerCall")
                {
                    if (DialpadPage.currentCall != null)
                    {
                        if (DialpadPage.currentCall.State == CallState.IncomingReceived)
                        {
                            if (!appWindows.Select(a => a.Title).Contains("Dialpad"))
                            {
                                await OpenWindow(typeof(DialpadPage), "");
                                await ProfilePage.appWindowCall.CloseAsync();
                            }
                            else
                            {
                                DialpadPage.currentCall.Accept();
                            }
                        }
                    }
                }
                else if (args.ToString() == "action=IgnoreCall")
                {
                    try
                    {
                        if (DialpadPage.currentCall != null)
                        {
                            DialpadPage.currentCall.Decline(Reason.Declined);
                            await ProfilePage.appWindowCall.CloseAsync();
                        }
                        else
                        {
                            await ProfilePage.appWindowCall.CloseAsync();
                        }
                    }
                    catch
                    {

                    }

                }
            }
            Window.Current.Activate();
        }
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        private void OnResuming(object sender, object e)
        {
            ApplicationData.Current.LocalSettings.Values["AppState"] = "Open";
        }
        // event, if app closed or not active
        public void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            if (appWindows.Count <= 1 && !firstWindow)
            {
                ApplicationData.Current.LocalSettings.Values["AppState"] = "Close";
            }
            deferral.Complete();
        }
        // show and save linphone core log
        public void OnLoggin(LoggingService logService, string domain, LogLevel lev, string message)
        {
            StringBuilder builder = new StringBuilder();
            _ = builder.Append("Linphone-[").Append(lev.ToString()).Append("](").Append(domain).Append(")").Append(message);
            Debug.WriteLine(builder.ToString());
            //try
            //{
            //    //await Task.Run(() => dbContext.AddLogAsync(new DAL.Entity.LogginNotes { Id = Guid.NewGuid(), Domain = domain, Level = lev, Message = message }));
            //}
            //catch
            //{
            //    return;
            //}
        }
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
                {
                    AppServiceDeferral = args.TaskInstance.GetDeferral();
                    args.TaskInstance.Canceled += OnTaskCanceled;

                    Connection = details.AppServiceConnection;
                    AppServiceConnected?.Invoke(this, args.TaskInstance.TriggerDetails as AppServiceTriggerDetails);
                }
            }
        }
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            AppServiceDeferral?.Complete();
            AppServiceDeferral = null;
            Connection = null;
        }
    }
}