using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.WindowManagement;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContactsPage : Page
    {
        readonly WebService webService;
        contacts_list contacts;
        public static List<Items.Contact> userContactsList;
        readonly ErrorService errorService;
        public ContactsPage()
        {
            this.InitializeComponent();
            webService = new WebService(App.userToken);
            contacts = new contacts_list
            {
                contacts = new List<Models.Contact>()
            };
            userContactsList = new List<Items.Contact>();
            this.Unloaded += App.RootFrame_Unloaded;
            this.SizeChanged += ContactsPage_SizeChanged;
            errorService = new ErrorService(MainGrid);
        }

        private void ContactsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int color = 1;
            contacts = await webService.GetcontactsList(App.AccountData.Data.Sip_Settings.Sip_username, App.AccountData.Data.User_Data.CompanyID, App.UserPbx);
            if (contacts.responseCode == HttpStatusCode.OK)
            {
                if(contacts.contacts != null)
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
                            userContactsList.Add(new Items.Contact(contact.Name, contact.Telephone, color));
                            color = color == 1 ? 0 : 1;
                        }
                    }
                }

            }
            else
            {
                errorService.ShowError(contacts.responseMessage);
            }
        }
        private void SearchFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            SolidColorBrush magnifyingGlassColorGrey = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
            SolidColorBrush magnifyingGlassColorBlack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            ContactsList.Items.Clear();

            if (String.IsNullOrEmpty(Search.Text))
            {
                var groupContacts = userContactsList.GroupBy(c => c.contactName[0].ToString().ToUpper()).OrderBy(c => c.Key);
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
                magnifyingGlassEllipse.Stroke =  magnifyingGlassColorGrey;
                magnifyingGlassLine.Background = magnifyingGlassColorGrey;
            }
            else
            {
                foreach (var contact in userContactsList)
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
        #region Navigate
        private async void Navigate_Click(object sender, RoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Control":
                    await OpenWindow(typeof(ControlPage)).ConfigureAwait(true);
                    break;
                case "Phone":
                    await OpenWindow(typeof(PhonePage)).ConfigureAwait(true);
                    break;
                case "History":
                    await OpenWindow(typeof(HistoryPage)).ConfigureAwait(true);
                    break;
                case "Fax":
                    await OpenWindow(typeof(FaxPage)).ConfigureAwait(true);
                    break;
                case "HotKeys":
                    await OpenWindow(typeof(HotKeyPage)).ConfigureAwait(true);
                    break;
            }
        }
        private async Task OpenWindow(Type Page)
        {
            if (App.AppWindows.Contains(Page.Name))
            {
                return;
            }
            else
            {
                AppWindow appWindow = await AppWindow.TryCreateAsync();
                Frame OpenPage1 = new Frame
                {
                    Name = Page.Name
                };
                OpenPage1.Navigate(Page);
                ElementCompositionPreview.SetAppWindowContent(appWindow, OpenPage1);
                appWindow.RequestMoveAdjacentToCurrentView();
                appWindow.TitleBar.BackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.InactiveBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonHoverForegroundColor = Colors.DarkGray;
                appWindow.TitleBar.ButtonHoverBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonPressedBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonBackgroundColor = Colors.WhiteSmoke;
                WindowManagementPreview.SetPreferredMinSize(appWindow, App.Size);
                await appWindow.TryShowAsync();
                appWindow.Changed += App.AppWindow_Changed;
                App.AppWindows.Add(Page.Name);
            }
        }


        #endregion

        private void ContactsPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
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

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
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
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            PausesFild.Visibility = Visibility.Collapsed;
        }

        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            if (ControlPage.getPauses == null)
            {
                ControlPage.getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                ControlPage.getPauses.ResponseData.Pauses = new List<Pause>();
                ControlPage.getPauses = await webService.GetPauses(App.AccountData.Data.Sip_Settings.Sip_username, App.UserPbx);
                if (ControlPage.getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ControlPage.getPauses.ResponseData.Pause_active == 0));
                    foreach (var pause in ControlPage.getPauses.ResponseData.Pauses)
                    {
                        PauseList.Items.Add(new PauseItem(pause, pause.Id == ControlPage.getPauses.ResponseData.Pause_active));
                    }
                }
                else
                {
                    errorService.ShowWarning(ControlPage.getPauses.ResponseMessage);
                }
            }
            else
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ControlPage.getPauses.ResponseData.Pause_active == 0));
                foreach (var pause in ControlPage.getPauses.ResponseData.Pauses)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == ControlPage.getPauses.ResponseData.Pause_active));
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
                    if (ControlPage.getPauses.ResponseData.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData.Data.Sip_Settings.Sip_username, id, App.UserPbx);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            ControlPage.getPauses.ResponseData.Pause_active = id;
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
