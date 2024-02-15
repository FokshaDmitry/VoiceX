using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HeadingContactList : ListBoxItem
    {
        public HeadingContactList(string heanding)
        {
            this.InitializeComponent();
            Heanding.Text = heanding;
        }
    }
}
