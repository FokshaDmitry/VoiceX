using PdfSharp.Pdf.Content.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Xml.Serialization;
using TTT.Win32;

using Microsoft.VisualBasic.CompilerServices;
using TTT.WindowsControls;

namespace VoiceX.Services
{
    public class ClickToCallService : NativeWindow
    {
        LocalStoreService storeService;
        private const int WM_HOTKEY = 0x0312;
        private const int WM_DESTROY = 0x0002;
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private Int32 IDs = 0;
        public delegate void HotkeyDelegate(string ID);
        public event HotkeyDelegate? HotkeyPressed;
        public string Key = "";
        public int MainKey = 0;
        public int RegistId = 1001;
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_KEYUP = 0x0002;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        // creates a headless Window to register for and handle WM_HOTKEY
        public ClickToCallService()
        {
            this.CreateHandle(new CreateParams());
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit!);
            storeService = new LocalStoreService();
        }
        private static Dictionary<string, ReadOnlyCollection<ClipboardDataItem>> clipboardBackupData = new Dictionary<string, ReadOnlyCollection<ClipboardDataItem>>();

        private static ReadOnlyCollection<ClipboardDataItem> m_singlebackup;

        public static string getGlobalSelectedText()
        {
            Application.DoEvents();
            string text = "";
            ClipboardHelper clipboardHelper = new ClipboardHelper();
            text = "";
            Backup();
            string text2 = "";
            text2 = clipboardHelper.getClipboardStringdata();
            SendCopy(User32.GetForegroundWindow());
            Application.DoEvents();
            text = clipboardHelper.getClipboardStringdata();
            if (Operators.CompareString(text2, text, TextCompare: false) == 0)
            {
                Thread.Sleep(100);
                SendCopy(User32.GetForegroundWindow());
                Application.DoEvents();
                text = clipboardHelper.getClipboardStringdata();
                if (Operators.CompareString(text2, text, TextCompare: false) == 0)
                {
                    Application.DoEvents();
                    Thread.Sleep(150);
                    SendCopy(User32.GetForegroundWindow());
                    text = clipboardHelper.getClipboardStringdata();
                }
            }

            Restore();
            Application.DoEvents();
            return text;
        }

        public static void SendCopy(IntPtr WindowHandle)
        {
            IntPtr foregroundWindow = User32.GetForegroundWindow();
            User32.SetForegroundWindow(WindowHandle);
            SendCopy();
            User32.SetForegroundWindow(foregroundWindow);
        }

        public static void SendPaste(IntPtr WindowHandle)
        {
            IntPtr foregroundWindow = User32.GetForegroundWindow();
            User32.SetForegroundWindow(WindowHandle);
            SendPaste();
            User32.SetForegroundWindow(foregroundWindow);
        }

        public static void SendCopy()
        {
            User32.keybd_event(17, 0, 0u, 0u);
            User32.keybd_event(67, 0, 0u, 0u);
            Thread.Sleep(200);
            User32.keybd_event(67, 0, 2u, 0u);
            User32.keybd_event(17, 0, 2u, 0u);
        }

        public static void SendPaste()
        {
            User32.keybd_event(17, 0, 0u, 0u);
            User32.keybd_event(86, 0, 0u, 0u);
            Thread.Sleep(200);
            User32.keybd_event(86, 0, 2u, 0u);
            User32.keybd_event(17, 0, 2u, 0u);
        }

        public string getClipboardStringdata()
        {
            IDataObject dataObject = Clipboard.GetDataObject();
            if (dataObject.GetDataPresent(typeof(string)))
            {
                return (string)dataObject.GetData(typeof(string));
            }

            return "";
        }

        public static bool EmptyClipboard()
        {
            return User32.EmptyClipboard();
        }

        public static bool SetClipboard(ReadOnlyCollection<ClipboardDataItem> clipData)
        {
            int num = 0;
            checked
            {
                while (!User32.OpenClipboard(IntPtr.Zero))
                {
                    if (num > 5)
                    {
                        throw new Exception("OpenClipboard FAILED!");
                    }

                    num++;
                    Thread.Sleep(num * 25);
                }

                EmptyClipboard();
                IEnumerator<ClipboardDataItem> enumerator = clipData.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ClipboardDataItem current = enumerator.Current;
                    IntPtr hMem = Kernel32.GlobalAlloc(8194u, current.Size);
                    IntPtr destination = Kernel32.GlobalLock(hMem);
                    if ((int)(uint)current.Size > 0)
                    {
                        Marshal.Copy(current.Buffer, 0, destination, current.Buffer.GetLength(0));
                    }

                    Kernel32.GlobalUnlock(hMem);
                    User32.SetClipboardData(current.Format, hMem);
                }

                User32.CloseClipboard();
                return true;
            }
        }

        public static void SaveToFile(ReadOnlyCollection<ClipboardDataItem> clipData, string clipName)
        {
            IEnumerator<ClipboardDataItem> enumerator = clipData.GetEnumerator();
            int num = 0;
            if (Directory.Exists(clipName))
            {
                Directory.Delete(clipName, recursive: true);
            }

            DirectoryInfo directoryInfo = Directory.CreateDirectory(clipName);
            while (enumerator.MoveNext())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ClipboardDataItem));
                using (StreamWriter streamWriter = new StreamWriter(directoryInfo.FullName + "\\" + num + ".cli", append: false))
                {
                    xmlSerializer.Serialize(streamWriter, enumerator.Current);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                num = checked(num + 1);
            }
        }

        private static ReadOnlyCollection<ClipboardDataItem> ReadFromFile(string clipName)
        {
            List<ClipboardDataItem> list = new List<ClipboardDataItem>();
            checked
            {
                if (Directory.Exists(clipName))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(clipName);
                    int num = directoryInfo.GetFiles().GetLength(0) - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ClipboardDataItem));
                        FileInfo fileInfo = new FileInfo(directoryInfo.FullName + "\\" + i + ".cli");
                        using FileStream fileStream = fileInfo.Open(FileMode.Open);
                        list.Add((ClipboardDataItem)xmlSerializer.Deserialize(fileStream));
                        fileStream.Flush();
                        fileStream.Close();
                    }
                }

                return new ReadOnlyCollection<ClipboardDataItem>(list);
            }
        }

        public static void Serialize(string clipName)
        {
            ReadOnlyCollection<ClipboardDataItem> clipboard = GetClipboard();
            SaveToFile(clipboard, clipName);
        }

        public static void Backup(string clipName, bool overwrite)
        {
            ReadOnlyCollection<ClipboardDataItem> clipboard = GetClipboard();
            if (overwrite && clipboardBackupData.ContainsKey(clipName))
            {
                clipboardBackupData.Remove(clipName);
            }

            clipboardBackupData.Add(clipName, clipboard);
        }

        public static bool Restore(string clipName)
        {
            clipboardBackupData.TryGetValue(clipName, out var value);
            return SetClipboard(value);
        }

        public static bool FreeBackup(string clipName)
        {
            clipboardBackupData.Remove(clipName);
            bool result = default(bool);
            return result;
        }

        public static void Backup()
        {
            m_singlebackup = GetClipboard();
        }

        public static bool Restore()
        {
            return SetClipboard(m_singlebackup);
        }

        public static bool Deserialize(string clipName)
        {
            ReadOnlyCollection<ClipboardDataItem> clipboard = ReadFromFile(clipName);
            return SetClipboard(clipboard);
        }

        public static ReadOnlyCollection<ClipboardDataItem> GetClipboard()
        {
            List<ClipboardDataItem> list = new List<ClipboardDataItem>();
            int num = 0;
            uint target;
            checked
            {
                while (!User32.OpenClipboard(IntPtr.Zero))
                {
                    if (num > 5)
                    {
                        throw new Exception("OpenClipboard FAILED!");
                    }

                    num++;
                    Thread.Sleep(num * 25);
                }

                target = 0u;
            }

            while ((ulong)InlineAssignHelper(ref target, User32.EnumClipboardFormats(target)) != 0)
            {
                try
                {
                    string formatName = "0";
                    if ((long)target > 14L)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        if (User32.GetClipboardFormatName(target, stringBuilder, 100) > 0)
                        {
                            formatName = stringBuilder.ToString();
                        }
                    }

                    IntPtr clipboardData = User32.GetClipboardData(target);
                    checked
                    {
                        if (!(clipboardData == IntPtr.Zero))
                        {
                            UIntPtr uIntPtr = Kernel32.GlobalSize(clipboardData);
                            IntPtr source = Kernel32.GlobalLock(clipboardData);
                            byte[] array;
                            if ((int)(uint)uIntPtr > 0)
                            {
                                array = new byte[(int)(uint)uIntPtr - 1 + 1];
                                int length = Convert.ToInt32(uIntPtr.ToString());
                                Marshal.Copy(source, array, 0, length);
                            }
                            else
                            {
                                array = new byte[0];
                            }

                            ClipboardDataItem clipboardDataItem = new ClipboardDataItem(target, formatName, array);
                            clipboardDataItem.FormatName = formatName;
                            list.Add(clipboardDataItem);
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            User32.CloseClipboard();
            return new ReadOnlyCollection<ClipboardDataItem>(list);
        }

        private static T InlineAssignHelper<T>(ref T target, T value)
        {
            target = value;
            return value;
        }
        public void RegisterCombo(Int32 ID, int fsModifiers, Keys vlc)
        {
            if (RegisterHotKey(this.Handle, ID, fsModifiers, (int)vlc))
            {
                if (IDs != 0)
                {
                    UnregisterHotKey(this.Handle, IDs);
                }
                IDs = ID;
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            this.DestroyHandle();
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SHOWME)
            {
                ChangeKey();
            }
            else if (m.Msg == WM_HOTKEY)
            {
                try
                {
                    HotkeyPressed?.Invoke(App.TTTHotKey_HotkeyPressed());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else if (m.Msg == WM_DESTROY)
            {
                UnregisterHotKey(this.Handle, IDs);
            }
            base.WndProc(ref m);

        }
        public async void ChangeKey()
        {
            try
            {
                Key = await storeService.LoadDataAsync("key");
                MainKey = int.Parse(await storeService.LoadDataAsync("mainkey"));
            }
            catch
            {
                Key = "S";
                MainKey = 1;
            }
            Keys keys1 = new Keys();
            
            try
            {
                keys1 = (Keys)char.Parse(Key);
            }
            catch
            {
                keys1 = (Keys)char.Parse("S");
                MainKey = 1;
                await storeService.SaveDataAsync("mainkey", "1");
                await storeService.SaveDataAsync("key", "S");
            }
            if (!String.IsNullOrEmpty(Key.ToString()) && MainKey > 0)
            {
                RegisterCombo(RegistId, MainKey, keys1);
                RegistId++;
            }
        }
    }
}
