using System;
using System.Collections.Generic;
using VoiceX.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaxFileItem : ListBoxItem
    {
        readonly FaxPage faxPage;
        public string Path;
        public KeyValuePair<string, byte[]> File;
        public FaxFileItem(KeyValuePair<string, byte[]> File, FaxPage faxBox)
        {
            this.InitializeComponent();
            this.File = File;
            this.Path = File.Key;
            this.FileName.Text = File.Key.Split(';')[0];
            this.Size.Text = File.Key.Split(';')[1] + "KB";
            faxPage = faxBox;
        }
        public FaxFileItem(FaxPage faxBox, bool first)
        {
            this.InitializeComponent();
            
            faxPage = faxBox;
            if (first)
            {
                Cloud.Visibility = Visibility.Visible;
                FaileInfo.Visibility = Visibility.Collapsed;
            }
            else
            {

                AddButtom.Visibility = Visibility.Visible;
                FaileInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void ListBoxItem_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            faxPage.RemoveFile(this);
        }

        private async void AddFaxItem_Click(object sender, RoutedEventArgs e)
        {
            await faxPage.OpenFileSelector();
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
