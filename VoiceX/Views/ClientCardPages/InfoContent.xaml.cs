using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using VoiceX.Services;
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
            if (!App.appWindows.Select(a => a.Title).Contains("Dialpad"))
            {
                await App.OpenWindow(typeof(DialpadPage), phoneNumber.Text);
            }
            else
            {
                try
                {
                    await CoreService.Instance.OpenMicrophonePopup();
                }
                catch { return; }
                var phone = phoneNumber.Text;
                foreach (var regex in ProfilePage.regexNotes.Where(r => r.Check))
                {
                    phone = phone.Replace(regex.Search, regex.Replace);
                }
                CoreService.Instance.Call(phone);
            }
        }
        private void Info_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, 1, Img.Margin.Right, 0);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
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
