using System;
using VoiceX.Enums;
using VoiceX.Services;
using VoiceX.Views.PhonePages;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

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
            if (!App.AppWindows.Contains("PhonePage"))
            {
                AppWindow appWindow = await AppWindow.TryCreateAsync();
                Frame OpenPage1 = new Frame();
                OpenPage1.Navigate(typeof(PhonePage), userPhone);
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
            }
            else
            {
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
