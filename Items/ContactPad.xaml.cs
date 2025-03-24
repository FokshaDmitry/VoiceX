using System.Windows;
using System.Windows.Controls;
using VoiceX.Services;
using VoiceX.Views;
using VoiceX.Views.PhonePages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContactPad : ListBoxItem
    {
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public ContactPad(string UserName, string UserPhone, bool transfer)
        {
            this.InitializeComponent();
            this.UserName = UserName;
            this.UserPhone = UserPhone;
            this.userName.Text = UserName;
            this.userPhone.Text = UserPhone;
            if (CoreService.activeCall!.CallAdtess.Contains(UserPhone))
            {
                Select.IsChecked = true;
            }
            else
            {
                Select.IsChecked = false;
            }
            if (transfer)
            {
                Select.Visibility = Visibility.Visible;
                Transfer.Visibility = Visibility.Collapsed;
            }
            else
            {
                Select.Visibility = Visibility.Collapsed;
                Transfer.Visibility = Visibility.Visible;
            }
        }
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (Select.IsChecked!.Value)
            {
                if (ProfilePage.SelectContacts!.Contains(UserPhone))
                {
                    ProfilePage.SelectContacts.Remove(UserPhone);
                }
                else
                {
                    ProfilePage.SelectContacts.Add(UserPhone);
                }
            }
            else
            {
                if (ProfilePage.SelectContacts!.Contains(UserPhone))
                {
                    ProfilePage.SelectContacts.Remove(UserPhone);
                }
            }
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            if (CoreService.activeCall != null)
            {
                CoreService.AddParticipant(UserPhone, App.AccountData?.Data.Sip_Settings.Sip_server!);
            }
        }
    }
}
