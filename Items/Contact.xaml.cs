using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.Services;
using VoiceX.Views;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;
using Windows.ApplicationModel.Core;

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

        private void Info_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, 5, Img.Margin.Right, 0);
        }

        private void Info_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, 6, Img.Margin.Right, 0);
        }

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                var phone = Phone.Text;
                foreach (var regex in ProfilePage.regexNotes?.Where(r => r.Check)!)
                {
                    phone = phone.Replace(regex?.Search!, regex?.Replace);
                }
                CoreService.Instance.MakeCall(phone, App.AccountData?.Data.Sip_Settings.Sip_server!);
            }
            catch
            {

            }
        }
    }
}
