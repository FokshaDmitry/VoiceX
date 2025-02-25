using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views.ControlPages;

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
        public DialpadCallPage()
        {
            this.InitializeComponent();
            this.Loaded += DialpadCallPage_Loaded;
            webService = new WebService();
            contacts = new contacts_list();
            contacts.contacts = new List<Models.Contact>();
        }
        private async void DialpadCallPage_Loaded(object sender, RoutedEventArgs e)
        {
            contacts = await webService.GetcontactsList(App.AccountData?.Data.Sip_Settings.Sip_username!, App.AccountData?.Data.User_Data.CompanyID!, App.UserPbx!, App.userToken!);
        }

        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(NumberFild.Text))
            {
                return;
            }
            CallNumber(NumberFild.Text);
        }
        private void CallNumber(string phone)
        {
            if (CoreService.activeCall == null)
            {
                foreach (var regex in ProfilePage.regexNotes?.Where(r => r.Check)!)
                {
                    phone = phone.Replace(regex.Search!, regex.Replace);
                }
                try
                {
                    CoreService.Instance.MakeCall(phone, App.AccountData?.Data.Sip_Settings.Sip_server!);
                }
                catch
                {
                    ProfilePage.window!.ShowError("Microphone not found");
                    return;
                }
                NumberFild.Text = "";
            }
        }

        private void ContactList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CallItem callItem = (CallItem)ContactList.SelectedItem;
            if (callItem != null)
            {
                CallNumber(callItem.UserPhone);
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            NumberFild.Text += b.Content;
        }

        private void NumberFild_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (String.IsNullOrEmpty(NumberFild.Text))
                {
                    return;
                }
                CallNumber(NumberFild.Text);
            }
        }
        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(NumberFild.Text))
            {
                NumberFild.Text = NumberFild.Text.Substring(0, NumberFild.Text.Length - 1);
            }
        }

        private void NumberFild_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(NumberFild.Text))
                {
                    Backspace.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Backspace.Visibility = Visibility.Visible; 
                }
                if (contacts.contacts!.Count > 0)
                {
                    ContactList.Items.Clear();
                    var contact = contacts.contacts.Where(c => c.Telephone!.Contains(NumberFild.Text)).FirstOrDefault();
                    if (contact != null && !String.IsNullOrEmpty(NumberFild.Text))
                    {
                        ContactList.Items.Add(new CallItem(contact.Name!, contact.Telephone!));
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}
