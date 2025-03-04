using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.DAL.Entity;
using VoiceX.Enums;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views.ClientPages;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;

namespace VoiceX.Views
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public static List<Regex_note>? regexNotes;
        readonly WebService webService;
        AddDbContext addDbContext;
        public static Get_pauses? getPauses;
        public static MainWindow? window {  get; set; }
        public static List<string>? SelectContacts { get; set; }
        public CoreService Core { get; } = CoreService.Instance;
        public static StatusCall StatusCall { get; set; }
        public static ClickToCallService? clickToCallService { get; set; }
        KeyPads keyPads;
        contacts_list contacts;
        public static bool TerminateAllCalls { get; set; }
        public static List<string>? AutoAnswerNumbers { get; set; }
        LocalStoreService localStoreService;
        GeneralSettingPage generalSettingPage;
        DialpadCallPage dialpadCallPage;
        ActivCallPage activCallPage;
        CallPage callPage;
        ClientsPage clientsPage;
        HistoryPage historyPage;
        ClickToCallPage clickToCallPage;
        FaxPage faxPage;
        Storyboard slide;
        Storyboard slideLeft;
        HotKeyPage hotKeyPage;
        AdditionPage additionPage;
        public IncomingWindow incomingWindow;
        public ProfilePage(MainWindow mainWindow)
        {
            this.InitializeComponent();
            window = mainWindow;
            webService = new WebService();
            generalSettingPage = new GeneralSettingPage(mainWindow);
            regexNotes = new List<Regex_note>();
            SelectContacts = new List<string>();
            TerminateAllCalls = false;
            if (AutoAnswerNumbers == null) AutoAnswerNumbers = new List<string>();
            dialpadCallPage = new DialpadCallPage();
            activCallPage = new ActivCallPage(this);
            callPage = new CallPage(this, dialpadCallPage, activCallPage);
            clientsPage = new ClientsPage();
            historyPage = new HistoryPage();
            faxPage = new FaxPage(this);
            hotKeyPage = new HotKeyPage();
            clickToCallPage = new ClickToCallPage();
            additionPage = new AdditionPage();
            contacts = new contacts_list
            {
                contacts = new List<Models.Contact>()
            };
            localStoreService = new LocalStoreService();
            slide = (Storyboard)FindResource("SlideUpAnimation");
            slideLeft = (Storyboard)FindResource("SlideLeftAnimation");
            clickToCallService = new ClickToCallService();
            incomingWindow = new IncomingWindow(mainWindow, this, activCallPage);
            window.moveOnDialpad += Window_moveOnDialpad;
            window.moveOnContact += Window_moveOnContact;
        }

        private void Window_moveOnContact()
        {

            MainFrame.Navigate(clientsPage);
            slide.Begin();
        }

        private void Window_moveOnDialpad()
        {
            MainFrame.Navigate(dialpadCallPage);
            slide.Begin();
        }

        public void Hotkeys_HotkeyPressed(string Phone)
        {

            if (!String.IsNullOrEmpty(Phone))
            {
                try
                {
                    if (CoreService.activeCall == null)
                    {
                        foreach (var regex in regexNotes?.Where(r => r.Check)!)
                        {
                            Phone = Phone.Replace(regex.Search!, regex.Replace);
                        }
                        try
                        {
                            CoreService.Instance.MakeCall(Phone, App.AccountData?.Data.Sip_Settings.Sip_server!);
                        }
                        catch
                        {
                            window?.ShowError("Microphone not found");
                            return;
                        }
                    }
                }
                catch
                {

                }
            }
        }
        private void Instance_OutgoingCallEvent()
        {
            window?.Show();
            window!.WindowState = WindowState.Normal;
            window.Activate();
            MainFrame.Navigate(activCallPage);
            slide.Begin();
            CoreService.activeCall!.EndCallEvent += ActiveCall_EndCallEvent;
            StatusCall = StatusCall.Outgoing;
        }

        private async void Instance_IncomingCallEvent()
        {
            if (CoreService.activeCall != null)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    var info = CoreService.activeCall.getInfo();
                    if (AutoAnswerNumbers!.Contains(info.remoteContact))
                    {
                        Thread.Sleep(3000);
                        CoreService.activeCall.Accept();
                        MainFrame.Navigate(activCallPage);
                        slide.Begin();
                    }
                    else
                    {

                        CoreService.activeCall?.PlayRingTone("Incoming");
                        var visible = window?.Visibility == Visibility.Visible;
                        if (visible) 
                        {
                            window?.Show();
                            window!.WindowState = WindowState.Normal;
                            window.Activate();
                            MainFrame.Navigate(callPage);
                            slide.Begin();
                            incomingWindow.ShowInBottomRight(ExtractValue(info.remoteContact), ExtractValue(info.remoteUri), !visible);
                        }
                        else
                        {
                            incomingWindow.ShowInBottomRight(ExtractValue(info.remoteContact), ExtractValue(info.remoteUri), !visible);
                        }
                    }
                    StatusCall = StatusCall.Incoming;
                    CoreService.activeCall!.EndCallEvent += ActiveCall_EndCallEvent;
                });
            }
        }

        private async void ActiveCall_EndCallEvent(string Name, string Phone, DateTime StartCall)
        {
            await Dispatcher.InvokeAsync(async () => {
                MainFrame.Navigate(dialpadCallPage);
                slide.Begin();
                if (StartCall == DateTime.MinValue)
                {
                    if (ProfilePage.StatusCall == StatusCall.Incoming)
                    {
                        ProfilePage.StatusCall = StatusCall.IncomeIgnore;
                    }
                    else if (ProfilePage.StatusCall == StatusCall.Outgoing)
                    {
                        ProfilePage.StatusCall = StatusCall.Ignore;
                    }
                }
                await addDbContext.AddNoteAcync(new HistoryNotes() { Id = Guid.NewGuid(), Name = ExtractValue(Name), Phone = ExtractValue(Phone), StartDialog = StartCall, EndDialog = DateTime.Now, StatusCall = ProfilePage.StatusCall });
                incomingWindow.Hide();
            });
        }
        public string ExtractValue(string input)
        {
            Match match = Regex.Match(input, "\"([^\"]+)\"");
            if (match.Success)
            {
                return match.Groups[1].Value; 
            }

            match = Regex.Match(input, @"sip:([^@]+)@");
            if (match.Success)
            {
                return match.Groups[1].Value; 
            }

            return string.Empty; 
        }
        private async void ControlPage_Loaded(object sender, RoutedEventArgs e)
        {
            addDbContext = new AddDbContext();
            var account = App.AccountData?.Data.Sip_Settings;
            CoreService.Instance.Login(account?.Sip_username!, account!.Sip_server, account.Sip_proxy, account.Sip_secret, 0);
            General.Checked += Filter_Checked;
            Addition.Checked += Filter_Checked;
            C2C.Checked += Filter_Checked;
            ContentControl.Navigate(generalSettingPage);
            CoreService.Instance.IncomingCallEvent += Instance_IncomingCallEvent;
            CoreService.Instance.OutgoingCallEvent += Instance_OutgoingCallEvent;

            contacts = await webService.GetcontactsList(App.AccountData?.Data.Sip_Settings.Sip_username!, App.AccountData?.Data.User_Data.CompanyID!, App.UserPbx!, App.userToken!);
            var AAlist = await localStoreService.LoadDataAsync("AACallList");
            if (AAlist != null)
            {
                AutoAnswerNumbers = JsonConvert.DeserializeObject<List<string>>(AAlist);
            }
            try
            {
                //User RegEx
                var regex = await localStoreService.LoadDataAsync("regexs");
                if (regex != null)
                {
                    regexNotes = JsonConvert.DeserializeObject<List<Regex_note>>(regex);
                }
            }
            catch
            {
                regexNotes = new List<Regex_note>();
            }
            clickToCallService!.HotkeyPressed += new ClickToCallService.HotkeyDelegate(Hotkeys_HotkeyPressed);
            clickToCallPage.OnChangeKey += clickToCallService.ChangeKey;
            clickToCallService.ChangeKey();
        }
        
        #region Navigete Button
        public void Navigate_Click(object sender, RoutedEventArgs e)
        {
            //App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Profile":
                    if (MainFrame.CanGoBack)
                    {
                        while (MainFrame.CanGoBack)
                        {
                            MainFrame.RemoveBackEntry();
                        }
                    }
                    MainFrame.Content = null;
                    break;
                case "Contacts":
                    MainFrame.Navigate(clientsPage);
                    slide.Begin();
                    break;
                case "Phone":
                    MainFrame.Navigate(dialpadCallPage);
                    slide.Begin();
                    break;
                case "History":
                    MainFrame.Navigate(historyPage);
                    slide.Begin();
                    break;
                case "Fax":
                    MainFrame.Navigate(faxPage);
                    slide.Begin();
                    break;
                case "HotKeys":
                    MainFrame.Navigate(hotKeyPage);
                    slide.Begin();
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
                ContentControl.Navigate(generalSettingPage);
                slideLeft.Begin();
            }
            else if (filter.Name == "C2C")
            {
                if (C2CCheck != null)
                {
                    GeneralCheck.Background = whiteLine;
                    C2CCheck.Background = blueLine;
                    AdditionChek.Background = whiteLine;
                }
                ContentControl.Navigate(clickToCallPage);
                slideLeft.Begin();
            }
            else if (filter.Name == "Addition")
            {
                if (AdditionChek != null)
                {
                    GeneralCheck.Background = whiteLine;
                    C2CCheck.Background = whiteLine;
                    AdditionChek.Background = blueLine;
                }
                ContentControl.Navigate(additionPage);
                slideLeft.Begin();
            }
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
        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            getPauses = new Get_pauses
            {
                ResponseData = new Status_pause()
            };
            getPauses.ResponseData.Pauses = new List<Pause>();
            getPauses = await webService.GetPauses(App.AccountData!.Data.Sip_Settings.Sip_username, App.UserPbx!, App.userToken!);
            if (getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, getPauses.ResponseData!.Pause_active == 0));
                foreach (var pause in getPauses.ResponseData.Pauses!)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == getPauses.ResponseData.Pause_active));
                }
            }
            else
            {
                window?.ShowError(getPauses.ResponseMessage!);
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
                    if (getPauses?.ResponseData!.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData!.Data.Sip_Settings.Sip_username, id, App.UserPbx!, App.userToken!);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            getPauses!.ResponseData!.Pause_active = id;
                        }
                        else
                        {
                            window?.ShowError(result.ResponseCode.ToString());
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

                        var groupContacts = contacts.contacts.GroupBy(c => c.Name?[0].ToString().ToUpper()).OrderBy(c => c.Key);
                        foreach (var groupcontact in groupContacts)
                        {
                            if (groupcontact.Key!.All(char.IsDigit))
                                ContactsList.Items.Add(new HeadingContactList(groupcontact.Key!.ToString()));
                            else
                                ContactsList.Items.Add(new HeadingContactList(groupcontact.Key!.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                            foreach (var contact in groupcontact)
                            {
                                ContactsList.Items.Add(new ContactPad(contact.Name!, contact.Telephone!, keyPads == KeyPads.AddCallPad));
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
                var groupContacts = contacts.contacts!.GroupBy(c => c.Name?.ToString().ToUpper()).OrderBy(c => c.Key);
                foreach (var groupcontact in groupContacts.ToList())
                {
                    if (groupcontact.Key!.All(char.IsDigit))
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key!.ToString()));
                    else
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key!.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                    foreach (var contact in groupcontact)
                    {
                        ContactsList.Items.Add(new ContactPad(contact.Name!, contact.Telephone!, keyPads == KeyPads.AddCallPad));
                    }
                }
                magnifyingGlass.Margin = new Thickness(0, 0, 5, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorGrey;
                magnifyingGlassLine.Fill = magnifyingGlassColorGrey;
            }
            else
            {
                foreach (var contact in contacts.contacts!)
                {
                    if (contact.Name!.Contains(Search.Text))
                    {
                        ContactsList.Items.Add(new ContactPad(contact.Name!, contact.Telephone!, keyPads == KeyPads.AddCallPad));
                    }
                    else if (contact.Telephone!.Contains(Search.Text))
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
            SelectContacts?.Clear();
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
            if (SelectContacts!.Count == 0)
            {
                return;
            }
            else
            {

                if (CoreService.activeCall != null)
                {
                    foreach (var contact in SelectContacts)
                    {
                        if (CoreService.activeCall.CallAdtess.Count == 0)
                        {

                        }
                        if (CoreService.activeCall.CallAdtess.Contains(contact))
                        {
                            CoreService.activeCall.CallAdtess.Add(contact);
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

                if (CoreService.activeCall != null)
                {
                    if (TitleNumpad.Text == "Forwarding")
                    {
                        CoreService.activeCall.TransferCall(KeypadFild.Text, App.AccountData!.Data.Sip_Settings.Sip_server);
                        NumpadFild.Visibility = Visibility.Collapsed;
                        return;
                    }
                    else
                    {
                        var uri = $"sip:{KeypadFild.Text}@{App.AccountData!.Data.Sip_Settings.Sip_server}";
                        if (CoreService.activeCall.CallAdtess?.Count == 0)
                        {
                            CoreService.activeCall.AddParticipant(KeypadFild.Text, App.AccountData.Data.Sip_Settings.Sip_server);
                        }
                        if (CoreService.activeCall.CallAdtess!.Contains(uri))
                        {
                            CoreService.activeCall.CallAdtess.Add(uri);

                        }
                    }
                }
                KeypadFild.Text = "";
            }
            NumpadFild.Visibility = Visibility.Collapsed;
        }

        private void NumDTMF_Click(object sender, RoutedEventArgs e)
        {
            if (CoreService.activeCall != null)
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
            if (CoreService.activeCall != null)
            {
                if (String.IsNullOrEmpty(sender.Text))
                {
                    Backspace.Visibility = Visibility.Collapsed;
                    CoreService.activeCall.dialDtmf(sender.Text);
                }
                else
                {
                    Backspace.Visibility = Visibility.Visible; ;
                }
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
            if (CoreService.activeCall != null)
            {
                if (String.IsNullOrEmpty(sender.Text))
                {
                    BackspaceNum.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BackspaceNum.Visibility = Visibility.Visible; 
                }
            }
        }
        private void Profile_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top - 1, Img.Margin.Right, Img.Margin.Bottom);
        }

        private void Profile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top + 1, Img.Margin.Right, Img.Margin.Bottom);
        }
    }
}
