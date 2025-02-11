using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.Items;
using VoiceX.Models;
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
        public ErrorService errorService;
        OperatorsPage operatorsPage;
        public ClientsPage()
        {
            this.InitializeComponent();
            webService = new WebService();
            userContactsList = new List<Items.Contact>();
            errorService = new ErrorService(MainGrid);
            operatorsPage = new OperatorsPage();
        }
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 253, 254, 255));

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
            }
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

        private void contactsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Clients.Checked += Filter_Checked;
            Operators.Checked += Filter_Checked;
        }
    }
}
