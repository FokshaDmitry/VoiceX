using System;
using System.Diagnostics;
using System.Linq;
using VoiceX.Interfeces;
using VoiceX.Items;
using VoiceX.Services;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ClientPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContactsPage : Page, IMoreItems
    {
        WebService webService;
        public ContactsPage()
        {
            this.InitializeComponent();
            webService = new WebService(App.userToken);
            this.Loaded += ContactsPage_Loaded;
        }

        private void ContactsPage_Loaded(object sender, RoutedEventArgs e)
        {
            AddMoreNotes();
        }

        private void SearchFild_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            SolidColorBrush magnifyingGlassColorGrey = new SolidColorBrush(Color.FromArgb(255, 137, 137, 137));
            SolidColorBrush magnifyingGlassColorBlack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            if (String.IsNullOrEmpty(Search.Text))
            {
                magnifyingGlass.Margin = new Thickness(0, 0, 5, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorGrey;
                magnifyingGlassLine.Background = magnifyingGlassColorGrey;
            }
            else
            {
                magnifyingGlass.Margin = new Thickness(0, 0, 23, 2);
                magnifyingGlassEllipse.Stroke = magnifyingGlassColorBlack;
                magnifyingGlassLine.Background = magnifyingGlassColorBlack;
                ContactsList.Items.Clear();
                AddMoreNotes();
            }
        }

        private async void magnifyingGlass_Click(object sender, RoutedEventArgs e)
        {
            ContactsList.Items.Clear();
            var search = Search.Text;
            if (!String.IsNullOrEmpty(search))
            {
                if (search.Length >= 3)
                {
                    var clients = await webService.SearchClient(search, App.UserPbx);
                    if (clients.data.Count != 0)
                    {
                        var groupContacts = clients.data.GroupBy(c => c.username[0].ToString().ToUpper()).OrderBy(c => c.Key);
                        foreach (var groupcontact in groupContacts.ToList())
                        {
                            if (groupcontact.Key.All(char.IsDigit))
                                ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString()));
                            else
                                ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                            int color = 0;
                            foreach (var client in groupcontact)
                            {
                                ContactsList.Items.Add(new ClientItem(client.username, client.phone1, client.email, client.db_id, color));
                                color = color == 0 ? 1 : 0;
                            }
                        }
                    }
                }
            }
        }
        private void ContactsPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
        private void Cursor_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }

        private void Cursor_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
        public async void AddMoreNotes()
        {
            int count = ContactsList.Items.Count;
            Debug.WriteLine(count);
            count += 25;
            ContactsList.Items.Clear();
            ContactsList.Items.Add(new MoreItems(this));
            var clients = await webService.SearchClient(count, App.UserPbx);
            
            if (clients.data != null && clients.data.Count != 0)
            {
                var groupContacts = clients.data.GroupBy(c => c.username[0].ToString().ToUpper()).OrderBy(c => c.Key);
                foreach (var groupcontact in groupContacts.ToList())
                {
                    if (groupcontact.Key.All(char.IsDigit))
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString()));
                    else
                        ContactsList.Items.Add(new HeadingContactList(groupcontact.Key.ToString() + groupcontact.Key.ToString().ToUpper().ToLower()));
                    int color = 0;
                    foreach (var client in groupcontact)
                    {
                        ContactsList.Items.Add(new ClientItem(client.username, client.phone1, client.email, client.db_id, color));
                        color = color == 0 ? 1 : 0;
                    }
                }
            }

        }
        private void Search_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                magnifyingGlass_Click(sender, e);
            }
        }
    }
}
