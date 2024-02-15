using System;
using System.Threading.Tasks;
using VoiceX.Services;
using VoiceX.Views.ClientCardPages;
using VoiceX.Views.PhonePages;
using Windows.UI;
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
    public sealed partial class Contact : ListBoxItem
    {
        public string contactName;
        public string contactPhone;
        public Contact(string Name, string Phone, int color)
        {
            this.InitializeComponent();
            contactName = Name;
            contactPhone = Phone;
            FirstWord.Text = Name.Substring(0, 1);
            this.UserName.Text = Name;
            this.Phone.Text = Phone;
            contactBackgroundColor.Background = color == 1 ?  new SolidColorBrush(Color.FromArgb(255, 138, 99, 251)) : new SolidColorBrush(Color.FromArgb(255, 229, 167, 224));
        }

        private void ListBoxItem_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
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

        private async void Call_Click(object sender, RoutedEventArgs e)
        {
            if (!App.AppWindows.Contains("PhonePage"))
            {
                await StartPage(typeof(PhonePage), Phone.Text);
            }
            else
            {
                CoreService.Instance.Call(Phone.Text);
            }
        }

        private async void Info_Click(object sender, RoutedEventArgs e)
        {
            await StartPage(typeof(ClientCard), Phone.Text + ";" + UserName.Text);
        }
        private async Task StartPage(Type NavigatePage, string Params)
        {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            Frame OpenPage1 = new Frame();
            OpenPage1.Navigate(NavigatePage, Params);
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
    }
}
