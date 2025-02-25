using pj;
using System.IO.Pipes;
using System.IO;
using System.Windows;
using TTT.WindowsControls;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf;
using PdfScribeCore;
using System.Diagnostics;
using System.Security.Principal;

namespace VoiceX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string PipeName = "VoiceXSingleInstance";
        private static Mutex mutex = new Mutex(true, "{VoiceX_Unique_Id}");
        public static Account_data? AccountData { get; set; }
        public static string? UserPbx { get; set; }
        public static string? userToken { get; set; }
        public static bool MyComputer {  get; set; }
        public static DateTime timeOut {  get; set; }
        public CoreService Core { get; } = CoreService.Instance;
        PdfScribeInstaller pdfScribeInstaller;
        Endpoint core;
        public App()
        {
            pdfScribeInstaller = new PdfScribeInstaller();
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
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
                    catch 
                    {
                       
                    }
                }
                Environment.Exit(0);
                return;
            }
            if (!pdfScribeInstaller.IsPdfScribePrinterInstalled())
            {
                if (!IsRunningAsAdmin())
                {
                    RestartAsAdmin();
                    return; // Завершаем текущий процесс
                }
                if (pdfScribeInstaller.InstallPdfScribePrinter(exePath + "\\DLL", exePath + "\\VoiceX.exe", ""))
                {
                    
                }

            }
            AccountData = new Account_data();
            UserPbx = "";
            userToken = "";
            MyComputer = true;
            timeOut = new DateTime();
            timeOut = DateTime.Now;
            core = CoreService.Instance.Core;
            StartPipeServer();
        }

        public bool IsRunningAsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        public static void RestartAsAdmin()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule!.FileName,
                UseShellExecute = true,
                Verb = "runas" // Запуск с правами администратора
            };

            try
            {
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске от имени администратора: {ex.Message}");
            }

            Environment.Exit(0); // Завершаем текущий процесс
        }
        private async Task OnFileCreated(string filePath)
        {
            byte[] mass;
            if (filePath!.Contains(".tmp"))
            {
                try
                {
                    Thread.Sleep(1000);
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        mass = new byte[fs.Length];
                        await fs.ReadAsync(mass, 0, mass.Length);
                        fs.Close();
                        fs.Dispose();
                    }
                    var date = DateTime.Now;
                    FaxPage.Files?.Add($"File: {date.ToString("dd.MM") + "/" + date.ToString("T")};{mass.Length / 1024}", mass);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

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
                                await Current.Dispatcher.InvokeAsync(async () =>
                                {
                                    if (Current.MainWindow is MainWindow mainWindow)
                                    {
                                        mainWindow.RestoreWindow();

                                        String standardInputFilename = Path.GetTempFileName();
                                        String outputFilename = standardInputFilename + ".pdf";
                                        try
                                        {
                                            //Create pdf file from driver 
                                            using (BinaryReader standardInputReader = new BinaryReader(Console.OpenStandardInput()))
                                            {
                                                using (FileStream standardInputFile = new FileStream(standardInputFilename, FileMode.Create, FileAccess.ReadWrite))
                                                {
                                                    standardInputReader.BaseStream.CopyTo(standardInputFile);

                                                    standardInputReader.Close();
                                                    standardInputReader.Dispose();

                                                }
                                            }
                                            async void CreatePdf()
                                            {
                                                try
                                                {
                                                    // Remove the existing PDF file if present
                                                    File.Delete(outputFilename);
                                                    // Only set absolute minimum parameters, let the postscript input
                                                    // dictate as much as possible
                                                    String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER",  "-sDEVICE=pdfwrite",
                                                    String.Format("-sOutputFile={0}", outputFilename), standardInputFilename,  "-c", @"[/Creator(PdfScribe 1.0.7 (PSCRIPT5)) /DOCINFO pdfmark", "-f"};

                                                    GhostScript64.CallAPI(ghostScriptArguments);
                                                    await OnFileCreated(outputFilename);
                                                }
                                                catch (Exception ex)
                                                {
                                                    MessageBox.Show(ex.Message);
                                                }
                                            }
                                            CreatePdf();
                                        }
                                        catch
                                        {

                                            // We couldn't delete, or create a file
                                            // because it was in use

                                        }

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
