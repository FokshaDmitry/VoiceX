using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ClientPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClientsPage : Page
    {
        readonly WebService webService;
        public static List<Items.Contact> userContactsList;
        OperatorsPage operatorsPage;
        ContactsPage contactsPages;
        public ClientsPage()
        {
            this.InitializeComponent();
            webService = new WebService();
            userContactsList = new List<Items.Contact>();
            operatorsPage = new OperatorsPage();
            contactsPages = new ContactsPage();
        }
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 193, 191, 255));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 240, 240, 255));

            if (filter.Name == "Operators")
            {
                OperatorsCheck.Background = blueLine;
                ClientsChek.Background = whiteLine;
                PageContent.Navigate(operatorsPage);
            }
            else if (filter.Name == "Clients")
            {
                OperatorsCheck.Background = whiteLine;
                ClientsChek.Background = blueLine;
                PageContent.Navigate(contactsPages);
            }
        }

        private void contactsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Clients.IsChecked = false;
            Operators.IsChecked = true;
            Clients.Checked += Filter_Checked;
            Operators.Checked += Filter_Checked;
            PageContent.Navigate(operatorsPage);
        }
    }
}
