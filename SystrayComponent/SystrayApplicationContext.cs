using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace SystrayComponent
{
    class SystrayApplicationContext : ApplicationContext
    {

        public static HotKeyWindow hotkeyWindow = null;
        private readonly NotifyIcon notifyIcon = null;

        public SystrayApplicationContext()
        {
            MenuItem openMenuItem = new MenuItem("Open VoiceX", new EventHandler(OpenApp));
            MenuItem sendMenuItem = new MenuItem("Open dialpad to VoiceX", new EventHandler(OpenDialpad));
            MenuItem legacyMenuItem = new MenuItem("Open clients", new EventHandler(OpenClient));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));
            openMenuItem.DefaultItem = true;

            notifyIcon = new NotifyIcon
            {
                Text = "VoiceX"
            };
            notifyIcon.DoubleClick += new EventHandler(OpenApp);
            notifyIcon.Icon = Properties.Resources.Logo48;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { openMenuItem, sendMenuItem, legacyMenuItem, exitMenuItem });
            notifyIcon.Visible = true;
            
            hotkeyWindow = new HotKeyWindow();
            hotkeyWindow.HotkeyPressed += new HotKeyWindow.HotkeyDelegate(Hotkeys_HotkeyPressed);
            hotkeyWindow.ChangeKey();
            Program.MouseClickFlag = true;
            Task.Run(() =>Program.StartListeningForMessages());
        }

        private async void OpenApp(object sender, EventArgs e)
        {
            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();
        }
        
        private async void OpenDialpad(object sender, EventArgs e)
        {
            try
            {
                IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
                await appListEntries.First().LaunchAsync();
                Thread.Sleep(1000);
                if (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() == "Open")
                {
                    await Program.SendToUWP("dialpad", "Message from Systray Extension");
                }
                else
                {
                    while (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() != "Open")
                    {
                        Thread.Sleep(1000);
                    }
                    await Program.SendToUWP("dialpad", "Message from Systray Extension");
                }
            }
            catch 
            {
               
            }
        }

        private async void OpenClient(object sender, EventArgs e)
        {
            try
            {
                IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
                await appListEntries.First().LaunchAsync();
                Thread.Sleep(1000);
                if (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() == "Open")
                {
                    await Program.SendToUWP("clients", "Message from Systray Extension");
                }
                else
                {
                    while (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() != "Open")
                    {
                        Thread.Sleep(1000);
                    }
                    await Program.SendToUWP("clients", "Message from Systray Extension");
                }
            }
            catch 
            {
                
            }
        }

        private async void Exit(object sender, EventArgs e)
        {
            try
            {
                Program.MouseClickFlag = false;
                if (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() == "Open")
                {
                    await Program.SendToUWP("exit", "Exit");
                    Application.Exit();
                }
                else
                {
                    Application.Exit();
                }
            }
            catch
            {
                Application.Exit();
            }

        }
        public async void Hotkeys_HotkeyPressed(string Phone)
        {
            
            if (!String.IsNullOrEmpty(Phone))
            {
                try
                {
                    IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
                    await appListEntries.First().LaunchAsync();
                    Thread.Sleep(1000);
                    while (ApplicationData.Current.LocalSettings.Values["AppState"].ToString() != "Open")
                    {
                        Thread.Sleep(1000);
                    }
                    await Program.SendToUWP("ID", Phone);
                }
                catch
                {
                    
                }
            }
        }
    }
}
