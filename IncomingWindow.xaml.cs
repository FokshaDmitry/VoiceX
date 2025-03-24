using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VoiceX.Services;
using VoiceX.Views;
using VoiceX.Views.PhonePages;

namespace VoiceX
{
    /// <summary>
    /// Interaction logic for IncomingWindow.xaml
    /// </summary>
    public partial class IncomingWindow : Window
    {
        MainWindow window;
        ProfilePage profilePage;
        ActivCallPage activCallPage;
        public IncomingWindow(MainWindow window, ProfilePage profilePage, ActivCallPage activCallPage)
        {
            InitializeComponent();
            this.window = window;
            this.profilePage = profilePage;
            this.activCallPage = activCallPage;
        }
        public void ShowInBottomRight(string Name, string Phone, bool position)
        {
            if (!this.IsVisible)
            {
                if (position)
                {
                    double screenWidth = SystemParameters.PrimaryScreenWidth;
                    double screenHeight = SystemParameters.PrimaryScreenHeight;

                    double offsetX = 10;
                    double offsetY = 50; 

                    this.Left = screenWidth - this.Width - offsetX;
                    this.Top = screenHeight - this.Height - offsetY;
                }
                else
                {
                    double screenWidth = SystemParameters.PrimaryScreenWidth;
                    double screenHeight = SystemParameters.PrimaryScreenHeight;

                    double windowWidth = this.Width;
                    double windowHeight = this.Height;

                    this.Left = (screenWidth - windowWidth) / 2;
                    this.Top = (screenHeight - windowHeight) / 2;
                }
                UserName.Text = Name;
                UserPhone.Text = Phone;
                this.Show();
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            if (CoreService.activeCall != null)
            {
                CoreService.activeCall.Accept();
                profilePage.MainFrame.Navigate(activCallPage);
                window.ShowInBottomRight();
            }
        }

        private void Ignore_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            if (CoreService.activeCall != null)
            {
                CoreService.activeCall.hangup(new pj.CallOpParam() { statusCode = pj.pjsip_status_code.PJSIP_SC_BUSY_HERE});
            }
        }
    }
}
