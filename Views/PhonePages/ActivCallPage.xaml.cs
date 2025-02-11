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
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using VoiceX.Models;

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
        ProfilePage phonePage;
        LocalStoreService localStoreService;
        DialpadCallPage dialpadCallPage;
        bool pause;
        Storyboard slide;
        bool Microphone;
        bool Audio;
        public ActivCallPage(ProfilePage profilePage, DialpadCallPage dialpadCallPage)
        {
            this.InitializeComponent();
            timeFlag = true;
            startCall = new DateTime();
            pause = false;
            phonePage = profilePage;
            localStoreService = new LocalStoreService();
            this.dialpadCallPage = dialpadCallPage;
            slide = (Storyboard)profilePage.FindResource("SlideUpAnimation");
            Microphone = true;
            Audio = true;
        }

        private async void EnalebleCalling(object sender)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (CoreService.activeCall != null)
                    {

                        if (CoreService.activeCall.CallAdtess.Count != 0)
                        {
                            Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                            StatusCurrentCall.Text = ProfilePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                            PhoneText.Text = CoreService.activeCall.CallAdtess.Aggregate((current, next) => CutNumber(current) + ", " + CutNumber(next)).TrimEnd(' ', ',');
                            TransferCall.IsEnabled = false;
                            Pause.IsEnabled = false;
                        }
                        CallInfo info = new CallInfo();
                        try
                        {
                           info = CoreService.activeCall.getInfo();
                        }
                        catch
                        {

                        }
                        if (info != null)
                        {
                            if (timeFlag)
                            {
                                startCall = DateTime.Now;
                                timeFlag = false;
                                if (ProfilePage.AutoAnswerNumbers!.Contains(info.remoteContact))
                                {
                                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 80, 204));
                                    AutoAnswerImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icone_v2/refreshblue.png"));
                                }
                                else
                                {
                                    AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                                    AutoAnswerImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icone_v2/refresh.png"));
                                }
                            }
                            PhoneText.Text = CutNumber(info.remoteContact);
                            UserNameText.Text = CutNumber(info.remoteContact);
                        }
                        Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                        StatusCurrentCall.Text = ProfilePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        AddCall.IsEnabled = ProfilePage.StatusCall != Enums.StatusCall.Incoming;
                        CloseMicrophone.Visibility = Microphone ? Visibility.Collapsed : Visibility.Visible;
                        CloseSound.Visibility = Audio ? Visibility.Collapsed : Visibility.Visible;
                        TransferCall.IsEnabled = true;
                    }
                    else if (CoreService.activeCall == null)
                    {
                        Time.Text = "00:00";
                        timeFlag = true;
                        if (phonePage.MainFrame.Content.ToString() != "VoiceX.Views.PhonePages.DialpadCallPage")
                        {
                            phonePage.MainFrame.Navigate(dialpadCallPage);
                            slide.Begin();
                        }
                        if (timer != null)
                        {
                            timer.Dispose();
                            timer = null;
                        }
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
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            if(CoreService.activeCall != null)
            {
                if (CoreService.activeCall?.CallAdtess.Count != 0)
                {

                    ProfilePage.TerminateAllCalls = true;
                    CoreService.activeCall?.hangup(new CallOpParam());
                    CoreService.activeCall?.StopRingTone();
                    CoreService.activeCall?.DisableMicrophone();
                    CoreService.activeCall?.Dispose();
                    CoreService.activeCall = null;
                    phonePage.MainFrame.Navigate(dialpadCallPage);
                    slide.Begin();
                    try
                    {
                        if (timer != null)
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
                else
                {
                    
                    CoreService.activeCall?.hangup(new CallOpParam());
                    CoreService.activeCall?.StopRingTone();
                    CoreService.activeCall?.DisableMicrophone();
                    CoreService.activeCall?.Dispose();
                    CoreService.activeCall = null;
                    phonePage.MainFrame.Navigate(dialpadCallPage);
                    slide.Begin();

                }
            }
            else
            {
                phonePage.MainFrame.Navigate(dialpadCallPage);
                slide.Begin();
            }
        }
        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            if (!pause)
            {
                if (CoreService.activeCall != null)
                {
                    Audio = CoreService.activeCall.MuteSpeaker(!Audio);
                }
            }
        }

        private void Microphone_Click(object sender, RoutedEventArgs e)
        {
            if (!pause)
            {
                if (CoreService.activeCall != null)
                {
                    Microphone = CoreService.activeCall.MuteMicrophone(!Microphone);
                }
            }
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoreService.activeCall != null)
            {
                if (CoreService.activeCall.CallAdtess?.Count() != 0)
                {
                    return;
                }
                try
                {
                    if (pause)
                    {
                        pause = false;
                        CoreService.activeCall.StopRingTone();
                        CoreService.activeCall.MuteMicrophone(true);
                        CoreService.activeCall.MuteSpeaker(true);
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 221, 219, 255));
                    }
                    else
                    {
                        
                        pause = true; 
                        CoreService.activeCall.MuteMicrophone(false);
                        CoreService.activeCall.MuteSpeaker(false);
                        CoreService.activeCall.PlayHoldMusic();
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 240, 186, 105));
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
            if (CoreService.activeCall != null)
            {
                try
                {
                    var uri = CoreService.activeCall.getInfo().remoteContact;
                    if (!ProfilePage.AutoAnswerNumbers!.Contains(uri))
                    {
                        ProfilePage.AutoAnswerNumbers.Add(uri);
                        AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 80, 204));
                        AutoAnswerImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icone_v2/refreshblue.png"));
                        await UpdateAutoAncwerCallList();
                    }
                    else
                    {
                        ProfilePage.AutoAnswerNumbers.Remove(uri);
                        AutoAnswerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
                        AutoAnswerImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icone_v2/refresh.png"));
                        await UpdateAutoAncwerCallList();
                    }
                }
                catch (Exception ex) 
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private async Task UpdateAutoAncwerCallList()
        {
             await localStoreService.SaveDataAsync("AACallList", JsonConvert.SerializeObject(ProfilePage.AutoAnswerNumbers));
        }
    }
}
