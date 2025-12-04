using System;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.Services;
using VoiceX.Views;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                var phone = Phone.Text;
                foreach (var regex in ProfilePage.regexNotes?.Where(r => r.Check)!)
                {
                    phone = phone.Replace(regex?.Search!, regex?.Replace);
                }
                phone = Regex.Replace(phone, @"[^0-9*#]", "");
                var call = CoreService.Instance.MakeCall(phone, App.AccountData?.Data.Sip_Settings.Sip_server!);
                if (call == null)
                {
                    ProfilePage.window?.ShowError("Call not create. Please check connection and audio.");
                }
            }
            catch
            {

            }
        }
    }
}
