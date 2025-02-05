using pj;
using System.Windows;
using VoiceX.Models;
using VoiceX.Services;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;

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
        public static DateTime timeOut {  get; set; }
        public CoreService Core { get; } = CoreService.Instance;
        Endpoint core;
        public App()
        {
            AccountData = new Account_data();
            UserPbx = "";
            userToken = "";
            MyComputer = false;
            timeOut = new DateTime();
            timeOut = DateTime.Now;
            core = CoreService.Instance.Core;
        }
    }

}
