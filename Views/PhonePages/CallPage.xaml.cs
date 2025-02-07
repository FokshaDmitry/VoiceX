using pj;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using VoiceX.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CallPage : Page
    {
        private ProfilePage phonePage;
        DialpadCallPage phoneCallPage;
        ActivCallPage activCallPage;
        Storyboard slide;
        public CallPage(ProfilePage profilePage, DialpadCallPage dialpadCallPage, ActivCallPage activCallPage)
        {
            this.InitializeComponent();
            phonePage = profilePage;
            phoneCallPage = dialpadCallPage;
            this.activCallPage = activCallPage;
            slide = (Storyboard)profilePage.FindResource("SlideUpAnimation");
        }
        private void IncomeCallPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (CoreService.activeCall != null)
            {
                var info = CoreService.activeCall.getInfo();
                if (!String.IsNullOrEmpty(info.remoteUri))
                {
                    PhoneText.Text = CutNumber(info.remoteUri);
                    UserNameText.Text = CutNumber(info.remoteUri);
                }
                else
                {
                    PhoneText.Text = "No Information";
                    UserNameText.Text = "No Information";
                }
            }
            else
            {
                phonePage.MainFrame.Navigate(phoneCallPage);
                slide.Begin();
            }
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
        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CoreService.activeCall != null)
                {
                    CallOpParam rejectPrm = new CallOpParam();
                    rejectPrm.statusCode = pjsip_status_code.PJSIP_SC_BUSY_HERE;
                    CoreService.activeCall.hangup(rejectPrm);
                    phonePage.MainFrame.Navigate(phoneCallPage);
                    slide.Begin();
                    return;
                }
            }
            catch
            {
                phonePage.MainFrame.Navigate(phoneCallPage);
            }
        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (CoreService.activeCall != null) 
            {
                CoreService.activeCall.Accept();
                phonePage.MainFrame.Navigate(activCallPage);
                slide.Begin();
            }
            else
            {

            }
        }
    }
}
