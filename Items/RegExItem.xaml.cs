using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.Views.ControlPages;

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
        public string? SearchText;
        public string? ReplaceText;
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
            this.clickToCallPage = clickToCallPage;
            this.Search.Text = search;
            this.Replece.Text = replace;
            if (search != "Search")
            {
                Search.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            if (replace != "Replace")
            {
                Replece.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            Check = chek;
            SearchText = search;
            ReplaceText = replace;
            flag = true;
            Flag.IsChecked = chek;
        }
        private void Replece_GotFocus(object sender, RoutedEventArgs e)
        {
            clickToCallPage.SelectItem = this;
            if (!String.IsNullOrEmpty(Search.Text))
            {
                Replece.IsReadOnly = false;
            }
            if (Replece.Text == "Replace")
            {
                Replece.Text = "";
                Replece.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            RegExBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 123, 118, 254));
        }
        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Search.Text == "Search")
            {
                Search.Text = "";
                Search.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            clickToCallPage.SelectItem = this;
            RegExBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 123, 118, 254));
        }

        private async void Search_LosingFocus(object sender, RoutedEventArgs args)
        {
            SearchText = Search.Text == "Search" ? "" : Search.Text;
            ReplaceText = Replece.Text == "Replace" ? "" : Replece.Text;
            await clickToCallPage.UpdateRegExList();
            if (String.IsNullOrEmpty(Search.Text))
            {
                Search.Text = "Search";
                Search.Foreground = new SolidColorBrush(Color.FromArgb(255, 181, 181, 181));
            }
            RegExBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 187, 186, 255));
        }
        private async void Replece_LosingFocus(object sender, RoutedEventArgs args)
        {
            SearchText = Search.Text == "Search" ? "" : Search.Text;
            ReplaceText = Replece.Text == "Replace" ? "" : Replece.Text;
            await clickToCallPage.UpdateRegExList();
            if (String.IsNullOrEmpty(Replece.Text))
            {
                Replece.Text = "Replace";
                Replece.Foreground = new SolidColorBrush(Color.FromArgb(255, 181, 181, 181));
            }
            RegExBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 187, 186, 255));
        }

        private void AddRegItem_Click(object sender, RoutedEventArgs e)
        {
            clickToCallPage.AddEmpty(this);
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (flag)
            {
                flag = false;
                Replece.IsReadOnly = false;

            }
            if (String.IsNullOrEmpty(Search.Text) && Search.Text != "Search")
            {
                Replece.IsReadOnly = true;
                flag = true;
            }
        }

        private async void Flag_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Search.Text) && Search.Text != "Search")
            {
                Check = (bool)Flag.IsChecked!;
                SearchText = Search.Text;
                ReplaceText = Replece.Text;
                await clickToCallPage.UpdateRegExList();
            }
            else
            {
                Flag.IsChecked = false;
            }
        }
    }
}
