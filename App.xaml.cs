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
        private FileSystemWatcher _watcher;
        public CoreService Core { get; } = CoreService.Instance;
        PdfScribeInstaller pdfScribeInstaller;
        Endpoint core;
        public App()
        {
            pdfScribeInstaller = new PdfScribeInstaller();
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
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
                void CreatePdf()
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
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartPipeServer();
            WatchDirectory(Path.GetTempPath()); // Следим за временной папкой
        }
        private void WatchDirectory(string path)
        {
            _watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*.pdf"
            };
            _watcher.Created += new FileSystemEventHandler(OnFileCreated);
            _watcher.EnableRaisingEvents = true;
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
        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            byte[] mass;
            string filePath = e.FullPath;
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
                    if (!IsPdfFileEmpty(filePath))
                    {
                        var date = DateTime.Now;
                        FaxPage.Files?.Add($"File: {date.ToString("dd.MM") + "/" + date.ToString("T")};{mass.Length / 1024}", mass);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
        private static bool IsPdfFileEmpty(string filePath)
        {
            PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);

            // Получение количества страниц в PDF-файле
            if (document.PageCount > 1)
            {
                return false;
            }
            else
            {
                return IsPageEmpty(document.Pages[0]);
            }
        }
        static bool IsPageEmpty(PdfPage page)
        {

            // Получение контента страницы
            var content = ContentReader.ReadContent(page);

            // Проверка наличия контента на странице
            if (ExtractText(content).Count() > 0)
            {
                return false; // Если есть контент, страница не пустая
            }
            if (CheckImage(page))
            {
                return false;
            }
            return true; // Если нет контента, страница пустая
        }
        private static IEnumerable<string> ExtractText(CObject cObject)
        {
            var textList = new List<string>();
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator?.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator?.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                    {
                        textList.AddRange(ExtractText(cOperand));
                    }
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence!)
                {
                    textList.AddRange(ExtractText(element));
                }
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                textList.Add(cString?.Value!);
            }
            return textList;
        }
        private static bool CheckImage(PdfPage page)
        {
            PdfDictionary resources = page.Elements.GetDictionary("/Resources")!;
            if (resources != null)
            {
                // Get external objects dictionary
                PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject")!;
                if (xObjects != null)
                {
                    ICollection<PdfItem> items = xObjects.Elements.Values!;
                    // Iterate references to external objects
                    foreach (PdfItem item in items)
                    {
                        PdfReference reference = (PdfReference)item;
                        if (reference != null)
                        {
                            PdfDictionary xObject = (PdfDictionary)reference.Value;
                            // Is external object an image?
                            if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
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
