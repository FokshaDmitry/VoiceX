using System;
using System.Windows;
using System.Windows.Controls;
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
            this.clickToCallPage = clickToCallPage;
            this.Search.Text = search;
            this.Replece.Text = replace;
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
        }
        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            clickToCallPage.SelectItem = this;
        }

        private async void Search_LosingFocus(object sender, RoutedEventArgs args)
        {
            SearchText = Search.Text;
            ReplaceText = Replece.Text;
            await clickToCallPage.UpdateRegExList();
        }
        private async void Replece_LosingFocus(object sender, RoutedEventArgs args)
        {
            SearchText = Search.Text;
            ReplaceText = Replece.Text;
            await clickToCallPage.UpdateRegExList();
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
            if (String.IsNullOrEmpty(Search.Text))
            {
                Replece.IsReadOnly = true;
                flag = true;
            }
        }

        private async void Flag_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Search.Text))
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
