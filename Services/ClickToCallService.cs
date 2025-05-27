using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
        // creates a headless Window to register for and handle WM_HOTKEY
        public ClickToCallService()
        {
            this.CreateHandle(new CreateParams());
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit!);
            storeService = new LocalStoreService();
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
