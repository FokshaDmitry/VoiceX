using pj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VoiceX.DAL.Context;
using VoiceX.Enums;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// Interaction logic for DialpadPage.xaml
    /// </summary>
    public partial class DialpadPage : Grid
    {
        static DateTime StartCall { get; set; }
        contacts_list contacts;
        public static List<string> SelectContacts { get; set; }
        public static bool Ignore { get; private set; }
        public static bool TerminateAllCalls { get; set; }
        public static List<string> CallAdtess { get; set; }
        public static Call currentCall { get; set; }
        public static StatusCall StatusCall { get; set; }
        readonly AddDbContext addDbContext;
        public static List<string> AutoAnswerNumbers { get; set; }
        readonly WebService webService;
        public ErrorService errorService;
        KeyPads keyPads;
        public DialpadPage()
        {
            this.InitializeComponent();
            //Context
            addDbContext = new AddDbContext();

            TerminateAllCalls = false;
            SelectContacts = new List<string>();
            webService = new WebService();
            contacts = new contacts_list
            {
                contacts = new List<Models.Contact>()
            };
            CallAdtess = new List<string>();
            if (AutoAnswerNumbers == null) AutoAnswerNumbers = new List<string>();
            errorService = new ErrorService(MainGrid);
        }
        public void AddContactsList(KeyPads transferPad)
        {
            keyPads = transferPad;
            switch (keyPads)
            {
                case KeyPads.DTMFPad:
                    KeyPad.Visibility = Visibility.Visible;
                    break;
                case KeyPads.TransferPad:
                    NumpadFild.Visibility = Visibility.Visible;
                    TitleNumpad.Text = "Forwarding";
                    ContactsList.Margin = new Thickness(0, 40, 0, 0);
                    break;
                case KeyPads.AddCallPad:
                    NumpadFild.Visibility = Visibility.Visible;
                    TitleNumpad.Text = "Add contacts";
                    ContactsList.Margin = new Thickness(0, 40, 0, 50);
                    break;
                default:
                    break;
            }
            if (keyPads != KeyPads.DTMFPad)
            {
                ContactsList.Items.Clear();
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
                                ContactsList.Items.Add(new ContactPad(contact.Name, contact.Telephone, keyPads == KeyPads.AddCallPad));
                            }
                        }
                    }

                }
            }

        }


        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 253, 254, 255));
            if (filter.Name == "Keypad")
            {
                if (GeneralCheck != null)
                {
                    GeneralCheck.Background = blueLine;
                    AdditionChek.Background = whiteLine;
                    ContactListPad.Visibility = Visibility.Collapsed;
                }
            }
            else if (filter.Name == "Contactspad")
            {
                GeneralCheck.Background = whiteLine;
                AdditionChek.Background = blueLine;
                ContactListPad.Visibility = Visibility.Visible;
            }
        }
        private void PhonePage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
        private void SearchFild_TextChanging(TextBox sender, TextChangedEventArgs args)
        {
            SolidColorBrush magnifyingGlassColorGrey = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
            SolidColorBrush magnifyingGlassColorBlack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            ContactsList.Items.Clear();
            if (String.IsNullOrEmpty(Search.Text))
            {
                var groupContacts = contacts.contacts.GroupBy(c => c.Name.ToString().ToUpper()).OrderBy(c => c.Key);
                foreach (var groupcontact in groupContacts.ToList())
                {
                    if (groupcontact.Key.All(char.IsDigit))
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString()));
                    else
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                    foreach (var contact in groupcontact)
                    {
                        ContactsList.Items.Add(new ContactPad(contact.Name, contact.Telephone, keyPads == KeyPads.AddCallPad));
                    }
                }
                magnifyingGlass.Margin = new Thickness(0, 0, 5, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorGrey;
                magnifyingGlassLine.Fill = magnifyingGlassColorGrey;
            }
            else
            {
                foreach (var contact in contacts.contacts)
                {
                    if (contact.Name.Contains(Search.Text))
                    {
                        ContactsList.Items.Add(new ContactPad(contact.Name, contact.Telephone, keyPads == KeyPads.AddCallPad));
                    }
                    else if (contact.Telephone.Contains(Search.Text))
                    {
                        ContactsList.Items.Add(new ContactPad(contact.Name, contact.Telephone, keyPads == KeyPads.AddCallPad));
                    }
                }
                magnifyingGlass.Margin = new Thickness(0, 0, 23, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorBlack;
                magnifyingGlassLine.Fill = magnifyingGlassColorBlack;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SelectContacts.Clear();
            KeypadFild.Text = "";
            DTMFFild.Text = "";
            NumpadFild.Visibility = Visibility.Collapsed;
            KeyPad.Visibility = Visibility.Collapsed;
            PausesFild.Visibility = Visibility.Collapsed;
        }

        private void Num_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            KeypadFild.Text += b.Content;
        }

        private void AddContacts_Click(object sender, RoutedEventArgs e)
        {
            if (SelectContacts.Count == 0)
            {
                return;
            }
            else
            {

                if (currentCall != null)
                {
                    foreach (var contact in SelectContacts)
                    {
                        if (CallAdtess.Count == 0)
                        {
                            
                        }
                        if (!CallAdtess.Contains(contact))
                        {
                            CallAdtess.Add(contact);
                        }
                    }
                }
                SelectContacts.Clear();
            }
            NumpadFild.Visibility = Visibility.Collapsed;
        }
        private void AddContact_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(KeypadFild.Text))
            {
                NumpadFild.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {

                if (currentCall != null)
                {
                    if (TitleNumpad.Text == "Forwarding")
                    {
                        
                        NumpadFild.Visibility = Visibility.Collapsed;
                        return;
                    }
                    else
                    {
                        if (CallAdtess.Count == 0)
                        {

                            
                        }
                        if (!CallAdtess.Contains(KeypadFild.Text))
                        {
                            CallAdtess.Add(KeypadFild.Text);
                            
                        }
                    }
                }
                KeypadFild.Text = "";
            }
            NumpadFild.Visibility = Visibility.Collapsed;
        }

        private void NumDTMF_Click(object sender, RoutedEventArgs e)
        {
            if (currentCall != null)
            {
                var button = (Button)sender;
                DTMFFild.Text += button.Content;
            }
        }
        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            if (ProfilePage.getPauses == null)
            {
                ProfilePage.getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                ProfilePage.getPauses.ResponseData.Pauses = new List<Pause>();
                ProfilePage.getPauses = await webService.GetPauses(App.AccountData.Data.Sip_Settings.Sip_username, App.UserPbx);
                if (ProfilePage.getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ProfilePage.getPauses.ResponseData.Pause_active == 0));
                    foreach (var pause in ProfilePage.getPauses.ResponseData.Pauses)
                    {
                        PauseList.Items.Add(new PauseItem(pause, pause.Id == ProfilePage.getPauses.ResponseData.Pause_active));
                    }
                }
                else
                {
                    errorService.ShowWarning(ProfilePage.getPauses.ResponseMessage);
                }
            }
            else
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ProfilePage.getPauses.ResponseData.Pause_active == 0));
                foreach (var pause in ProfilePage.getPauses.ResponseData.Pauses)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == ProfilePage.getPauses.ResponseData.Pause_active));
                }
            }
            PausesFild.Visibility = Visibility.Visible;
        }
        private void PauseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            foreach (var item in list.Items)
            {
                var pause = (PauseItem)item;
                pause.SelectChange(pause == list.SelectedItem);
            }

        }
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pause = (PauseItem)PauseList.SelectedItem;
                if (pause != null)
                {
                    int id = pause.pause.Id;
                    if (ProfilePage.getPauses.ResponseData.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData.Data.Sip_Settings.Sip_username, id, App.UserPbx);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            ProfilePage.getPauses.ResponseData.Pause_active = id;
                        }
                        else
                        {
                            errorService.ShowWarning(result.ResponseMessage);
                            PauseList.SelectedIndex = -1;
                        }
                    }
                }

            }
            catch
            {
                PausesFild.Visibility = Visibility.Collapsed;
                return;
            }
            PausesFild.Visibility = Visibility.Collapsed;
        }

        private async void PhonePage_Loaded(object sender, RoutedEventArgs e)
        {
            contacts = await webService.GetcontactsList(App.AccountData.Data.Sip_Settings.Sip_username, App.AccountData.Data.User_Data.CompanyID, App.UserPbx);
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(DTMFFild.Text))
            {
                DTMFFild.Text = DTMFFild.Text.Substring(0, DTMFFild.Text.Length - 1);
            }
        }

        private void DTMFFild_TextChanging(TextBox sender, TextChangedEventArgs args)
        {
            if (String.IsNullOrEmpty(sender.Text))
            {
                Backspace.Visibility = Visibility.Collapsed;
            }
            else
            {
                Backspace.Visibility = Visibility.Visible; ;
            }
        }

        private void BackspaceNum_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(KeypadFild.Text))
            {
                KeypadFild.Text = KeypadFild.Text.Substring(0, KeypadFild.Text.Length - 1);
            }
        }
        private void KeypadFild_TextChanging(TextBox sender, TextChangedEventArgs args)
        {
            if (String.IsNullOrEmpty(sender.Text))
            {
                BackspaceNum.Visibility = Visibility.Collapsed;
            }
            else
            {
                BackspaceNum.Visibility = Visibility.Visible; ;
            }
        }
    }
}
