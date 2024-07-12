using Microsoft.Win32;
using PdfScribeCore;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTT.WindowsControls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace SystrayComponent
{
    static partial class Program 
    {
        public static Mutex mutex = null;
        public static PdfScribeInstaller pdfScribeInstaller;
        public static bool NumberBoxFlag; // if number box open
        public static bool MouseClickFlag;
        public static AppServiceConnection connection = null;
        public static NamedPipeServerStream pipeServer;

        [STAThread]
        
        static void Main()
        {
            
            NumberBoxFlag = false;
            MouseClickFlag = false;
            pdfScribeInstaller = new PdfScribeInstaller();
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

                    // Remove the existing PDF file if present
                    File.Delete(outputFilename);
                    // Only set absolute minimum parameters, let the postscript input
                    // dictate as much as possible
                    String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER",  "-sDEVICE=pdfwrite",
                                                String.Format("-sOutputFile={0}", outputFilename), standardInputFilename,
                                                "-c", @"[/Creator(PdfScribe 1.0.7 (PSCRIPT5)) /DOCINFO pdfmark", "-f"};

                    GhostScript64.CallAPI(ghostScriptArguments);
                }
                CreatePdf();
            }
            catch
            {

                // We couldn't delete, or create a file
                // because it was in use

            }
            #region Config
            if (!ApplicationData.Current.LocalSettings.Values.Keys.Contains("Rools"))
            {
                ApplicationData.Current.LocalSettings.Values["Rools"] = "User";
            }
            if (ApplicationData.Current.LocalSettings.Values["Rools"].ToString() == "User" && !pdfScribeInstaller.IsPdfScribePrinterInstalled())
            {

                // Run app with admin root
                ProcessStartInfo procInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                };
                try
                {
                    Process.Start(procInfo);
                    ApplicationData.Current.LocalSettings.Values["Rools"] = "Admin";


                }
                catch (Exception ex)
                {
                    ApplicationData.Current.LocalSettings.Values["Rools"] = "User";
                    MessageBox.Show("Unable run application as administrator." + " " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


            }
            else if (ApplicationData.Current.LocalSettings.Values["Rools"].ToString() == "Admin" && !pdfScribeInstaller.IsPdfScribePrinterInstalled())
            {
                //Run virtual printer                                                    C:\Windows\System32\spool\drivers\x64    
                if (!pdfScribeInstaller.InstallPdfScribePrinter(Application.StartupPath, pdfScribeInstaller.RetrievePrinterDriverDirectory() + "\\SystrayComponent.exe", ""))
                {
                    MessageBox.Show(Application.StartupPath);
                    MessageBox.Show("Unable run application as administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApplicationData.Current.LocalSettings.Values["Rools"] = "User";
                }
                else
                {
                    // Copy drivers and App
                    CopyFolder(Application.StartupPath, pdfScribeInstaller.RetrievePrinterDriverDirectory());
                }

            }
            else if (!pdfScribeInstaller.IsPdfScribePrinterInstalled())
            {
                ApplicationData.Current.LocalSettings.Values["Rools"] = "User";
            }
            #endregion
            // Singl Run
            if (!Mutex.TryOpenExisting("MySystrayExtensionMutex", out mutex))
            {
                // Follow C:\Users\user\AppData\Local\Temp
                WatchDirectory(Path.GetTempPath());
                RunPipe();
                SetStartup();
                mutex = new Mutex(false, "MySystrayExtensionMutex");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SystrayApplicationContext());
                mutex.Close();

            }
            else
            {
                //Regist Hotkey
                HotKeyWindow.PostMessage(
                (IntPtr)HotKeyWindow.HWND_BROADCAST,
                HotKeyWindow.WM_SHOWME,
                IntPtr.Zero,
                IntPtr.Zero);
            }
            
        }

        private static void RunPipe()
        {
            pipeServer = new NamedPipeServerStream("VoiceXPipe", PipeDirection.In);

            // Запускаем поток для прослушивания подключений
            Thread serverThread = new Thread(ServerThread);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private static async void ServerThread()
        {
            while (true)
            {
                try
                {
                    await pipeServer.WaitForConnectionAsync();
                    using (StreamReader sr = new StreamReader(pipeServer))
                    {
                        string message = sr.ReadLine();
                        try
                        {
                            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
                            await appListEntries.First().LaunchAsync();
                            Thread.Sleep(1000);
                            while (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() != "Open")
                            {
                                Thread.Sleep(1000);
                            }
                            await Program.SendToUWP("ID", message);
                        }
                        catch
                        {

                        }
                        sr.Close();
                        sr.Dispose();
                    }
                    pipeServer = new NamedPipeServerStream("VoiceXPipe", PipeDirection.In);
                }
                catch
                {
        
                }
            }
        }

        public async static Task StartListeningForMessages()
        {
            do
            {
                try
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("MessageRecive"))
                    {
                        if (ApplicationData.Current.LocalSettings.Values["MessageRecive"].ToString() == "Recive")
                        {
                            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
                            await appListEntries.First().LaunchAsync();
                            ApplicationData.Current.LocalSettings.Values["MessageRecive"] = "";
                            Thread.Sleep(1000);
                            await SendToUWP("call", "Recive");
                        }
                    }
                    await Task.Delay(20);
                }
                catch
                {

                }
            } while (true);
        }
        public static async Task SendToUWP(string key, string message)
        {
            try
            {

                if (connection != null)
                {
                    connection.ServiceClosed -= Connection_ServiceClosed;
                    connection = null;
                }
                ValueSet value = new ValueSet
                {
                    { key, message }
                };
                if (connection == null)
                {
                    connection = new AppServiceConnection
                    {
                        PackageFamilyName = Package.Current.Id.FamilyName,
                        AppServiceName = "ClickToCallConnection"
                    };
                    connection.ServiceClosed += Connection_ServiceClosed;
                    AppServiceConnectionStatus connectionStatus = await connection.OpenAsync();
                    if (connectionStatus != AppServiceConnectionStatus.Success)
                    {
                        System.Windows.MessageBox.Show("Status: " + connectionStatus.ToString());
                        Application.Exit();
                        return;
                    }
                }
                await connection.SendMessageAsync(value);
            }
            catch
            {
                throw;
            }
        }
        public static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            if (connection != null)
            {
                connection.ServiceClosed -= Connection_ServiceClosed;
                connection = null;
            }

        }
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }
        private static void WatchDirectory(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*.pdf"
            };
            watcher.Created += new FileSystemEventHandler(OnFileCreated);
            watcher.EnableRaisingEvents = true;
        }

        private async static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            string mass = "";
            bool fileIsAvailable = false;
            string fileName = e.Name;
            string filePath = e.FullPath;
            if (fileName.Contains(".tmp"))
            {
                while (!fileIsAvailable)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            var masstmp = new byte[fs.Length];
                            await fs.ReadAsync(masstmp, 0, masstmp.Length);
                            mass = Convert.ToBase64String(masstmp);
                            fs.Close();
                            fs.Dispose();
                            fileIsAvailable = true;
                        }
                    }
                    catch (IOException)
                    {
                        // Файл занят другим процессом или потоком
                        Thread.Sleep(1000); // Подождать одну секунду
                    }
                }
                if (!IsPdfFileEmpty(filePath))
                {
                    try
                    {
                        IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
                        await appListEntries.First().LaunchAsync();
                        Thread.Sleep(1000);
                    }
                    catch { }
                    try
                    {
                        while (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() != "Open")
                        {
                            Thread.Sleep(1000);
                        }
                        ValueSet hotkeyPressed = new ValueSet
                        {
                            { "path", mass }
                        };
                        AppServiceConnection connection = new AppServiceConnection
                        {
                            PackageFamilyName = Package.Current.Id.FamilyName,
                            AppServiceName = "ClickToCallConnection"
                        };
                        await connection.OpenAsync();
                        await connection.SendMessageAsync(hotkeyPressed);
                        File.Delete(filePath);
                        connection.Dispose();
                    }
                    catch { }
                }
            }
            
        }
        private static bool IsPdfFileEmpty(string filePath)
        {
            PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);

            // Получение количества страниц в PDF-файле
            if(document.PageCount > 1)
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
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
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
                foreach (var element in cSequence)
                {
                    textList.AddRange(ExtractText(element));
                }
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                textList.Add(cString.Value);
            }
            return textList;
        }
        private static bool CheckImage(PdfPage page)
        {
            PdfDictionary resources = page.Elements.GetDictionary("/Resources");
            if (resources != null)
            {
                // Get external objects dictionary
                PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                if (xObjects != null)
                {
                    ICollection<PdfItem> items = xObjects.Elements.Values;
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
        private static void SetStartup()
        {
            using (RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                reg.SetValue("SystrayComponent", Application.ExecutablePath.ToString() + "SystrayComponent.exe");
                
            }
        }
    }
}
