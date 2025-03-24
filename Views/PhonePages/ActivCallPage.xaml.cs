using VoiceX.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using pj;
using System.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        Timer? timer;
        LocalStoreService localStoreService;
        ProfilePage phonePage;
        bool pause;
        bool Microphone;
        bool Audio;
        public ActivCallPage(ProfilePage profilePage)
        {
            this.InitializeComponent();
            timeFlag = true;
            startCall = new DateTime();
            pause = false;
            localStoreService = new LocalStoreService();
            Microphone = true;
            Audio = true;
            phonePage = profilePage;
        }

        private async void EnalebleCalling(object sender)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (CoreService.activeCall != null)
                    {
                        var boo = CoreService.Instance.Core.mediaActivePorts();
                        if (CoreService.activeCall.CallAdtess.Count() != 0)
                        {
                            var calls = CoreService.activeCall.CallAdtess;
                            Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                            StatusCurrentCall.Text = ProfilePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                            PhoneText.Text = CoreService.activeCall.CallAdtess.Aggregate((current, next) => phonePage.ExtractValue(current) + ", " + phonePage.ExtractValue(next)).TrimEnd(' ', ',');
                            TransferCall.IsEnabled = false;
                            Pause.IsEnabled = false;
                        }
                        else
                        {
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
                                if (String.IsNullOrEmpty(PhoneText.Text))
                                {
                                    PhoneText.Text = phonePage.ExtractValue(info.remoteUri);
                                }
                                if (String.IsNullOrEmpty(UserNameText.Text))
                                {
                                    var userName = phonePage.ExtractValue(info.remoteContact);
                                    var contactName = ProfilePage.LDAPService?.SearchLdaps(App.AccountData?.Data.Ldap_Settings.Base!, userName).Where(l => l.Phone == userName).Select(l => l.Name).FirstOrDefault();
                                    UserNameText.Text = String.IsNullOrEmpty(contactName) ? userName : contactName;
                                }
                            }
                        }
                        Time.Text = (DateTime.Now - startCall).ToString(@"mm\:ss");
                        StatusCurrentCall.Text = ProfilePage.StatusCall.ToString().ToUpper() + " " + "CALL";
                        //AddCall.IsEnabled = ProfilePage.StatusCall != Enums.StatusCall.Incoming;
                        CloseMicrophone.Visibility = Microphone ? Visibility.Collapsed : Visibility.Visible;
                        CloseSound.Visibility = Audio ? Visibility.Collapsed : Visibility.Visible;
                        TransferCall.IsEnabled = true;
                    }
                    else if (CoreService.activeCall == null)
                    {
                        Time.Text = "00:00";
                        timeFlag = true;
                        
                        if (timer != null)
                        {
                            timer.Dispose();
                            timer = null!;
                        }
                        phonePage.Navigate_Click(new Button() { Name = "Profile" }, new RoutedEventArgs());
                    }
                }
                catch
                {

                }
            });
        }
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            if(CoreService.activeCall != null)
            {
                CoreService.activeCall.EndAllCalls = true;
                CoreService.activeCall?.hangup(new CallOpParam());
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
                        CoreService.activeCall.SetHold(false);
                        Pause.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 221, 219, 255));
                    }
                    else
                    {
                        
                        pause = true;
                        CoreService.activeCall.SetHold(true);
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
                timer = new Timer(EnalebleCalling!, null, (int)TimeSpan.FromSeconds(1).TotalMilliseconds, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
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

        private void ActivPage_Unloaded(object sender, RoutedEventArgs e)
        {
            UserNameText.Text = "";
            PhoneText.Text = "";
        }
    }
}
