using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using VoiceX.Interfeces;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ClientPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContactsPage : Page, IMoreItems
    {
        contacts_list contacts;
        readonly WebService webService;
        public ContactsPage()
        {
            this.InitializeComponent();
            this.Loaded += ContactsPage_Loaded;
            webService = new WebService();
            contacts = new contacts_list
            {
                contacts = new List<Models.Contact>()
            };
        }

        private async void ContactsPage_Loaded(object sender, RoutedEventArgs e)
        {
            ProfilePage.window!.LoadIcone.Visibility = Visibility.Visible;
            contacts = await webService.GetcontactsList(App.UserPbx!, App.userToken!, App.fw!);
            ProfilePage.window!.LoadIcone.Visibility = Visibility.Collapsed;
            if (contacts.ResponseCode != HttpStatusCode.OK || contacts.contacts == null)
            {
                contacts.contacts = new List<Models.Contact>();
            }
            AddMoreNotes();
            var clients = ProfilePage.LDAPService?.GetLdapUsers(50, App.AccountData?.Data.Ldap_Settings.Base!);
            ContactsList.Items.Clear();
            if (clients != null && clients.Count != 0)
            {
                clients = clients.Where(c => !contacts.contacts.Select(u => u.Telephone).Contains(c.Phone)).ToList();
                var groupContacts = clients.GroupBy(c => c.Name?[0].ToString().ToUpper()).OrderBy(c => c.Key);
                foreach (var groupcontact in groupContacts.ToList())
                {
                    if (groupcontact.Key!.All(char.IsDigit))
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key?.ToString()!));
                    else
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key?.ToString()! + groupcontact.Key?.ToString()!.ToUpper().ToLower()));
                    int color = 0;
                    foreach (var client in groupcontact)
                    {
                        ContactsList.Items.Add(new Items.Contact(client.Name!, client.Phone!, color));
                        color = color == 0 ? 1 : 0;
                    }
                }
                ContactsList.Items.Add(new MoreItems(this));
            }
        }
        public void AddMoreNotes()
        {
            int count = ContactsList.Items.Count;
            Debug.WriteLine(count);
            count += 25;
            ContactsList.Items.Clear();
            var clients = ProfilePage.LDAPService?.GetLdapUsers(count, App.AccountData?.Data.Ldap_Settings.Base!);

            if (clients != null && clients.Count != 0)
            {
                clients = clients.Where(c => !contacts.contacts!.Select(u => u.Telephone).Contains(c.Phone)).ToList();
                var groupContacts = clients.GroupBy(c => c.Name![0].ToString().ToUpper()).OrderBy(c => c.Key);
                foreach (var groupcontact in groupContacts.ToList())
                {
                    if (groupcontact.Key.All(char.IsDigit))
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString()));
                    else
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                    int color = 0;
                    foreach (var client in groupcontact)
                    {
                        ContactsList.Items.Add(new Items.Contact(client.Name!, client.Phone!, color));
                        color = color == 0 ? 1 : 0;
                    }
                }
                ContactsList.Items.Add(new MoreItems(this));
            }

        }
        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ContactsList.Items.Clear();
            var search = Search.Text;
            if (!String.IsNullOrEmpty(search))
            {
                var clients = ProfilePage.LDAPService?.SearchLdaps(App.AccountData?.Data.Ldap_Settings.Base!, search);
                if (clients != null && clients.Count != 0)
                {
                    clients = clients.Where(c => !contacts.contacts!.Select(u => u.Telephone).Contains(c.Phone)).ToList();
                    var groupContacts = clients.GroupBy(c => c.Name?[0].ToString().ToUpper()).OrderBy(c => c.Key);
                    foreach (var groupcontact in groupContacts.ToList())
                    {
                        if (groupcontact.Key!.All(char.IsDigit))
                            ContactsList.Items.Add(new HeadingContactList(groupcontact.Key?.ToString()!));
                        else
                            ContactsList.Items.Add(new HeadingContactList(groupcontact.Key?.ToString()! + groupcontact.Key?.ToString()!.ToUpper().ToLower()));
                        int color = 0;
                        foreach (var client in groupcontact)
                        {
                            ContactsList.Items.Add(new Items.Contact(client.Name!, client.Phone!, color));
                            color = color == 0 ? 1 : 0;
                        }
                    }
                }
            }
        }
    }
}
