using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.Interfeces;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoreItems : ListBoxItem
    {
        IMoreItems moreItems;
        public MoreItems(IMoreItems page)
        {
            this.InitializeComponent();
            this.moreItems = page; 
        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            moreItems.AddMoreNotes();
        }

        private void More_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 142, 142, 142));
            AngleColor.Stroke = new SolidColorBrush(Color.FromArgb(255, 142, 142, 142));
        }

        private void More_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextColor.Foreground = new SolidColorBrush(Color.FromArgb(255, 135, 97, 246));
            AngleColor.Stroke = new SolidColorBrush(Color.FromArgb(255, 135, 97, 246));
        }
    }
}
