using System;
using System.Threading.Tasks;
using VoiceX.Services;
using VoiceX.Views.PhonePages;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ClientCardPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InfoContent : Page
    {
        Frame frame;
        public InfoContent()
        {
            this.InitializeComponent();
            AccountFild.Text = ClientCard.UserName;
            phoneNumber.Text = ClientCard.Number;
            frame = new Frame();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            if (e.Parameter is Frame frame)
            {
                this.frame = frame;
            }
        }
        private void Info_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, 0, Img.Margin.Right, 0);
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
        private async void Call_Click(object sender, RoutedEventArgs e)
        {
            if (!App.AppWindows.Contains("PhonePage"))
            {
                await StartPage(typeof(PhonePage), phoneNumber.Text);
            }
            else
            {
                CoreService.Instance.Call(phoneNumber.Text);
            }
        }
        private void Info_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, 1, Img.Margin.Right, 0);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private async Task StartPage(Type Page, string Params)
        {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            Frame OpenPage1 = new Frame();
            OpenPage1.Navigate(Page, Params);
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

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Tasks":
                    frame.Navigate(typeof(TasksPage), frame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    break;
                case "History":
                    frame.Navigate(typeof(HistoryPage), frame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    break;
                case "Family":
                    frame.Navigate(typeof(FamilyPage), frame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    break;
                case "Events":
                    frame.Navigate(typeof(EventsPage), frame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    break;
                case "Info":
                    frame.Navigate(typeof(InfoPage), frame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    break;
                case "Docs":
                    frame.Navigate(typeof(DocksPage), frame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    break;
                default:
                    break;
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
        private void Grid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
    }
}
