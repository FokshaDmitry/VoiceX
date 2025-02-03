using System.Configuration;
using System.Data;
using System.Windows;
using VoiceX.Models;

namespace VoiceX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static Account_data AccountData { get; set; }
        public static string UserPbx { get; set; }
        public static string userToken { get; set; }
        public static bool MyComputer {  get; set; }
        public App()
        {
            AccountData = new Account_data();
            UserPbx = "";
            userToken = "";
            MyComputer = false;
        }
    }

}
