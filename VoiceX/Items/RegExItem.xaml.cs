using System;
using VoiceX.Views.ControlPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegExItem : ListBoxItem
    {
        readonly ClickToCallPage clickToCallPage;
        bool flag;
        public string SearchText;
        public string ReplaceText;
        public bool Check;
        public RegExItem(ClickToCallPage clickToCallPage)
        {
            this.InitializeComponent();
            this.clickToCallPage = clickToCallPage;
            RegItem.Visibility = Visibility.Collapsed;
            AddButtom.Visibility = Visibility.Visible;
        }
        public RegExItem(ClickToCallPage clickToCallPage, string search, string replace, bool chek)
        {
            this.InitializeComponent();
            this.Style = ListStyle;
            this.clickToCallPage = clickToCallPage;
            this.Search.Text = search;
            this.Replece.Text = replace;
            Check = chek;
            SearchText = search;
            ReplaceText = replace;
            flag = true;
            Flag.IsOn = chek;
            Flag.Toggled += ToggleSwitch_Toggled;
        }

        private void Search_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (flag)
            {
                flag = false;
                Replece.IsReadOnly = false;
                
            }
            if (String.IsNullOrEmpty(Search.Text)) 
            {
                Replece.IsReadOnly = true;
                flag = true;
            }
        }
        private void Replece_GotFocus(object sender, RoutedEventArgs e)
        {
            clickToCallPage.SelectItem = this;
            if (!String.IsNullOrEmpty(Search.Text))
            {
                Replece.IsReadOnly = false;
            }
        }
        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            clickToCallPage.SelectItem = this;
        }

        private async void Search_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            SearchText = Search.Text;
            ReplaceText = Replece.Text;
            clickToCallPage.UpdateRegExList();
        }
        private async void Replece_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            SearchText = Search.Text;
            ReplaceText = Replece.Text;
            clickToCallPage.UpdateRegExList();
        }

        private void ListBoxItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Search.Text))
            {
                Check = Flag.IsOn;
                SearchText = Search.Text;
                ReplaceText = Replece.Text;
                clickToCallPage.UpdateRegExList();
            }
            else
            {
                Flag.IsOn = false;
            }
        }

        private void AddRegItem_Click(object sender, RoutedEventArgs e)
        {
            clickToCallPage.AddEmpty(this);
        }
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
    }
}
