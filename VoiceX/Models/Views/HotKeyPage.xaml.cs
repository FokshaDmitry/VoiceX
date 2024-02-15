using Linphone;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceX.DAL.Context;
using VoiceX.Services;
using VoiceX.Views.PhonePages;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.WindowManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Views.ControlPages;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml.Media;
using VoiceX.DAL.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.System;
using System.Diagnostics;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HotKeyPage : Page
    {
        AddDbContext addDbContext;
        readonly WebService webService;
        readonly ErrorService errorService;
        List<HotKeyItem> hotKeyItems;
        public HotKeyPage()
        {
            this.InitializeComponent();
            addDbContext = new AddDbContext();
            this.Loaded += HotKeyPage_Loaded;
            this.Unloaded += HotKeyPage_Unloaded;
            errorService = new ErrorService(HotKeyMainGrid);
            webService = new WebService(App.userToken);
            hotKeyItems = new List<HotKeyItem>();
        }

        private void HotKeyPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.AppWindows.Remove(sender.GetType().Name);
        }

        private void HotKeyPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var hotKeyUsers = addDbContext.GetHotKeyUsers();
                if (hotKeyUsers.Count != 0)
                {
                    var freandList = CoreService.Instance.Core.CreateFriendList();
                    foreach (var user in hotKeyUsers)
                    {
                        var freand = CoreService.Instance.Core.CreateFriend();
                        freand.Name = user.Name;
                        var domain = $"pbx{App.UserPbx}.x-cloud.info";
                        freand.Address = CoreService.Instance.Core.CreateAddress($"sip:{user.Phone}@{domain}");
                        freand.CreateVcard(user.Name);
                        freandList.AddFriend(freand);
                        var HKItem = new HotKeyItem(user.Id, user.Name, user.Phone, ContactsList);
                        hotKeyItems.Add(HKItem);
                        ContactsList.Items.Add(HKItem);
                    }
                    CoreService.Instance.Core.AddFriendList(freandList);
                    CoreService.Instance.Core.Listener.OnNotifyPresenceReceived = OnNotifyPresenceReceived;
                }
            }
            catch (Exception ex)
            {
                errorService.ShowWarning(ex.Message);
            }

        }
        private void SearchFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            SolidColorBrush magnifyingGlassColorGrey = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
            SolidColorBrush magnifyingGlassColorBlack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            ContactsList.Items.Clear();

            if (String.IsNullOrEmpty(Search.Text))
            {
                foreach (var contact in hotKeyItems)
                {
                    ContactsList.Items.Add(contact);
                }
                magnifyingGlass.Margin = new Thickness(0, 0, 5, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorGrey;
                magnifyingGlassLine.Background = magnifyingGlassColorGrey;
            }
            else
            {
                foreach (var contact in hotKeyItems)
                {
                    if (contact.HotKeyName.Contains(Search.Text))
                    {
                        ContactsList.Items.Add(contact);
                    }
                    else if (contact.HotKeyPhone.Contains(Search.Text))
                    {
                        ContactsList.Items.Add(contact);
                    }
                }
                magnifyingGlass.Margin = new Thickness(0, 0, 23, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorBlack;
                magnifyingGlassLine.Background = magnifyingGlassColorBlack;
            }
        }
        private void OnNotifyPresenceReceived(Core core, Friend friend)
        {
            if(!String.IsNullOrEmpty(friend?.Address?.Username))
            {
                try
                {
                    var presenceModel = friend?.PresenceModel;

                    var presenceStatus = presenceModel?.BasicStatus;
                    if (presenceStatus == PresenceBasicStatus.Open)
                    {
                        hotKeyItems.Where(hk => hk.HotKeyPhone == friend.Address.Username).DefaultIfEmpty().First().SetState(true);
                    }
                    else
                    {
                        hotKeyItems.Where(hk => hk.HotKeyPhone == friend.Address.Username).DefaultIfEmpty().First().SetState(false);
                    }
                }
                catch (Exception ex)
                {
                    errorService.ShowWarning(ex.Message);
                }
            }
        }
        #region Navigete Button
        private async void Navigate_Click(object sender, RoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Contacts":
                    await OpenWindow(typeof(ContactsPage)).ConfigureAwait(true);
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
                case "Control":
                    await OpenWindow(typeof(ControlPages.ControlPage)).ConfigureAwait(true);
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
                App.AppWindows.Add(Page.Name);
            }
        }
        #endregion
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
        private void PauseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            foreach (var item in list.Items)
            {
                var pause = (PauseItem)item;
                pause.SelectChange(pause == list.SelectedItem);
            }

        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            PausesFild.Visibility = Visibility.Collapsed;
            AddHotkeyFild.Visibility=Visibility.Collapsed;
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
            App.timeOut = DateTime.Now;
        }

        private void AddHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            AddHotkeyFild.Visibility = Visibility.Visible;
        }

        private async void SaveHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(UserName.Text) && !String.IsNullOrEmpty(UserPhone.Text))
            {
                string phone = UserPhone.Text;
                string name = UserName.Text;
                if (Regex.IsMatch(phone, "[^0-9]"))
                {
                    errorService.ShowWarning("Phone contains invalid characters");
                    AddHotkeyFild.Visibility = Visibility.Collapsed;
                    UserName.Text = "";
                    UserPhone.Text = "";
                    return;
                }
                if (hotKeyItems.Select(hk => hk.HotKeyPhone).Contains(phone))
                {
                    errorService.ShowWarning("This phone has already been added");
                    AddHotkeyFild.Visibility = Visibility.Collapsed;
                    UserName.Text = "";
                    UserPhone.Text = "";
                    return;
                }
                if (hotKeyItems.Select(hk => hk.HotKeyName).Contains(name))
                {
                    errorService.ShowWarning("This name already exists");
                    AddHotkeyFild.Visibility = Visibility.Collapsed;
                    UserName.Text = "";
                    UserPhone.Text = "";
                    return;
                }
                try
                {
                    var id = Guid.NewGuid();
                    var freandList = CoreService.Instance.Core.CreateFriendList();
                    var freand = CoreService.Instance.Core.CreateFriend();
                    freand.Name = name;
                    var domain = $"pbx{App.UserPbx.TrimStart('0')}.x-cloud.info";
                    freand.Address = CoreService.Instance.Core.CreateAddress($"sip:{phone}@{domain}");
                    freand.CreateVcard(name);
                    freandList.AddFriend(freand);
                    CoreService.Instance.Core.AddFriendList(freandList);
                    CoreService.Instance.Core.Listener.OnNotifyPresenceReceived = OnNotifyPresenceReceived;
                    await addDbContext.AddHotKeyUserAsync(new HotKeyUser() { Id = id, Name = name, Phone = phone });
                    var HKItem = new HotKeyItem(id, name, phone, ContactsList);
                    hotKeyItems.Add(HKItem);
                    ContactsList.Items.Add(HKItem);
                }
                catch { }
                AddHotkeyFild.Visibility = Visibility.Collapsed;
                UserName.Text = "";
                UserPhone.Text = "";
            }
            else
            {
                errorService.ShowWarning("Fild is Empty");
                AddHotkeyFild.Visibility = Visibility.Collapsed;
                UserName.Text = "";
                UserPhone.Text = "";
                return;
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
    }
}
