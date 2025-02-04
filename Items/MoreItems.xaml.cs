using VoiceX.Interfeces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoreItems : Page
    {
        IMoreItems moreItems;
        public MoreItems(IMoreItems page)
        {
            this.InitializeComponent();
            this.moreItems = page; 
        }

        private void More_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            TextColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 142, 142, 142));
            AngleColor.Stroke = new SolidColorBrush(Color.FromArgb(255, 142, 142, 142));
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void More_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            TextColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 135, 97, 246));
            AngleColor.Stroke = new SolidColorBrush(Color.FromArgb(255, 135, 97, 246));
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            moreItems.AddMoreNotes();
        }
    }
}
