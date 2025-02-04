using System;
using System.Linq;
using System.Numerics;
using VoiceX.Enums;
using VoiceX.Services;
using VoiceX.Views;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

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
        HistoryPage historyPage;
        public HistoryNote(string Name, string Phone, DateTime dateCall, string Time, StatusCall statusCall)
        {
            this.InitializeComponent();
            this.UserName.Text = Name;
            this.FirstWord.Text = Name.Substring(0, 1);
            this.Time.Text = dateCall.ToString("HH:mm") + "   " + Time;
            userPhone = Phone;
            this.statusCall = statusCall;
            this.dateCall = dateCall;
            this.Loaded += HistoryNote_Loaded;
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
                default:
                    break;
            }
        }

        private async void Call_Click(object sender, RoutedEventArgs e)
        {
            if (!App.appWindows.Select(a => a.Title).Contains("Dialpad"))
            {
                await App.OpenWindow(typeof(DialpadPage), userPhone);
            }
            else
            {
                try
                {
                    await CoreService.Instance.OpenMicrophonePopup();
                }catch { return; }
                foreach (var regex in ProfilePage.regexNotes.Where(r => r.Check))
                {
                    userPhone = userPhone.Replace(regex.Search, regex.Replace);
                }
                CoreService.Instance.Call(userPhone);
            }
        }

        private void Info_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, 5, Img.Margin.Right, 0);
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void Info_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, 6, Img.Margin.Right, 0);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }

        private void ListBoxItem_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

    }

}
