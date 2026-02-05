using System;
using System.Windows;
using System.Windows.Controls;
using VoiceX.Enums;
using VoiceX.Services;
using VoiceX.Views;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryNote : ListBoxItem
    {
        public string userPhone;
        public StatusCall statusCall;
        public DateTime dateCall;
        public HistoryNote(string Name, string Phone, DateTime dateCall, string Time, StatusCall statusCall)
        {
            this.InitializeComponent();
            this.UserName.Text = Name;
            this.FirstWord.Text = Name.Substring(0, 1);
            this.Time.Text = dateCall.ToString("HH:mm") + "   " + Time;
            userPhone = Phone;
            this.statusCall = statusCall;
            this.dateCall = dateCall;
        }
        private void HistoryNote_Loaded(object sender, RoutedEventArgs e)
        {
            switch (statusCall)
            {
                case StatusCall.Outgoing:
                    OutcomeCall.Visibility = Visibility.Visible;
                    break;
                case StatusCall.Incoming:
                    IncomeCall.Visibility = Visibility.Visible;
                    break;
                case StatusCall.Ignore:
                    MissedCall.Visibility = Visibility.Visible;
                    break;
                case StatusCall.IncomeIgnore:
                    MissedInCall.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            var call = CoreService.Instance.MakeCall(userPhone, App.AccountData!.Data.Sip_Settings.Sip_server);
            if (call == null)
            {
                ProfilePage.window?.ShowError("Call not create. Please check connection and audio.");
            }
        }

        private void UserName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(userPhone))
            {
                Clipboard.SetText(userPhone);
            }
        }

        private void Sms_Click(object sender, RoutedEventArgs e)
        {
            ProfilePage.window?.ShowSmsBlock();
        }
    }

}
