using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoiceX.Services;
using Windows.Storage;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using pj;
using Windows.UI.Core;
using System.Text.RegularExpressions;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActivCallPage : Grid
    {
        private DateTime startCall;
        bool timeFlag;
        Timer timer;
        ProfilePage phonePage;
        LocalStoreService localStoreService;
        DialpadCallPage dialpadCallPage;
        bool pause;
        public ActivCallPage(ProfilePage profilePage, DialpadCallPage dialpadCallPage)
        {
            this.InitializeComponent();
            timeFlag = true;
            startCall = new DateTime();
            pause = false;
            phonePage = profilePage;
            localStoreService = new LocalStoreService();
            this.dialpadCallPage = dialpadCallPage;
        }
        private async void EnalebleCalling(object sender)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (CoreService.activeCall != null)
                    {
                        var info = CoreService.activeCall.getInfo();
                        if (timeFlag)
                        {
                            startCall = DateTime.Now;
                            timeFlag = false;
                            if (ProfilePage.AutoAnswerNumbers.Contains(CutNumber(info.remoteContact)))
                            {
                                AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 80, 204));
                                AutoAnswerImage.Source = new BitmapImage(new Uri("/Assets/Icone_v2/refreshblue.png"));
                            }
                            else
                            {
                                AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                                AutoAnswerImage.Source = new BitmapImage(new Uri("/Assets/Icone_v2/refresh.png"));
                            }
                        }
                        Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                        StatusCurrentCall.Text = ProfilePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        AddCall.IsEnabled = ProfilePage.StatusCall != Enums.StatusCall.Incoming;
                        //CloseMicrophone.Visibility = CoreService.Instance.Core.MicEnabled ? Visibility.Collapsed : Visibility.Visible;
                        //CloseSound.Visibility = DialpadPage.currentCall.SpeakerMuted ? Visibility.Visible : Visibility.Collapsed;
                        TransferCall.IsEnabled = true;
                        PhoneText.Text = CutNumber(info.remoteContact);
                        UserNameText.Text = CutNumber(info.remoteContact);
                    }
                    else if (CoreService.activeCall == null)
                    {
                        Time.Text = "00:00";
                        timeFlag = true;
                        phonePage.ControlMainPage.Children.Clear();
                        phonePage.ControlMainPage.Children.Add(dialpadCallPage);
                        if (timer != null)
                        {
                            timer.Dispose();
                            timer = null;
                        }
                    }
                    if (ProfilePage.CallAdtess.Count != 0)
                    {
                        Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                        StatusCurrentCall.Text = ProfilePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        PhoneText.Text = ProfilePage.CallAdtess.Aggregate((current, next) => current + ", " + next).TrimEnd(' ', ',');
                        TransferCall.IsEnabled = false;
                        Pause.IsEnabled = false;
                    }
                }
                catch
                {

                }
            });
        }
        private string CutNumber(string sipUri)
        {
            string pattern = @"sip:(.*?)@";
            Match match = Regex.Match(sipUri, pattern);

            if (match.Success)
            {
                string extractedNumber = match.Groups[1].Value;
                return extractedNumber;
            }
            else
            {
                return "NOT FORMAT SIP URI";
            }
        }
        private async void Profile_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilePage.CallAdtess.Count != 0)
            {
                
                ProfilePage.TerminateAllCalls = true;
                if (CoreService.activeCall != null)
                {
                    CoreService.activeCall.hangup(new CallOpParam());
                    CoreService.activeCall.DisableMicrophone();
                    CoreService.activeCall.Dispose();
                    CoreService.activeCall = null;
                    phonePage.ControlMainPage.Children.Clear();
                    phonePage.ControlMainPage.Children.Add(dialpadCallPage);
                }
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
            }
            else if (CoreService.activeCall != null)
            {
                CoreService.activeCall.hangup(new CallOpParam());
                CoreService.activeCall.DisableMicrophone();
                CoreService.activeCall.Dispose();
                CoreService.activeCall = null;
                phonePage.ControlMainPage.Children.Clear();
                phonePage.ControlMainPage.Children.Add(dialpadCallPage);

            }
            else
            {
                phonePage.ControlMainPage.Children.Clear();
                phonePage.ControlMainPage.Children.Add(dialpadCallPage);
            }
        }
        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            if (!pause)
            {
                

            }
        }

        private void Microphone_Click(object sender, RoutedEventArgs e)
        {
            if (!pause)
            {

            }
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilePage.CallAdtess.Count() != 0)
            {
                return;
            }
            if (ProfilePage.currentCall != null)
            {
                try
                {
                    if (pause)
                    {
                        
                        pause = false;
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 221, 219, 255));
                        PauseRect.Stroke = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                        PauseRect1.Stroke = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                    }
                    else
                    {
                        
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
            if (ProfilePage.currentCall != null)
            {
                if (!ProfilePage.AutoAnswerNumbers.Contains("tmpPhone"))
                {
                    ProfilePage.AutoAnswerNumbers.Add("tmpPhone");
                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 80, 204));
                    AutoAnswerImage.Source = new BitmapImage(new Uri("/Assets/Icone_v2/refreshblue.png"));
                    await UpdateAutoAncwerCallList();
                }
                else
                {
                    ProfilePage.AutoAnswerNumbers.Remove("tmpPhone");
                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                    AutoAnswerImage.Source = new BitmapImage(new Uri("/Assets/Icone_v2/refresh.png"));
                    await UpdateAutoAncwerCallList();
                }
            }
        }

        private async Task UpdateAutoAncwerCallList()
        {
             await localStoreService.SaveDataAsync("AACallList", JsonConvert.SerializeObject(ProfilePage.AutoAnswerNumbers));
        }
    }
}
