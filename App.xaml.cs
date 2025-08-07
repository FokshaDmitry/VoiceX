using PdfScribeCore;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TTT.WindowsControls;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views;
using Windows.ApplicationModel;

namespace VoiceX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user64.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user64.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        private const string PipeName = "VoiceXSingleInstance";
        private static Mutex mutex = new Mutex(true, "{VoiceX_Unique_Id}");
        public static Account_data? AccountData { get; set; }
        public static string? UserPbx { get; set; }
        public static string? userToken { get; set; }
        public static string? fw { get; set; }
        public static bool MyComputer {  get; set; }
        public static DateTime timeOut {  get; set; }
        public static bool IsMSIX { get; set; }
        private FileSystemWatcher? _watcher;
        private PdfScribeInstaller pdfScribeInstaller;
        private static List<CultureInfo> m_Languages = new List<CultureInfo>();

        public static event EventHandler LanguageChanged;
        public static List<CultureInfo> Languages
        {
            get
            {
                return m_Languages;
            }
        }

        public App()
        {
            m_Languages.Clear();
            m_Languages.Add(new CultureInfo("en-US")); 
            m_Languages.Add(new CultureInfo("he-IL"));
            m_Languages.Add(new CultureInfo("ru-RU"));
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            String standardInputFilename = Path.GetTempFileName();
            String outputFilename = standardInputFilename + ".pdf";
            pdfScribeInstaller = new PdfScribeInstaller();
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
                        //MessageBox.Show(ex.Message);
                    }
                }
                CreatePdf();
            }
            catch
            {

                // We couldn't delete, or create a file
                // because it was in use

            }
            if (!pdfScribeInstaller.IsPdfScribePrinterInstalled())
            {
                Task.Run(() =>
                {
                    try
                    {
                        Process process = new Process();
                        var psi = new ProcessStartInfo
                        {
                            FileName = Path.Combine(exePath, "PrinterInstaller", "SystrayComponent.exe"),
                            Arguments = exePath + "VoiceX.exe",
                            UseShellExecute = true,
                            Verb = "runas", // Запуск с правами администратора
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        process.StartInfo = psi;
                        process.Start();
                        Task.Delay(5000);
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill(true);
                        }
                    }
                    catch
                    {

                    }
                });
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
            AccountData = new Account_data();
            UserPbx = "";
            userToken = "";
            MyComputer = true;
            timeOut = new DateTime();
            timeOut = DateTime.Now;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                //MessageBox.Show("Unhandled exception: " + ex.Message);
            };
        }
        public static CultureInfo Language
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == Thread.CurrentThread.CurrentUICulture) return;

                //1. Меняем язык приложения:
                Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri(String.Format("/Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    case "he-IL":
                        dict.Source = new Uri(String.Format("/Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("/Resources/lang.xaml", UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("/Resources/lang.")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }
                if (LanguageChanged != null)
                {
                    //4. Вызываем евент для оповещения всех окон.
                    LanguageChanged(Application.Current, new EventArgs());
                }
            }
        }
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                IsMSIX = Package.Current?.Id != null;
            }
            catch
            {
                IsMSIX = false;
            }
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
                    //MessageBox.Show(ex.Message);
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

            string globalSelectedText = "";
            try
            {
                ClipboardHelper.Backup();
                globalSelectedText = ClipboardHelper.getGlobalSelectedText();
                Debug.WriteLine("SELECT TEXT: " + globalSelectedText);
            }
            catch (AccessViolationException ex)
            {
                Debug.Write(ex.Message);
                return "";
            }
            return globalSelectedText;
        }

    }

}
