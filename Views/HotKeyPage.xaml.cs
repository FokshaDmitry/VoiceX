using VoiceX.DAL.Context;
using VoiceX.Services;
using VoiceX.DAL.Entity;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using VoiceX.Items;
using System.Windows.Media;

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
        List<HotKeyItem> hotKeyItems;
        public HotKeyPage()
        {
            this.InitializeComponent();
            addDbContext = new AddDbContext();
            this.Loaded += HotKeyPage_Loaded;
            webService = new WebService();
            hotKeyItems = new List<HotKeyItem>();
        }

        private void HotKeyPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var hotKeyUsers = addDbContext.GetHotKeyUsers();
                if (hotKeyUsers.Count != 0)
                {
                    foreach (var user in hotKeyUsers)
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

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
                    ProfilePage.window?.ShowError("Phone contains invalid characters");
                    AddHotkeyFild.Visibility = Visibility.Collapsed;
                    UserName.Text = "";
                    UserPhone.Text = "";
                    return;
                }
                if (hotKeyItems.Select(hk => hk.HotKeyPhone).Contains(phone))
                {
                    ProfilePage.window?.ShowError("This phone has already been added");
                    AddHotkeyFild.Visibility = Visibility.Collapsed;
                    UserName.Text = "";
                    UserPhone.Text = "";
                    return;
                }
                if (hotKeyItems.Select(hk => hk.HotKeyName).Contains(name))
                {
                    ProfilePage.window?.ShowError("This name already exists");
                    AddHotkeyFild.Visibility = Visibility.Collapsed;
                    UserName.Text = "";
                    UserPhone.Text = "";
                    return;
                }
                try
                {
                    var id = Guid.NewGuid();
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
                ProfilePage.window?.ShowError("Fild is Empty");
                AddHotkeyFild.Visibility = Visibility.Collapsed;
                UserName.Text = "";
                UserPhone.Text = "";
                return;
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddHotkeyFild.Visibility = Visibility.Collapsed;
        }
    }
}
