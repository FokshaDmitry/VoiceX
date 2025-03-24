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
            this.Unloaded += CallPage_Unloaded;
        }

        private void CallPage_Unloaded(object sender, RoutedEventArgs e)
        {
            UserNameText.Text = "";
            PhoneText.Text = "";
        }

        private void IncomeCallPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (CoreService.activeCall != null)
            {
                var info = CoreService.activeCall.getInfo();
                if (!String.IsNullOrEmpty(info.remoteContact))
                {
                    if (String.IsNullOrEmpty(PhoneText.Text))
                    {
                        PhoneText.Text = phonePage.ExtractValue(info.remoteContact);
                    }
                    if (String.IsNullOrEmpty(UserNameText.Text))
                    {
                        var userName = phonePage.ExtractValue(info.remoteContact);
                        var contactName = ProfilePage.LDAPService?.SearchLdaps(App.AccountData?.Data.Ldap_Settings.Base!, userName).Where(l => l.Phone == userName).Select(l => l.Name).FirstOrDefault();
                        UserNameText.Text = String.IsNullOrEmpty(contactName) ? userName : contactName;
                    }
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
                phonePage.incomingWindow.Hide();
            }
            else
            {

            }
        }
    }
}
