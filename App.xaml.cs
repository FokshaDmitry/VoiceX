using pj;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using TTT.WindowsControls;
using VoiceX.Models;
using VoiceX.Services;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using VoiceX.Views;
using System.Security.AccessControl;
using System.Security.Principal;

namespace VoiceX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string PipeName = "VoiceXSingleInstance";
        private static Mutex mutex = new Mutex(true, "{MyApp_Unique_Id}");
        public static Account_data? AccountData { get; set; }
        public static string? UserPbx { get; set; }
        public static string? userToken { get; set; }
        public static bool MyComputer {  get; set; }
        public static DateTime timeOut {  get; set; }
        public CoreService Core { get; } = CoreService.Instance;
        Endpoint core;
        public App()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    try
                    {
                        pipeClient.Connect(100); // Подключаемся к серверу (основному процессу)
                        using (StreamWriter writer = new StreamWriter(pipeClient))
                        {
                            writer.WriteLine("RestoreWindow");
                            writer.Flush();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error " + ex.Message);
                    }
                }
                Environment.Exit(0);
                return;
            }
            AccountData = new Account_data();
            UserPbx = "";
            userToken = "";
            MyComputer = false;
            timeOut = new DateTime();
            timeOut = DateTime.Now;
            core = CoreService.Instance.Core;
            StartPipeServer();
        }
        private void StartPipeServer()
        {
            new Thread(async () =>
            {
                while (true)
                {
                    using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        await pipeServer.WaitForConnectionAsync(); // Ждём сигнал от второго процесса
                        using (StreamReader reader = new StreamReader(pipeServer))
                        {
                            string message = await reader.ReadLineAsync();
                            if (message == "RestoreWindow")
                            {
                                await Current.Dispatcher.InvokeAsync(() =>
                                {
                                    if (Current.MainWindow is MainWindow mainWindow)
                                    {
                                        mainWindow.RestoreWindow();
                                    }
                                });
                            }
                        }
                        // Закрываем соединение без ошибки
                        try
                        {
                            pipeServer.Disconnect();
                        }
                        catch (ObjectDisposedException)
                        {
                            // Игнорируем ошибку, если NamedPipe уже закрыт
                        }
                    }
                }
            })
            { IsBackground = true }.Start();
        }
        public static string TTTHotKey_HotkeyPressed()
        {

            string globalSelectedText;
            try
            {
                globalSelectedText = ClipboardHelper.getGlobalSelectedText(); 
            }
            catch
            {

                return "";
            }
            return globalSelectedText;
        }

    }

}
