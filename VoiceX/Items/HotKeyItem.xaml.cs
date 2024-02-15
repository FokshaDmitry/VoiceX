using System;
using VoiceX.DAL.Context;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HotKeyItem : ListBoxItem
    {
        AddDbContext addDbContext;
        public string HotKeyName;
        public string HotKeyPhone;
        private ListBox hotkeyList;
        Guid hotKeyGuid;
        public HotKeyItem(Guid hotKeyGuid, string Name, string Phone, ListBox hotkeyList)
        {
            this.InitializeComponent();
            this.Phone.Text = Phone;
            this.UserName.Text = Name;
            this.FirstWord.Text = Name.ToUpper().Substring(0, 1);
            HotKeyName = Name;
            HotKeyPhone = Phone;
            this.hotkeyList = hotkeyList;
            this.hotKeyGuid = hotKeyGuid;
            addDbContext = new AddDbContext();
        }
        public void SetState(bool state)
        {
            this.Activ.Background = state ? new SolidColorBrush(Color.FromArgb(255, 76, 176, 78)) : new SolidColorBrush(Color.FromArgb(255, 243, 30, 56));
        }
        private async void Trash_Click(object sender, RoutedEventArgs e)
        {
            hotkeyList.Items.Remove(this);
            await addDbContext.RemoveHotKeyUserAsync(hotKeyGuid);
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
        private void ListBoxItem_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
    }
}
