using Newtonsoft.Json;
using pj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.Enums;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;

namespace VoiceX.Views
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Grid
    {
        public static List<Regex_note> regexNotes;
        readonly WebService webService;
        readonly AddDbContext addDbContext;
        public static Get_pauses getPauses;
        //readonly ErrorService errorService;
        GeneralSettingPage generalSettingPage;
        MainWindow window;
        public static MyCall currentCall { get; set; }
        public static List<string> SelectContacts { get; set; }
        public CoreService Core { get; } = CoreService.Instance;
        public static List<string> CallAdtess { get; set; }
        public static StatusCall StatusCall { get; set; }
        KeyPads keyPads;
        contacts_list contacts;
        public static bool TerminateAllCalls { get; set; }
        public static List<string> AutoAnswerNumbers { get; set; }
        DialpadCallPage dialpadCallPage;
        public ProfilePage(MainWindow mainWindow)
        {
            this.InitializeComponent();
            window = mainWindow;
            webService = new WebService();
            addDbContext = new AddDbContext();
            generalSettingPage = new GeneralSettingPage(mainWindow);
            regexNotes = new List<Regex_note>();
            CallAdtess = new List<string>();
            SelectContacts = new List<string>();
            TerminateAllCalls = false;
            if (AutoAnswerNumbers == null) AutoAnswerNumbers = new List<string>();
            dialpadCallPage = new DialpadCallPage(this);
            contacts = new contacts_list
            {
                contacts = new List<Models.Contact>()
            };
            //errorService = new ErrorService();
        }

        private void ControlPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {


        }
        private async void ControlPage_Loaded(object sender, RoutedEventArgs e)
        {
            var account = App.AccountData.Data.Sip_Settings;
            CoreService.Instance.Login(account.Sip_username, account.Sip_server, account.Sip_proxy, account.Sip_secret, 0);
            this.SizeChanged += ControlPage_SizeChanged;
            General.Checked += Filter_Checked;
            Addition.Checked += Filter_Checked;
            C2C.Checked += Filter_Checked;
            ContentControl.Children.Add(generalSettingPage);

            contacts = await webService.GetcontactsList(App.AccountData.Data.Sip_Settings.Sip_username, App.AccountData.Data.User_Data.CompanyID, App.UserPbx, App.userToken);
            try
            {
                //User RegEx
                //if (ApplicationData.Current.LocalSettings.Values["regexs"] != null)
                //{
                //    regexNotes = JsonConvert.DeserializeObject<List<Regex_note>>(ApplicationData.Current.LocalSettings.Values["regexs"].ToString());
                //}
            }
            catch
            {
                regexNotes = new List<Regex_note>();
            }
        }
        
        #region Navigete Button
        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            //App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Contacts":
                    
                    break;
                case "Phone":
                    ControlMainPage.Children.Clear();
                    ControlMainPage.Children.Add(dialpadCallPage);
                    break;
                case "History":

                    break;
                case "Fax":
                    
                    break;
                case "HotKeys":
                    
                    break;
            }
        }
        #endregion
        //General Page Navigate
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 253, 254, 255));

            if (filter.Name == "General")
            {
                if (GeneralCheck != null)
                {
                    GeneralCheck.Background = blueLine;
                    C2CCheck.Background = whiteLine;
                    AdditionChek.Background = whiteLine;
                }
                ContentControl.Children.Clear();
                ContentControl.Children.Add(generalSettingPage);
                //ContentControl.Navigate(typeof(GeneralSettingPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if (filter.Name == "C2C")
            {
                //var NTrasform = ContentControl.Content.ToString() == "VoiceX.Views.ControlPages.GeneralSettingPage" ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
                if (C2CCheck != null)
                {
                    GeneralCheck.Background = whiteLine;
                    C2CCheck.Background = blueLine;
                    AdditionChek.Background = whiteLine;
                }
                //ContentControl.Navigate(typeof(ClickToCallPage), "", new SlideNavigationTransitionInfo() { Effect = NTrasform });
            }
            else if (filter.Name == "Addition")
            {
                if (AdditionChek != null)
                {
                    GeneralCheck.Background = whiteLine;
                    C2CCheck.Background = whiteLine;
                    AdditionChek.Background = blueLine;
                }
                //ContentControl.Navigate(typeof(AdditionPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
        private void Navigate_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top - 1, 0, 0);
        }

        private void Navigate_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top + 1, 0, 0);
        }
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Margin.Bottom == -50)
            {
                Menu.Margin = new Thickness(0, 0, 0, 0);
                Butter.Visibility = Visibility.Collapsed;
                Cross.Visibility = Visibility.Visible;
            }
            else
            {
                Menu.Margin = new Thickness(0, 0, 0, -50);
                Butter.Visibility = Visibility.Visible;
                Cross.Visibility = Visibility.Collapsed;
            }
        }
        private void ControlPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //App.timeOut = DateTime.Now;
        }

        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            if (getPauses == null)
            {
                getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                getPauses.ResponseData.Pauses = new List<Pause>();
                getPauses = await webService.GetPauses(App.AccountData.Data.Sip_Settings.Sip_username, App.UserPbx);
                if (getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, getPauses.ResponseData.Pause_active == 0));
                    foreach (var pause in getPauses.ResponseData.Pauses)
                    {
                        PauseList.Items.Add(new PauseItem(pause, pause.Id == getPauses.ResponseData.Pause_active));
                    }
                }
                else
                {
                    //errorService.ShowWarning(getPauses.ResponseMessage);
                }
            }
            else
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, getPauses.ResponseData.Pause_active == 0));
                foreach (var pause in getPauses.ResponseData.Pauses)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == getPauses.ResponseData.Pause_active));
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
                    if (getPauses.ResponseData.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData.Data.Sip_Settings.Sip_username, id, App.UserPbx);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            getPauses.ResponseData.Pause_active = id;
                        }
                        else
                        {
                            //errorService.ShowWarning(result.ResponseMessage);
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


        private void Filter_Checked_1(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 253, 254, 255));
            if (filter.Name == "Keypad")
            {
                if (GeneralCheck1 != null)
                {
                    GeneralCheck1.Background = blueLine;
                    AdditionChek1.Background = whiteLine;
                    ContactListPad.Visibility = Visibility.Collapsed;
                }
            }
            else if (filter.Name == "Contactspad")
            {
                GeneralCheck1.Background = whiteLine;
                AdditionChek1.Background = blueLine;
                ContactListPad.Visibility = Visibility.Visible;
            }
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
