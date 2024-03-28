using Linphone;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ClientCardPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InfoPage : Page
    {
        public InfoPage()
        {
            this.InitializeComponent();
            ID.Text = ClientCard.clientCard.data.client_id ?? "";
            Phone.Text = ClientCard.clientCard.data.phone1 ?? "";
            if (ClientCard.clientCard.data.created_at.HasValue)
            { 
                DateIssure.Text = ClientCard.clientCard.data.created_at.Value.ToString("mm.dd.yyyy");
            }
            if (ClientCard.clientCard.data.date_of_birth.HasValue)
            {
                Birthday.Text = ClientCard.clientCard.data.date_of_birth.Value.ToString("mm.dd.yyyy");
            }
            Country.Text = ClientCard.clientCard.data.country ?? "";
            this.Adress.Text = ClientCard.clientCard.data.address ?? "";
            Email.Text = ClientCard.clientCard.data.email ?? "";
            Language.Text = ClientCard.clientCard.data.lang ?? "";
            Status.Text = ClientCard.clientCard.data.marital_status ?? "";

        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack(new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
