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
        private DialpadPage phonePage;

        public CallPage()
        {
            this.InitializeComponent();
            phonePage = new DialpadPage();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e?.Parameter != null)
            {
                phonePage = (DialpadPage)e.Parameter;
                //CoreService.Instance.Core.Listener.OnCallStateChanged = phonePage.OnCallStateChanged;
            }
        }
        private void IncomeCallPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(DialpadPage.currentCall.RemoteAddress.Username))
            {
                PhoneText.Text = DialpadPage.currentCall.RemoteAddress.Username;
                UserNameText.Text = String.IsNullOrEmpty(DialpadPage.currentCall.RemoteAddress.DisplayName) ? DialpadPage.currentCall.RemoteAddress.Username : DialpadPage.currentCall.RemoteAddress.DisplayName;
            }
            else
            {
                PhoneText.Text = "No Information";
                UserNameText.Text = "No Information";
            }
        }
        private void EndCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialpadPage.currentCall.Decline(Linphone.Reason.Busy);
                if (CoreService.Instance.Core.Calls.Count() != 0 && CoreService.Instance.Core.Calls.Last() != null)
                {
                    DialpadPage.currentCall = CoreService.Instance.Core.Calls.Last();
                    Frame.GoBack();
                }
                else
                {
                    Frame.Navigate(typeof(DialpadCallPage), phonePage);
                }
            }
            catch
            {

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
            DialpadPage.currentCall?.Accept();
            Frame.Navigate(typeof(ActivCallPage), phonePage);
        }
    }
}
