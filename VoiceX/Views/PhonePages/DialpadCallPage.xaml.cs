using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views.ControlPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DialpadCallPage : Page
    {

        contacts_list contacts;
        readonly WebService webService;
        private DialpadPage phonePage;

        public DialpadCallPage()
        {
            this.InitializeComponent();
            this.Loaded += DialpadCallPage_Loaded;
            webService = new WebService(App.userToken);
            contacts = new contacts_list();
            phonePage = new DialpadPage();
            contacts.contacts = new List<Models.Contact>();
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
        private async void DialpadCallPage_Loaded(object sender, RoutedEventArgs e)
        {
            contacts = await webService.GetcontactsList(App.AccountData.Data.Sip_Settings.Sip_username, App.AccountData.Data.User_Data.CompanyID, App.UserPbx);
        }

        private async void CallButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(NumberFild.Text))
            {
                return;
            }
            await CallNumber(NumberFild.Text);
        }
        private async Task CallNumber(string phone)
        {
            if (DialpadPage.currentCall == null)
            {
                try
                {
                    await CoreService.Instance.OpenMicrophonePopup();
                }
                catch
                {
                    phonePage.errorService.ShowError("Microphone not found");
                    return;
                }
                foreach (var regex in ProfilePage.regexNotes.Where(r => r.Check))
                {
                    phone = phone.Replace(regex.Search, regex.Replace);
                }
                CoreService.Instance.Call(phone);
                NumberFild.Text = "";
                Frame.Navigate(typeof(ActivCallPage), phonePage);
            }
        }
        private void NumberFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            try
            {
                if (String.IsNullOrEmpty(NumberFild.Text))
                {
                    Backspace.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Backspace.Visibility = Visibility.Visible; ;
                }
                if (contacts.contacts.Count > 0)
                {
                    ContactList.Items.Clear();
                    var contact = contacts.contacts.Where(c => c.Telephone.Contains(NumberFild.Text)).FirstOrDefault();
                    if (contact != null && !String.IsNullOrEmpty(NumberFild.Text))
                    {
                        ContactList.Items.Add(new CallItem(contact.Name, contact.Telephone));
                    }
                }
            }
            catch
            {
                return;
            }

        }

        private async void ContactList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CallItem callItem = (CallItem)ContactList.SelectedItem;
            if (callItem != null)
            {
                await CallNumber(callItem.UserPhone);
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            NumberFild.Text += b.Content;
        }
        private void PhonePage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        private async void NumberFild_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (String.IsNullOrEmpty(NumberFild.Text))
                {
                    return;
                }
                await CallNumber(NumberFild.Text);
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
        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(NumberFild.Text))
            {
                NumberFild.Text = NumberFild.Text.Substring(0, NumberFild.Text.Length - 1);
            }
        }
    }
}
