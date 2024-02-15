using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoiceX.Services;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActivCallPage : Page
    {
        private DateTime startCall;
        bool timeFlag;
        Timer timer;
        PhonePage phonePage;
        bool pause;
        public ActivCallPage()
        {
            this.InitializeComponent();
            timeFlag = true;
            startCall = new DateTime();
            phonePage = new PhonePage();
            pause = false;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e?.Parameter != null)
            {
                phonePage = (PhonePage)e.Parameter;
                CoreService.Instance.Core.Listener.OnCallStateChanged = phonePage.OnCallStateChanged;
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }
        private async void EnalebleCalling(object sender)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    if (CoreService.Instance.Core.CurrentCall != null)
                    {
                        if (timeFlag)
                        {
                            startCall = DateTime.Now;
                            timeFlag = false;
                            if (PhonePage.AutoAnswerNumbers.Contains(PhonePage.currentCall.RemoteAddress.Username))
                            {
                                AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 80, 204));
                                AutoAnswerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icone_v2/refreshblue.png"));
                            }
                            else
                            {
                                AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                                AutoAnswerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icone_v2/refresh.png"));
                            }
                        }
                        Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                        StatusCurrentCall.Text = PhonePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        AddCall.IsEnabled = PhonePage.StatusCall != Enums.StatusCall.Incoming;
                        CloseMicrophone.Visibility = CoreService.Instance.Core.MicEnabled ? Visibility.Collapsed : Visibility.Visible; 
                        CloseSound.Visibility = PhonePage.currentCall.SpeakerMuted ? Visibility.Visible : Visibility.Collapsed; 
                        TransferCall.IsEnabled = true;
                        PhoneText.Text = CoreService.Instance.Core.CurrentCall.RemoteAddress.Username;
                        UserNameText.Text = String.IsNullOrEmpty(CoreService.Instance.Core.CurrentCall.RemoteAddress.DisplayName) ? CoreService.Instance.Core.CurrentCall.RemoteAddress.Username : CoreService.Instance.Core.CurrentCall.RemoteAddress.DisplayName;
                    }
                    else if (PhonePage.currentCall == null)
                    {
                        Time.Text = "00:00";
                        timeFlag = true;
                        Frame.Navigate(typeof(DialpadCallPage), phonePage);
                        if (timer != null)
                        {
                            timer.Dispose();
                            timer = null;
                        }
                    }
                    else if (PhonePage.CallAdtess.Count != 0)
                    {
                        Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                        StatusCurrentCall.Text = PhonePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        PhoneText.Text = PhonePage.CallAdtess.Aggregate((current, next) => current + ", " + next).TrimEnd(' ', ',');
                        TransferCall.IsEnabled = false;
                    }
                }
                catch
                {

                }
            });
        }
        private async void Profile_Click(object sender, RoutedEventArgs e)
        {
            await OpenWindow(typeof(ClientCardPages.ClientCard), PhoneText.Text + ";" + UserNameText.Text);
        }
        private async Task OpenWindow(Type Page, string Params)
        {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            Frame OpenPage1 = new Frame
            {
                Name = Page.Name
            };
            OpenPage1.Navigate(Page, Params);

            ElementCompositionPreview.SetAppWindowContent(appWindow, OpenPage1);
            appWindow.RequestMoveAdjacentToCurrentView();
            appWindow.TitleBar.BackgroundColor = Colors.WhiteSmoke;
            appWindow.TitleBar.InactiveBackgroundColor = Colors.WhiteSmoke;
            appWindow.TitleBar.ButtonHoverForegroundColor = Colors.DarkGray;
            appWindow.TitleBar.ButtonHoverBackgroundColor = Colors.WhiteSmoke;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.WhiteSmoke;
            appWindow.TitleBar.ButtonPressedBackgroundColor = Colors.WhiteSmoke;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.WhiteSmoke;
            WindowManagementPreview.SetPreferredMinSize(appWindow, App.Size);

            await appWindow.TryShowAsync();
            appWindow.Changed += App.AppWindow_Changed;
            App.AppWindows.Add(Page.Name);
        }

        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            if (PhonePage.CallAdtess.Count != 0)
            {
                PhonePage.TerminateAllCalls = true;
                CoreService.Instance.Core.TerminateAllCalls();
                timer.Dispose();
                timer = null;
                Frame.Navigate(typeof(DialpadCallPage), phonePage);
            }
            else
            {
                PhonePage.currentCall.Terminate();
                if (CoreService.Instance.Core.Calls.Count() != 0 && CoreService.Instance.Core.Calls.Last() != null)
                {
                    PhonePage.currentCall = CoreService.Instance.Core.Calls.Last();
                    try
                    {
                        PhonePage.currentCall.Resume();
                    }
                    catch { }
                }
                else
                {
                    Frame.Navigate(typeof(DialpadCallPage), phonePage);
                }
            }

        }
        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            if (!pause)
            {
                if (PhonePage.CallAdtess.Count() != 0)
                {
                    try
                    {
                        foreach (var call in CoreService.Instance.Core.Calls)
                        {
                            if (call.SpeakerMuted = !call.SpeakerMuted)
                            {
                                CloseSound.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                CloseSound.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    catch
                    {

                    }
                    return;
                }
                if (CoreService.Instance.ToggleSpeaker())
                {
                    CloseSound.Visibility = Visibility.Visible;
                }
                else
                {
                    CloseSound.Visibility = Visibility.Collapsed;
                }

            }
        }

        private void Microphone_Click(object sender, RoutedEventArgs e)
        {
            if (!pause)
            {
                if (PhonePage.CallAdtess.Count() != 0)
                {
                    try
                    {
                        foreach (var call in CoreService.Instance.Core.Calls)
                        {
                            if (call.MicrophoneMuted = !call.MicrophoneMuted)
                            {
                                CloseMicrophone.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                CloseMicrophone.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    catch
                    {

                    }
                    return;
                }
                if (CoreService.Instance.ToggleMic())
                {
                    CloseMicrophone.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CloseMicrophone.Visibility = Visibility.Visible;
                }
            }
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhonePage.CallAdtess.Count() != 0)
            {
                return;
            }
            if (PhonePage.currentCall != null)
            {
                try
                {
                    if (pause)
                    {
                        pause = false;
                        PhonePage.currentCall.Resume();
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 221, 219, 255));
                        PauseRect.Stroke = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                        PauseRect1.Stroke = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                    }
                    else
                    {
                        pause = true;
                        PhonePage.currentCall.Pause();
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 240, 186, 105));
                        PauseRect.Stroke = new SolidColorBrush(Color.FromArgb(255, 240, 186, 105));
                        PauseRect1.Stroke = new SolidColorBrush(Color.FromArgb(255, 240, 186, 105));
                    }
                }
                catch
                {
                    return;
                }
            }
        }
        private void TransferCall_Click(object sender, RoutedEventArgs e)
        {
            phonePage.AddContactsList(Enums.KeyPads.TransferPad);
        }
        private void AddCall_Click(object sender, RoutedEventArgs e)
        {
            
            phonePage.AddContactsList(Enums.KeyPads.AddCallPad);
        }

        private void KeyPadActiv_Click(object sender, RoutedEventArgs e)
        {
            phonePage.AddContactsList(Enums.KeyPads.DTMFPad);
        }
        private void ActivPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                timer = new Timer(EnalebleCalling, null, (int)TimeSpan.FromSeconds(1).TotalMilliseconds, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
            }
            
        }
        private async void AutoAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (PhonePage.currentCall != null && PhonePage.currentCall.RemoteAddress != null)
            {
                var tmpPhone = PhonePage.currentCall.RemoteAddress.Username;
                if (!PhonePage.AutoAnswerNumbers.Contains(tmpPhone))
                {
                    PhonePage.AutoAnswerNumbers.Add(tmpPhone);
                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 80, 204));
                    AutoAnswerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icone_v2/refreshblue.png"));
                    await Task.Run(() => UpdateAutoAncwerCallList());
                }
                else
                {
                    PhonePage.AutoAnswerNumbers.Remove(tmpPhone);
                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                    AutoAnswerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icone_v2/refresh.png"));
                    await Task.Run(() => UpdateAutoAncwerCallList());
                }
            }
        }

        private void UpdateAutoAncwerCallList()
        {
            ApplicationData.Current.LocalSettings.Values["AACallList"] = JsonConvert.SerializeObject(PhonePage.AutoAnswerNumbers);
        }
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
        private void PhonePage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
    }
}
