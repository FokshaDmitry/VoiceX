using System;
using System.Linq;
using VoiceX.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CallPage : Page
    {
        private PhonePage phonePage;

        public CallPage()
        {
            this.InitializeComponent();
            phonePage = new PhonePage();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e?.Parameter != null)
            {
                phonePage = (PhonePage)e.Parameter;
                CoreService.Instance.Core.Listener.OnCallStateChanged = phonePage.OnCallStateChanged;
            }
        }
        private void IncomeCallPage_Loaded(object sender, RoutedEventArgs e)
        {
            PhoneText.Text = PhonePage.currentCall.RemoteAddress.Username;
            UserNameText.Text = String.IsNullOrEmpty(PhonePage.currentCall.RemoteAddress.DisplayName) ? PhonePage.currentCall.RemoteAddress.Username : PhonePage.currentCall.RemoteAddress.DisplayName;
        }
        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            PhonePage.currentCall?.Terminate();
            if (CoreService.Instance.Core.Calls.Count() != 0 && CoreService.Instance.Core.Calls.Last() != null)
            {
                PhonePage.currentCall = CoreService.Instance.Core.Calls.Last();
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(DialpadCallPage), phonePage);
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
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            PhonePage.currentCall?.Accept();
            Frame.Navigate(typeof(ActivCallPage), phonePage);
        }
    }
}
