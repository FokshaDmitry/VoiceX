using System.Windows;
using System.Windows.Controls;
using VoiceX.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PauseItem : ListBoxItem
    {
        public bool IsSelect;
        public Pause pause;
        public PauseItem(Pause pause, bool select)
        {
            this.InitializeComponent();
            this.pause = pause;
            Pause.Content = pause.Name;
            Pause.IsChecked = select;
            IsSelect = select;
        }

        public void SelectChange(bool select)
        {
            IsSelect = select;
        }
    }
}
