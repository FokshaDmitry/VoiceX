using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceX.DAL.Context;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net;
using Linphone;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Text;
using VoiceX.DAL.Entity;
using VoiceX.Enums;
using VoiceX.Views.ControlPages;
using VoiceX.Views.ClientPages;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DialpadPage : Page
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
            webService = new WebService(App.userToken);
            contacts = new contacts_list
            {
                contacts = new List<Models.Contact>()
            };
            CallAdtess = new List<string>();
            this.SizeChanged += PhonePage_SizeChanged;
            if(AutoAnswerNumbers == null) AutoAnswerNumbers = new List<string>();
            errorService = new ErrorService(MainGrid);
            this.Unloaded += PhonePage_Unloaded;
        }
        private void PhonePage_Unloaded(object sender, RoutedEventArgs e)
        {
            TerminateAllCalls = true;
            CoreService.Instance.Core.TerminateAllCalls();
        }

        public async void OnCallStateChanged(Core core, Call call, CallState state, string message)
        {
            var rootFrame = (Frame)Window.Current.Content;
            switch (state)
            {
                case CallState.Paused:
                    currentCall = call;
                    break;
                case CallState.Pausing:
                    currentCall = call;
                    break;
                case CallState.Resuming:
                    currentCall = call;
                    break;
                case CallState.IncomingReceived:
                    if (currentCall == null)
                    {
                        currentCall = call;
                        StatusCall = StatusCall.Incoming;
                        StartCall = DateTime.Now;
                        if (AutoAnswerNumbers.Contains(currentCall.RemoteAddress.Username))
                        {
                            currentCall.Accept();
                            if (!App.appWindows.Select(s => s.Title).Contains("Dialpad"))
                            {
                                 Frame.Navigate(typeof(DialpadPage), "Activ", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                            }
                            else
                            {
                                PhonePageContent.Navigate(typeof(ActivCallPage), this);
                            }
                            break;
                        }
                        if (!App.appWindows.Select(s => s.Title).Contains("Dialpad"))
                        {
                            Frame.Navigate(typeof(DialpadPage), "Call", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                        }
                        else
                        {
                            PhonePageContent.Navigate(typeof(CallPage), this);
                        }
                    }
                    else if (call.RemoteAddress.Username != currentCall.RemoteAddress.Username)
                    {
                        if (CallAdtess.Count == 0 && currentCall.State != CallState.IncomingReceived)
                        {
                            currentCall = call;
                            PhonePageContent.Navigate(typeof(CallPage), this);
                        }
                        else
                        {
                            call.Decline(Reason.Busy);
                        }
                    }
                    break;
                case CallState.Connected:
                    Ignore = false;
                    if (currentCall == null)
                    {
                        currentCall = call;
                    }
                    if (this.PhonePageContent.Content?.ToString() != "VoiceX.Views.PhonePages.ActivCallPage")
                    {
                        PhonePageContent.Navigate(typeof(ActivCallPage), this);
                    }
                    break;
                case CallState.OutgoingInit:
                    if (currentCall == null)
                    {
                        currentCall = call;
                        StatusCall = StatusCall.Outgoing;
                        StartCall = DateTime.Now;
                    }
                    if (this.PhonePageContent.Content?.ToString() != "VoiceX.Views.PhonePages.ActivCallPage")
                    {
                        PhonePageContent.Navigate(typeof(ActivCallPage), this);
                    }


                    break;
                case CallState.End:
                    // if event start twice
                    if (currentCall == null)
                    {
                        break;
                    }
                    // if we have conference     //true if user and all calls
                    if (CallAdtess.Count != 0 && TerminateAllCalls)
                    {
                        if (Ignore)
                        {
                            StatusCall = StatusCall.Ignore;
                        }
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (var callad in CallAdtess)
                        {
                            stringBuilder.Append($"{callad}, ");
                        }
                        await addDbContext.AddNoteAcync(new HistoryNotes { Id = Guid.NewGuid(), Name = stringBuilder.ToString().TrimEnd(' ', ','), Phone = stringBuilder.ToString().TrimEnd(' ', ','), StartDialog = StartCall, EndDialog = DateTime.Now, StatusCall = StatusCall });
                        currentCall = null;
                        CallAdtess.Clear();
                        TerminateAllCalls = false;
                    }
                    // if one user from conference end call, we need to save other user in contact
                    else if (CallAdtess.Count != 0)
                    {
                        try
                        {
                            if (CallAdtess.Contains(call?.RemoteAddress.Username))
                            {
                                CallAdtess.Remove(call?.RemoteAddress.Username);
                            }
                            // if last user end the dialog
                            if (CallAdtess.Count == 0)
                            {
                                if (call.RemoteAddress.DisplayName != null)
                                {
                                    await addDbContext.AddNoteAcync(new HistoryNotes { Id = Guid.NewGuid(), Name = call.RemoteAddress.DisplayName, Phone = call.RemoteAddress.Username, StartDialog = StartCall, EndDialog = DateTime.Now, StatusCall = StatusCall });
                                }
                                else
                                {
                                    await addDbContext.AddNoteAcync(new HistoryNotes { Id = Guid.NewGuid(), Name = call.RemoteAddress.Username, Phone = call.RemoteAddress.Username, StartDialog = StartCall, EndDialog = DateTime.Now, StatusCall = StatusCall });
                                }
                                CoreService.Instance.Core.TerminateAllCalls();
                                TerminateAllCalls = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                    else if (CoreService.Instance.Core.Calls.Count() != 0 && CoreService.Instance.Core.Calls.Last() != null)
                    {
                        if (CoreService.Instance.Core.Calls.Last().RemoteAddress.Username != currentCall.RemoteAddress.Username)
                        {
                            currentCall = CoreService.Instance.Core.Calls.Last();
                            try
                            {
                                currentCall.Resume();
                            }
                            catch { }
                        }
                        break;
                    }
                    else if (currentCall.RemoteAddress == call.RemoteAddress)
                    {
                        if (Ignore)
                        {
                            StatusCall = StatusCall.Ignore;
                        }
                        if (currentCall.RemoteAddress.DisplayName != null)
                        {
                            await addDbContext.AddNoteAcync(new HistoryNotes { Id = Guid.NewGuid(), Name = currentCall.RemoteAddress.DisplayName, Phone = currentCall.RemoteAddress.Username, StartDialog = StartCall, EndDialog = DateTime.Now, StatusCall = StatusCall });
                            currentCall = null;
                        }
                        else
                        {
                            await addDbContext.AddNoteAcync(new HistoryNotes { Id = Guid.NewGuid(), Name = currentCall.RemoteAddress.Username, Phone = currentCall.RemoteAddress.Username, StartDialog = StartCall, EndDialog = DateTime.Now, StatusCall = StatusCall });
                            currentCall = null;
                        }
                    }

                    Ignore = true;
                    SelectContacts.Clear();
                    KeypadFild.Text = "";
                    DTMFFild.Text = "";
                    NumpadFild.Visibility = Visibility.Collapsed;
                    KeyPad.Visibility = Visibility.Collapsed;
                    PausesFild.Visibility = Visibility.Collapsed;
                    if (this.PhonePageContent.Content?.ToString() != "VoiceX.Views.PhonePages.DialpadCallPage")
                    {
                        PhonePageContent.Navigate(typeof(DialpadCallPage), this);
                    }
                    var type = CoreService.Instance.Core.DefaultAccount.Transport;
                    CoreService.Instance.LogOut();
                    CoreService.Instance.LogIn(App.AccountData.Data.Sip_Settings.Sip_username.ToString(), App.AccountData.Data.Sip_Settings.Sip_secret, App.AccountData.Data.Sip_Settings.Sip_server, App.AccountData.Data.Sip_Settings.Sip_proxy, type);
                    break;
                case CallState.Error:
                    // send message from user notification
                    try
                    {
                        Ignore = true;
                        var builder = new ToastContentBuilder()
                        .AddText("Error Call To VoiceX", hintMaxLines: 2)
                        .AddText(currentCall.RemoteAddress.Username, hintMaxLines: 1)
                        .AddText(message);
                        builder.Show();
                        if (CallAdtess.Count == 0)
                        {
                            currentCall = null;
                        }
                        else
                        {
                            if (CallAdtess.Contains(call.RemoteAddress.Username))
                            {
                                CallAdtess.Remove(call.RemoteAddress.Username);   
                            }
                            break;
                        }
                        SelectContacts.Clear();
                        KeypadFild.Text = "";
                        DTMFFild.Text = "";
                        NumpadFild.Visibility = Visibility.Collapsed;
                        KeyPad.Visibility = Visibility.Collapsed;
                        PausesFild.Visibility = Visibility.Collapsed;
                        if (this.PhonePageContent.Content?.ToString() != "VoiceX.Views.PhonePages.DialpadCallPage")
                        {
                            PhonePageContent.Navigate(typeof(DialpadCallPage), this);
                        }
                    }
                    catch
                    {

                    }
                    var typet = CoreService.Instance.Core.DefaultAccount.Transport;
                    CoreService.Instance.LogOut();
                    CoreService.Instance.LogIn(App.AccountData.Data.Sip_Settings.Sip_username.ToString(), App.AccountData.Data.Sip_Settings.Sip_secret, App.AccountData.Data.Sip_Settings.Sip_server, App.AccountData.Data.Sip_Settings.Sip_proxy, typet);
                    break;
            }
        }
        private void PhonePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!String.IsNullOrEmpty((string)e?.Parameter))
            {
                var param = (string)e.Parameter;
                if (param == "Call")
                {
                    PhonePageContent.Navigate(typeof(CallPage), this);
                }
                else if (param == "Activ")
                {
                    PhonePageContent.Navigate(typeof(ActivCallPage), this);
                }
                else
                {
                    try
                    {
                        await CoreService.Instance.OpenMicrophonePopup();
                    }
                    catch
                    {
                        errorService.ShowError("Microphone not found");
                        CoreService.Instance.Core.Listener.OnCallStateChanged = OnCallStateChanged;
                        return;
                    }
                    foreach (var regex in ProfilePage.regexNotes.Where(r => r.Check))
                    {
                        param = param.Replace(regex.Search, regex.Replace);
                    }
                    CoreService.Instance.Call($"{param}");
                    PhonePageContent.Navigate(typeof(ActivCallPage), this);
                }

            }
            else
            {
                PhonePageContent.Navigate(typeof(DialpadCallPage), this);
            }
            CoreService.Instance.Core.Listener.OnCallStateChanged = OnCallStateChanged;
        }
        #region Navigate
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
        private void Navigate_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top - 1, 0, 0);
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void Navigate_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top + 1, 0, 0);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private async void Navigate_Click(object sender, RoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Control":
                    Frame.Navigate(typeof(ProfilePage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "Contacts":
                    await App.OpenWindow(typeof(ClientsPage), "");
                    break;
                case "History":
                    Frame.Navigate(typeof(HistoryPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "Fax":
                    Frame.Navigate(typeof(FaxPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "HotKeys":
                    Frame.Navigate(typeof(HotKeyPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
            }
        }
        #endregion
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
                GeneralCheck.Background = blueLine;
                AdditionChek.Background = whiteLine;
                ContactListPad.Visibility = Visibility.Collapsed;
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
        private void SearchFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
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
                magnifyingGlassLine.Background = magnifyingGlassColorGrey;
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
                magnifyingGlassLine.Background = magnifyingGlassColorBlack;
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
                            CallAdtess.Add(currentCall.RemoteAddress.Username);
                        }
                        if (!CallAdtess.Contains(contact))
                        {
                            CallAdtess.Add(contact);
                        }
                    }
                    CoreService.Instance.CreateConference(CallAdtess.Select(c => c).ToArray());
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
                        currentCall.Transfer($"{KeypadFild.Text}");
                        NumpadFild.Visibility = Visibility.Collapsed;
                        return;
                    }
                    else
                    {
                        if (CallAdtess.Count == 0)
                        {

                            CallAdtess.Add(currentCall.RemoteAddress.Username);
                        }
                        if (!CallAdtess.Contains(KeypadFild.Text))
                        {
                            CallAdtess.Add(KeypadFild.Text);
                            CoreService.Instance.CreateConference(CallAdtess.Select(c => c).ToArray());
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
                currentCall.SendDtmfs(button.Content.ToString());
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

        private void DTMFFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
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
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }

        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
        private void KeypadFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
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