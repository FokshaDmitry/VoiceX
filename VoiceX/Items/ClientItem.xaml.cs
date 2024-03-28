using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VoiceX.Services;
using VoiceX.Views.ClientCardPages;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClientItem : ListBoxItem
    {
        private string idDb;
        public ClientItem(string Name, string Phone, string Email, string IdDB, int color)
        {
            this.InitializeComponent();
            idDb = IdDB;
            FirstWord.Text = Name.Substring(0, 1);
            UserName.Text = Name;
            this.Phone.Text = Phone;
            this.Email.Text = Email;
            contactBackgroundColor.Background = color == 1 ? new SolidColorBrush(Color.FromArgb(255, 138, 99, 251)) : new SolidColorBrush(Color.FromArgb(255, 229, 167, 224));
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

        private async void Info_Click(object sender, RoutedEventArgs e)
        {
            await App.OpenWindow(typeof(ClientCard), idDb);
        }
    }
}
