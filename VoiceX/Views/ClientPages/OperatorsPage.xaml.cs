using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ClientPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OperatorsPage : Page
    {
        readonly WebService webService;
        contacts_list contacts;
        ClientsPage clientsPage;
        public OperatorsPage()
        {
            this.InitializeComponent();
            webService = new WebService(App.userToken);
            contacts = new contacts_list
            {
                contacts = new List<Models.Contact>()
            };
            clientsPage = new ClientsPage();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                clientsPage = (ClientsPage)e.Parameter;
            }
            
        }
        private void SearchFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            SolidColorBrush magnifyingGlassColorGrey = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
            SolidColorBrush magnifyingGlassColorBlack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            ContactsList.Items.Clear();

            if (String.IsNullOrEmpty(Search.Text))
            {
                var groupContacts = ClientsPage.userContactsList.GroupBy(c => c.contactName[0].ToString().ToUpper()).OrderBy(c => c.Key);
                foreach (var groupcontact in groupContacts.ToList())
                {
                    if (groupcontact.Key.All(char.IsDigit))
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString()));
                    else
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                    foreach (var contact in groupcontact)
                    {
                        ContactsList.Items.Add(contact);
                    }
                }
                magnifyingGlass.Margin = new Thickness(0, 0, 5, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorGrey;
                magnifyingGlassLine.Background = magnifyingGlassColorGrey;
            }
            else
            {
                foreach (var contact in ClientsPage.userContactsList)
                {
                    if (contact.contactName.Contains(Search.Text))
                    {
                        ContactsList.Items.Add(contact);
                    }
                    else if (contact.contactPhone.Contains(Search.Text))
                    {
                        ContactsList.Items.Add(contact);
                    }
                }
                magnifyingGlass.Margin = new Thickness(0, 0, 23, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorBlack;
                magnifyingGlassLine.Background = magnifyingGlassColorBlack;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int color = 1;
            contacts = await webService.GetcontactsList(App.AccountData.Data.Sip_Settings.Sip_username, App.AccountData.Data.User_Data.CompanyID, App.UserPbx);
            if (contacts.ResponseCode == HttpStatusCode.OK)
            {
                if (contacts.contacts != null)
                {

                    var groupContacts = contacts.contacts.GroupBy(c => c.Name[0].ToString().ToUpper()).OrderBy(c => c.Key);
                    foreach (var groupcontact in groupContacts)
                    {
                        if (groupcontact.Key.All(char.IsDigit))
                            ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString()));
                        else
                            ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                        foreach (var contact in groupcontact)
                        {
                            ContactsList.Items.Add(new Items.Contact(contact.Name, contact.Telephone, color));
                            ClientsPage.userContactsList.Add(new Items.Contact(contact.Name, contact.Telephone, color));
                            color = color == 1 ? 0 : 1;
                        }
                    }
                }

            }
            else
            {
                clientsPage.errorService.ShowError(contacts.ResponseMessage);
            }
        }
        private void ContactsPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }

        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
    }
}
