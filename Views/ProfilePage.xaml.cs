using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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
using static System.Net.Mime.MediaTypeNames;

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
        public static List<string>? AutoAnswerNumbers;
        public static bool onlineToken { get; set; } 
        public static LDAPService? LDAPService { get; set; }
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
        Dictionary<int, string> MenuIcons;
        private Timer _dailyTimer;

        public ProfilePage(MainWindow mainWindow)
        {
            this.InitializeComponent();
            window = mainWindow;
            webService = new WebService();
            generalSettingPage = new GeneralSettingPage(mainWindow);
            regexNotes = new List<Regex_note>();
            SelectContacts = new List<string>();
            if(AutoAnswerNumbers == null) AutoAnswerNumbers = new List<string>();
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
            LDAPService = new LDAPService();
            onlineToken = false;
            MenuIcons = new Dictionary<int, string>();
            window.moveOnDialpad += Window_moveOnDialpad;
            window.moveOnContact += Window_moveOnContact;
            window.moveOnHistory += Window_moveOnHistory;
            this.PreviewKeyDown += OnPreviewKeyDown;
        }
        private async Task SetVersion()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\"));
            var filePath = Path.Combine(projectRoot, "Installer.iss");
            if (!File.Exists(filePath))
                return;

            // 1. Читаем исходный текст
            string original = await File.ReadAllTextAsync(filePath);
            var match = Regex.Match(original, @"#define\s+MyAppVersion\s+""([^""]+)""");
            string version = "";
            if (match.Success)
            {
                version = match.Groups[1].Value;
            }
            else
            {
                return;

            }
            if (version != window?.Version)
            {
                string updated = original.Replace(version, window?.Version);
                await File.WriteAllTextAsync(filePath, updated);
            }
        } 
        private void Window_moveOnHistory()
        {
            historyPage.IgnoreCall.IsChecked = true;
            MainFrame.Navigate(historyPage);
            slide.Begin();
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
                        if (Phone.Length >= 8 && Phone.Length <= 10)
                        {
                            if (Phone[0] != '0')
                            {
                                Phone = "0" + Phone;
                            }
                        }
                        foreach (var regex in ProfilePage.regexNotes?.Where(r => r.Check)!)
                        {
                            Phone = Phone.Replace(regex.Search!, regex.Replace);
                        }
                        Phone = Regex.Replace(Phone, @"[^0-9*#]", "");
                        if (String.IsNullOrEmpty(Phone))
                        {
                            ProfilePage.window?.ShowError("Number is empty.");
                            return;
                        }
                        try
                        {
                           var call = CoreService.Instance.MakeCall(Phone, App.AccountData?.Data.Sip_Settings.Sip_server!);
                            if (call == null)
                            {
                                ProfilePage.window?.ShowError("Call not create. Please check connection and audio.");
                            }
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
                await Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        var info = CoreService.activeCall.getInfo();
                        
                        if (AutoAnswerNumbers != null && AutoAnswerNumbers.Contains(info.remoteContact))
                        {
                            Thread.Sleep(3000);
                            CoreService.activeCall.Accept();
                            MainFrame.Navigate(activCallPage);
                            slide.Begin();
                        }
                        else
                        {
                            var idDev = await localStoreService.LoadDataAsync("ring");
                            int id = 0;
                            int.TryParse(idDev, out id);
                            CoreService.activeCall?.PlayIncomingRing(id);
                            var visible = window?.Visibility == Visibility.Visible;
                            if (visible)
                            {
                                window?.Show();
                                window!.WindowState = WindowState.Normal;
                                window.Activate();
                                MainFrame.Navigate(callPage);
                                slide.Begin();
                                if (!String.IsNullOrEmpty(info.remoteContact))
                                {
                                    var userName = ExtractValue(info.remoteContact);
                                    var contactName = ProfilePage.LDAPService?.SearchLdaps(App.AccountData?.Data.Ldap_Settings.Base!, userName).Where(l => l.Phone == userName).Select(l => l.Name).FirstOrDefault();
                                    incomingWindow.ShowInBottomRight(String.IsNullOrEmpty(contactName) ? userName : contactName, userName, !visible);
                                }
                                else
                                {
                                    incomingWindow.ShowInBottomRight("No Informations", "No Informations", !visible);
                                }
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(info.remoteContact))
                                {
                                    var userName = ExtractValue(info.remoteContact);
                                    var contactName = ProfilePage.LDAPService?.SearchLdaps(App.AccountData?.Data.Ldap_Settings.Base!, userName).Where(l => l.Phone == userName).Select(l => l.Name).FirstOrDefault();
                                    incomingWindow.ShowInBottomRight(String.IsNullOrEmpty(contactName) ? userName : contactName, userName, !visible);
                                }
                                else
                                {
                                    incomingWindow.ShowInBottomRight("No Informations", "No Informations", !visible);
                                }
                            }
                        }
                        StatusCall = StatusCall.Incoming;
                        if (CoreService.activeCall != null)
                        {
                            CoreService.activeCall.EndCallEvent += ActiveCall_EndCallEvent;
                        }
                    }
                    catch
                    {

                    }
                });
            }
        }
        private async void ActiveCall_EndCallEvent(string Name, string Phone, DateTime StartCall)
        {
            await Dispatcher.InvokeAsync(async () => {
                Navigate_Click(Dialpad, new RoutedEventArgs());
                slide.Begin();
                incomingWindow.Hide();
                foreach (var regex in ProfilePage.regexNotes?.Where(r => r.Check)!)
                {
                    Phone = Phone.Replace(regex.Search!, regex.Replace);
                }
                Phone = Regex.Replace(Phone, @"\D", "");
                
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
                
                var phone = LDAPService?.SearchLdaps(App.AccountData?.Data.Ldap_Settings.Base!, Phone);
                if (phone != null) 
                {
                    if (phone.Count() != 0)
                    {
                        var phoneNumber = phone.FirstOrDefault(p => p.Phone == Phone);
                        if (phoneNumber != null)
                        {
                            if (!String.IsNullOrEmpty(phoneNumber.Name))
                            {
                                Name = phoneNumber.Name;
                                if (!String.IsNullOrEmpty(phoneNumber.Phone))
                                {
                                    Phone = phoneNumber.Phone;
                                }
                            }
                        }
                    }
                }
                if (Name.All(char.IsDigit))
                {
                    if (Name.Length >= 8 && Name.Length <= 10)
                    {
                        if (Name.First() != '0')
                        {
                            Name = "0" + Name;
                        }
                    }
                }
                if (Phone.Length >= 8 && Phone.Length <= 10)
                {
                    if (Phone.First() != '0')
                    {
                        Phone = "0" + Phone;
                    }
                }
                await addDbContext.AddNoteAcync(new HistoryNotes() { Id = Guid.NewGuid(), Name = Name, Phone = Phone, StartDialog = StartCall, EndDialog = DateTime.Now, StatusCall = ProfilePage.StatusCall });
                if (ProfilePage.StatusCall == StatusCall.IncomeIgnore)
                {
                    if (window != null)
                    {
                        window.TrayIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Assets/icone_v2/TrayIgnoreIcon.ico"));
                        if (window.TrayIcon.ToolTipText == "VoiceX")
                        {
                            window.TrayIcon.ToolTipText = Phone;
                        }
                        else
                        {
                            if (!window.TrayIcon.ToolTipText!.Contains(Phone))
                            {
                                window.TrayIcon.ToolTipText += ",\n" + Phone;
                            }
                            else
                            {
                                window.TrayIcon.ToolTipText = AddOrIncrement(window.TrayIcon.ToolTipText!, Phone);
                            }
                        }
                    }
                }
            });
        }
        public string AddOrIncrement(string source, string number)
        {
            var parts = source.Split(',')
                              .Select(p => p.Trim())
                              .ToList();

            bool found = false;

            for (int i = 0; i < parts.Count; i++)
            {
                // 998 | 998 (x2) | 998(x10)
                var match = Regex.Match(
                    parts[i],
                    $@"^{number}(?:\s*\(x(\d+)\))?$"
                );

                if (match.Success)
                {
                    int count = match.Groups[1].Success
                        ? int.Parse(match.Groups[1].Value) + 1
                        : 2;

                    parts[i] = $"{number} (x{count})";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                parts.Add(number.ToString());
            }

            return string.Join(",\n", parts);
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
            var items = MenuIcons.OrderByDescending(m => m.Key);
            
            addDbContext = new AddDbContext();
            addDbContext.InitializeDB();
            await LDAPService?.Authenticate(App.AccountData?.Data.Ldap_Settings.Dn!, App.AccountData?.Data.Ldap_Settings.Pass!, App.AccountData?.Data.Ldap_Settings.Server!)!;
            string mic = await localStoreService.LoadDataAsync("micro");
            string audio = await localStoreService.LoadDataAsync("audio");
            General.Checked += Filter_Checked;
            Addition.Checked += Filter_Checked;
            C2C.Checked += Filter_Checked;
            ContentControl.Navigate(generalSettingPage);
            CoreService.Instance.IncomingCallEvent += Instance_IncomingCallEvent;
            CoreService.Instance.OutgoingCallEvent += Instance_OutgoingCallEvent;

            window!.LoadIcone.Visibility = Visibility.Visible;
            contacts = await webService.GetcontactsList(App.UserPbx!, App.userToken!, App.fw!);
            window!.LoadIcone.Visibility = Visibility.Collapsed;
            var AAlist = await localStoreService.LoadDataAsync("AACallList");
            if (!String.IsNullOrEmpty(AAlist))
            {
                AutoAnswerNumbers = JsonConvert.DeserializeObject<List<string>>(AAlist);
                if (AutoAnswerNumbers == null) AutoAnswerNumbers = new List<string>();
            }
            try
            {
                //User RegEx
                var regex = await localStoreService.LoadDataAsync("regexs");
                if (!String.IsNullOrEmpty(regex))
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
            var manager = CoreService.Instance.Core.audDevManager();
            _dailyTimer = new Timer(async _ =>
            {
                await addDbContext.DeleteOldLogsAsync();
            }, null, TimeSpan.FromHours(2), TimeSpan.FromHours(2));

            if (!String.IsNullOrEmpty(mic))
            {
                int id;
                var res = int.TryParse(mic, out id);
                if (res)
                {
                    manager.setCaptureDev(id);
                }
            }
            if (!String.IsNullOrEmpty(audio))
            {
                int id;
                var res = int.TryParse(audio, out id);
                if (res)
                {
                    manager.setPlaybackDev(id);
                }
            }
            await SetVersion();
            var lang = App.Language.Name;
            if (lang == "he-IL") 
            {
                
            }
            //ChangePerspectiveIl();
        }
        #region Navigete Button
        public void Navigate_Click(object sender, RoutedEventArgs e)
        {
            var Navigate = (Button)sender;
            var paths = MenuItems.Children.OfType<Button>()
                                  .Select(button => button.Content)
                                  .OfType<Border>()
                                  .Select(border => border.Child).OfType<System.Windows.Shapes.Path>();

            // Устанавливаем белый цвет для каждого Path
            foreach (var path in paths)
            {
                path.Fill = Brushes.White;
            }
            var border = Navigate.Content as Border;
            if (border != null)
            {
                var select = border.Child as System.Windows.Shapes.Path;
                if (select != null)
                {
                    select.Fill = new SolidColorBrush(Color.FromArgb(255, 123, 118, 255));
                }
            }
            var page = Navigate.Name;
            if (!String.IsNullOrEmpty(page))
            {
                switch (page)
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
                    case "Dialpad":
                        if (CoreService.activeCall != null)
                        {
                            MainFrame.Navigate(activCallPage);
                        }
                        else
                        {
                            MainFrame.Navigate(dialpadCallPage);
                        }
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
                    case "Hotkeys":
                        MainFrame.Navigate(hotKeyPage);
                        slide.Begin();
                        break;
                    default:

                        break;
                }
            }
        }
        #endregion
        //General Page Navigate
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;

            if (filter.Name == "General")
            {
                if (GeneralCheck != null)
                {
                    GeneralCheck.Visibility = Visibility.Visible;
                    C2CCheck.Visibility = Visibility.Collapsed;
                    AdditionChek.Visibility = Visibility.Collapsed;
                }
                ContentControl.Navigate(generalSettingPage);
                slideLeft.Begin();
            }
            else if (filter.Name == "C2C")
            {
                if (C2CCheck != null)
                {
                    GeneralCheck.Visibility = Visibility.Collapsed;
                    C2CCheck.Visibility = Visibility.Visible;
                    AdditionChek.Visibility = Visibility.Collapsed;
                }
                ContentControl.Navigate(clickToCallPage);
                slideLeft.Begin();
            }
            else if (filter.Name == "Addition")
            {
                if (AdditionChek != null)
                {
                    GeneralCheck.Visibility = Visibility.Collapsed;
                    C2CCheck.Visibility = Visibility.Collapsed;
                    AdditionChek.Visibility = Visibility.Visible;
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
            }
            else
            {
                Menu.Margin = new Thickness(0, 0, 0, -50);
            }
        }
        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            if (!PausesFild.IsVisible)
            {
                PausesFild.Visibility = Visibility.Visible;
                PauseList.Items.Clear();
                getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                getPauses.ResponseData.Pauses = new List<Pause>();
                window!.LoadIcone.Visibility = Visibility.Visible;
                getPauses = await webService.GetPauses(App.AccountData!.Data.Sip_Settings.Sip_username, App.UserPbx!, App.userToken!, App.fw!);
                window.LoadIcone.Visibility = Visibility.Collapsed;
                if (getPauses.ResponseCode == HttpStatusCode.OK)
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
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PauseItem pause = null!; 
                foreach (var item in PauseList.Items)
                {
                    var pauses = (PauseItem)item;
                    if (pauses.Pause.IsChecked == true)
                    {
                        pause = pauses;
                    }
                } 
                   
                if (pause != null)
                {
                    int id = pause.pause.Id;
                    if (getPauses?.ResponseData!.Pause_active != id)
                    {
                        window!.LoadIcone.Visibility = Visibility.Visible;
                        var result = await webService.SetPause(App.AccountData!.Data.Sip_Settings.Sip_username, id, App.UserPbx!, App.userToken!, App.fw);
                        window!.LoadIcone.Visibility = Visibility.Collapsed;
                        if (result.ResponseCode == HttpStatusCode.OK)
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
                        CoreService.activeCall.AddCallToConference(KeypadFild.Text, App.AccountData.Data.Sip_Settings.Sip_server);
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

        private void BackspaceNum_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(KeypadFild.Text))
            {
                KeypadFild.Text = KeypadFild.Text.Substring(0, KeypadFild.Text.Length - 1);
            }
        }
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Если нажата клавиша Backspace
            if (e.Key == Key.Back)
            {
                // Проверяем, фокус не в TextBox или PasswordBox
                if (!(Keyboard.FocusedElement is TextBox) && !(Keyboard.FocusedElement is PasswordBox))
                {
                    e.Handled = true; // Блокируем "назад"
                }
            }
        }

        private void DTMFFild_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                var Num = (TextBox)sender;
                if (CoreService.activeCall != null)
                {
                    if (!String.IsNullOrEmpty(Num.Text))
                    {
                        CoreService.activeCall.dialDtmf(Num.Text.Last().ToString());
                    }
                }
            }
            catch
            {

            }
        }

        private void KeypadFild_TextChanged(object sender, TextChangedEventArgs e)
        {
            var num = (TextBox)sender;
            if (CoreService.activeCall != null)
            {
                if (String.IsNullOrEmpty(num.Text))
                {
                    BackspaceNum.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BackspaceNum.Visibility = Visibility.Visible;
                }
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
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

        private void Scroll_Click(object sender, RoutedEventArgs e)
        {
            var scroll = (Button)sender;
            scroll.Visibility = Visibility.Collapsed;
            if (scroll.Name == "MoreRight")
            {
                MoreLeft.Visibility = Visibility.Visible;
                MenuItems.Margin = new Thickness(MenuItems.Margin.Left + 38, 0, 0, 0);
            }
            else
            {
                MoreRight.Visibility = Visibility.Visible;
                MenuItems.Margin = new Thickness(MenuItems.Margin.Left - 38, 0, 0, 0);
            }
        }
        public async Task OpenBrowser(string phone)
        {
            if (!String.IsNullOrEmpty(App.AccountData?.Data.Custom_Data.url))
            {
                if (!String.IsNullOrEmpty(CoreService.activeCall?.prmMess))
                {
                    var xuid = ParseSipHeader(CoreService.activeCall.prmMess, "X-uniqueid");
                    if (!String.IsNullOrEmpty(xuid))
                    {
                        var castom = ParseSipHeader(CoreService.activeCall.prmMess, "X-CampaignCustom");
                        if (!String.IsNullOrEmpty(castom))
                        {
                            await webService.OpenBrowser(App.AccountData.Data.Custom_Data.url, App.AccountData?.Data.Sip_Settings?.Sip_username!, phone, xuid, castom);
                        }
                    }
                }
            }
        }
        public string? ParseSipHeader(string? rawSip, string headerName)
        {
            if (string.IsNullOrWhiteSpace(rawSip) || string.IsNullOrWhiteSpace(headerName))
                return null;

            // Normalize line endings and unfold folded headers
            var normalized = rawSip.Replace("\r\n", "\n").Replace("\r", "\n");
            normalized = Regex.Replace(normalized, @"\n[ \t]+", " ");

            // Split headers and body (stop at first empty line)
            var parts = normalized.Split(new[] { "\n\n" }, StringSplitOptions.None);
            var headersPart = parts.Length > 0 ? parts[0] : normalized;

            var normalizedToValue = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var originalToValue = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rawLine in headersPart.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(rawLine)) break;
                var idx = rawLine.IndexOf(':');
                if (idx <= 0) continue;

                var name = rawLine.Substring(0, idx).Trim();
                var value = rawLine.Substring(idx + 1).Trim();

                // store by normalized key (remove non-alphanum), and by original name (case-insensitive)
                var normKey = Regex.Replace(name, @"[^A-Za-z0-9]", "").ToLowerInvariant();
                if (normalizedToValue.ContainsKey(normKey))
                    normalizedToValue[normKey] = normalizedToValue[normKey] + ", " + value;
                else
                    normalizedToValue[normKey] = value;

                if (originalToValue.ContainsKey(name))
                    originalToValue[name] = originalToValue[name] + ", " + value;
                else
                    originalToValue[name] = value;
            }

            // Normalize lookup key same way
            var lookup = Regex.Replace(headerName, @"[^A-Za-z0-9]", "").ToLowerInvariant();

            if (normalizedToValue.TryGetValue(lookup, out var found))
                return TrimHeaderValue(found);

            // fallback: try direct case-insensitive match on original header names
            foreach (var kv in originalToValue)
            {
                if (string.Equals(kv.Key, headerName, StringComparison.OrdinalIgnoreCase))
                    return TrimHeaderValue(kv.Value);
            }

            return null;

            static string TrimHeaderValue(string v)
            {
                v = v.Trim();
                // cut at first semicolon or newline if present
                var endIdx = v.IndexOfAny(new char[] { ';', '\r', '\n' });
                if (endIdx > 0) v = v.Substring(0, endIdx).Trim();
                // strip surrounding quotes
                if (v.Length >= 2 && v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                return v;
            }
        }
    }
}
