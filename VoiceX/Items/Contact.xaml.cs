using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using VoiceX.Services;
using VoiceX.Views.ClientCardPages;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Contact : ListBoxItem
    {
        public string contactName;
        public string contactPhone;
        public Contact(string Name, string Phone, int color)
        {
            this.InitializeComponent();
            contactName = Name;
            contactPhone = Phone;
            FirstWord.Text = Name.Substring(0, 1);
            this.UserName.Text = Name;
            this.Phone.Text = Phone;
            contactBackgroundColor.Background = color == 1 ?  new SolidColorBrush(Color.FromArgb(255, 138, 99, 251)) : new SolidColorBrush(Color.FromArgb(255, 229, 167, 224));
        }
        private void ListBoxItem_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
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

        private async void Call_Click(object sender, RoutedEventArgs e)
        {
            if (!App.appWindows.Select(s => s.Title).Contains("Dialpad"))
            {
                await App.OpenWindow(typeof(DialpadPage), Phone.Text);
            }
            else
            {
                try
                {
                    try
                    {
                        await CoreService.Instance.OpenMicrophonePopup();
                    }
                    catch
                    {
                        return;
                    }
                    var phone = Phone.Text;
                    foreach (var regex in ProfilePage.regexNotes.Where(r => r.Check))
                    {
                        phone = phone.Replace(regex.Search, regex.Replace);
                    }
                    CoreService.Instance.Call(phone);
                }
                catch 
                {
                    
                }
            }
        }
    }
}
