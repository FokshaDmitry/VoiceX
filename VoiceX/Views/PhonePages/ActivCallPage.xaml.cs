using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoiceX.Services;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

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
        DialpadPage phonePage;
        bool pause;
        public ActivCallPage()
        {
            this.InitializeComponent();
            timeFlag = true;
            startCall = new DateTime();
            phonePage = new DialpadPage();
            pause = false;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e?.Parameter != null)
            {
                phonePage = (DialpadPage)e.Parameter;
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
                            if (DialpadPage.AutoAnswerNumbers.Contains(DialpadPage.currentCall.RemoteAddress.Username))
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
                        StatusCurrentCall.Text = DialpadPage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        AddCall.IsEnabled = DialpadPage.StatusCall != Enums.StatusCall.Incoming;
                        CloseMicrophone.Visibility = CoreService.Instance.Core.MicEnabled ? Visibility.Collapsed : Visibility.Visible; 
                        CloseSound.Visibility = DialpadPage.currentCall.SpeakerMuted ? Visibility.Visible : Visibility.Collapsed; 
                        TransferCall.IsEnabled = true;
                        PhoneText.Text = CoreService.Instance.Core.CurrentCall.RemoteAddress.Username;
                        UserNameText.Text = String.IsNullOrEmpty(CoreService.Instance.Core.CurrentCall.RemoteAddress.DisplayName) ? CoreService.Instance.Core.CurrentCall.RemoteAddress.Username : CoreService.Instance.Core.CurrentCall.RemoteAddress.DisplayName;
                    }
                    else if (DialpadPage.currentCall != null && String.IsNullOrEmpty(DialpadPage.currentCall.RemoteAddress.Username))
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
                    else if (DialpadPage.CallAdtess.Count != 0)
                    {
                        Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                        StatusCurrentCall.Text = DialpadPage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        PhoneText.Text = DialpadPage.CallAdtess.Aggregate((current, next) => current + ", " + next).TrimEnd(' ', ',');
                        TransferCall.IsEnabled = false;
                        Pause.IsEnabled = false;
                    }
                }
                catch
                {

                }
            });
        }
        private async void Profile_Click(object sender, RoutedEventArgs e)
        {
            await App.OpenWindow(typeof(ClientCardPages.ClientCard), PhoneText.Text + ";" + UserNameText.Text);
        }

        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            if (DialpadPage.CallAdtess.Count != 0)
            {
                
                DialpadPage.TerminateAllCalls = true;
                CoreService.Instance.Core.TerminateAllCalls();
                try
                {
                    if(timer != null)
                    {
                        timer.Dispose();
                        timer = null;
                    }
                }
                catch
                {
                    timer = null;
                }
                Frame.Navigate(typeof(DialpadCallPage), phonePage);
            }
            else if (DialpadPage.currentCall != null)
            {
                try
                {
                    DialpadPage.currentCall.Terminate();
                    if (CoreService.Instance.Core.Calls.Count() != 0 && CoreService.Instance.Core.Calls.Last() != null)
                    {
                        DialpadPage.currentCall = CoreService.Instance.Core.Calls.Last();
                        try
                        {
                            DialpadPage.currentCall.Resume();
                        }
                        catch { }
                    }
                    else
                    {
                        Frame.Navigate(typeof(DialpadCallPage), phonePage);
                    }
                }
                catch
                {
                    Frame.Navigate(typeof(DialpadCallPage), phonePage);
                }
                
            }
            else
            {
                Frame.Navigate(typeof(DialpadCallPage), phonePage);
            }
        }
        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            if (!pause)
            {
                if (DialpadPage.CallAdtess.Count() != 0)
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
                if (DialpadPage.CallAdtess.Count() != 0)
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
            if (DialpadPage.CallAdtess.Count() != 0)
            {
                return;
            }
            if (DialpadPage.currentCall != null)
            {
                try
                {
                    if (pause)
                    {
                        DialpadPage.currentCall.Resume();
                        pause = false;
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 221, 219, 255));
                        PauseRect.Stroke = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                        PauseRect1.Stroke = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                    }
                    else
                    {
                        DialpadPage.currentCall.Pause();
                        pause = true;
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 240, 186, 105));
                        PauseRect.Stroke = new SolidColorBrush(Color.FromArgb(255, 240, 186, 105));
                        PauseRect1.Stroke = new SolidColorBrush(Color.FromArgb(255, 240, 186, 105));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
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
            if (DialpadPage.currentCall != null && DialpadPage.currentCall.RemoteAddress != null)
            {
                var tmpPhone = DialpadPage.currentCall.RemoteAddress.Username;
                if (!DialpadPage.AutoAnswerNumbers.Contains(tmpPhone))
                {
                    DialpadPage.AutoAnswerNumbers.Add(tmpPhone);
                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 80, 204));
                    AutoAnswerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icone_v2/refreshblue.png"));
                    await Task.Run(() => UpdateAutoAncwerCallList());
                }
                else
                {
                    DialpadPage.AutoAnswerNumbers.Remove(tmpPhone);
                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                    AutoAnswerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icone_v2/refresh.png"));
                    await Task.Run(() => UpdateAutoAncwerCallList());
                }
            }
        }

        private void UpdateAutoAncwerCallList()
        {
            ApplicationData.Current.LocalSettings.Values["AACallList"] = JsonConvert.SerializeObject(DialpadPage.AutoAnswerNumbers);
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
