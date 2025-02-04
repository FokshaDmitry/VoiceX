using VoiceX.Views.PhonePages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            if (DialpadPage.CallAdtess.Contains(UserPhone))
            {
                Select.IsChecked = true;
            }
            else
            {
                Select.IsChecked = false;
            }
            if (transfer)
            {
                Select.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Transfer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                Select.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Transfer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
        private void Select_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Select.IsChecked.Value)
            {
                if (DialpadPage.SelectContacts.Contains(UserPhone))
                {
                    DialpadPage.SelectContacts.Remove(UserPhone);
                }
                else
                {
                    DialpadPage.SelectContacts.Add(UserPhone);
                }
            }
            else
            {
                if (DialpadPage.SelectContacts.Contains(UserPhone))
                {
                    DialpadPage.SelectContacts.Remove(UserPhone);
                }
            }
        }

        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void Transfer_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DialpadPage.currentCall?.Transfer(UserPhone);
        }
    }
}
