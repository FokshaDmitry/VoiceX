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

namespace VoiceX.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        LocalStoreService localStoreService;
        RegistrationPage registrationPage;
        ProfilePage profilePage;
        public MainWindow()
        {
            InitializeComponent();
            localStoreService = new LocalStoreService();
            registrationPage = new RegistrationPage();
            profilePage = new ProfilePage();
            ExistingLigin();
        }

        public async void ExistingLigin()
        {
            var token = await localStoreService.LoadDataAsync("token");
            var pbx = await localStoreService.LoadDataAsync("pbxCode");
            if (!String.IsNullOrEmpty(token))
            {
                if (!String.IsNullOrEmpty(pbx))
                {
                    this.MainPage.Content = profilePage;
                }
            }
        }
    }
}
