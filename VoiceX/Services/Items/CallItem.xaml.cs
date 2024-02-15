using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CallItem : ListBoxItem
    {
        public string UserName;
        public string UserPhone;
        public CallItem(string Name, string Phone)
        {
            this.InitializeComponent();
            this.UserName = Name;
            this.UserPhone = Phone;
            
            ContactName.Text = Name;
        }

        private void ListBoxItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
    }
}
