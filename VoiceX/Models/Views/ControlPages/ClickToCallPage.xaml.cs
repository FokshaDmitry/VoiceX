using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceX.Items;
using VoiceX.Models;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClickToCallPage : Page
    {
        private int MainTab;
        string Key;
        private bool toggele;
        public RegExItem SelectItem { get; set; }
        public ClickToCallPage()
        {
            this.InitializeComponent();
            MainTab = 1;
            Key = "S";
            toggele = false;
        }
        private void ClickToCall_Loaded(object sender, RoutedEventArgs e)
        {
            if(ApplicationData.Current.LocalSettings.Values.Keys.Contains("mainkey"))
            {
                MainTab = (int)ApplicationData.Current.LocalSettings.Values["mainkey"];
                Key = ApplicationData.Current.LocalSettings.Values["key"].ToString();
                switch (MainTab)
                {
                    case 1:
                        HotKeyBox.Text = "ALT + " + Key;
                        break;
                    case 2:
                        HotKeyBox.Text = "CTRL + " + Key;
                        break;
                    case 4:
                        HotKeyBox.Text = "Shift + " + Key;
                        break;
                    default:
                        break;
                }
                foreach (var note in ControlPage.regexNotes)
                {
                    RegExList.Items.Insert(0, new RegExItem(this, note.Search, note.Replace, note.Check));
                }
            }
            else
            {
                switch (MainTab)
                {
                    case 1:
                        HotKeyBox.Text = "ALT + " + Key;
                        break;
                    case 2:
                        HotKeyBox.Text = "CTRL + " + Key;
                        break;
                    case 4:
                        HotKeyBox.Text = "Shift + " + Key;
                        break;
                    default:
                        break;
                }
            }
            RegExList.Items.Add(new RegExItem(this));

        }
        public async Task UpdateRegExList()
        {
            ControlPage.regexNotes.Clear();
            foreach (RegExItem regExItem in RegExList.Items.Cast<RegExItem>())
            {
                if (!String.IsNullOrEmpty(regExItem.SearchText))
                {
                    ControlPage.regexNotes.Add(new Regex_note() { Check = regExItem.Check, Replace = regExItem.ReplaceText, Search = regExItem.SearchText });
                }
            }
            AddToLocalStore(MainTab, Key, ControlPage.regexNotes);
            await ChangeParamsInSysTray();
        }
        
        public void UpMainItem(RegExItem regExItem)
        {
            RegExList.Items.Insert(0, regExItem);
        }
        public void UpBotton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectItem != null)
            {
                MoveItem(-1);
            }
        }
        private void MoveItem(int direction)
        {
            int index = RegExList.Items.IndexOf(SelectItem);
            // Calculate new index using move direction
            if (index == -1)
                return;
            int newIndex = index + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= RegExList.Items.Count)
                return; // Index out of range - nothing to do

            // Removing removable element
            RegExList.Items.Remove(SelectItem);
            // Insert it in new position
            RegExList.Items.Insert(newIndex, SelectItem);
        }
        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (RegExList.Items.Count > 1)
            {
                if (SelectItem != null)
                {
                    RegExList.Items.Remove(SelectItem);
                    await UpdateRegExList();
                }
            }
        }
        private async void HotKeyBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Menu:
                    HotKeyBox.Text = "ALT + ";
                    MainTab = 1;
                    toggele = true;
                    break;
                case VirtualKey.LeftMenu:
                    HotKeyBox.Text = "ALT + ";
                    MainTab = 1;
                    toggele = true;
                    break;
                case VirtualKey.RightMenu:
                    HotKeyBox.Text = "ALT + ";
                    MainTab = 1;
                    toggele = true;
                    break;
                case VirtualKey.Control:
                    HotKeyBox.Text = "CTRL + ";
                    MainTab = 2;
                    toggele = true;
                    break;
                case VirtualKey.LeftControl:
                    HotKeyBox.Text = "CTRL + ";
                    MainTab = 2;
                    toggele = true;
                    break;
                case VirtualKey.RightControl:
                    HotKeyBox.Text = "CTRL + ";
                    MainTab = 2;
                    toggele = true;
                    break;
                case VirtualKey.Shift:
                    HotKeyBox.Text = "Shift + ";
                    MainTab = 4;
                    toggele = true;
                    break;
                case VirtualKey.LeftShift:
                    HotKeyBox.Text = "Shift + ";
                    MainTab = 4;
                    toggele = true;
                    break;
                case VirtualKey.RightShift:
                    HotKeyBox.Text = "Shift + ";
                    MainTab = 4;
                    toggele = true;
                    break;
                default:
                    if (toggele)
                    {
                        HotKeyBox.Text += e.OriginalKey.ToString();
                        Key = e.OriginalKey.ToString();
                        AddToLocalStore(MainTab, Key, ControlPage.regexNotes);
                        await ChangeParamsInSysTray();
                        toggele = false;
                    }
                    else
                    {
                        HotKeyBox.Text = e.OriginalKey.ToString() + " (invalid)";
                    }
                    break;
            }
        }
        private void HotKeyBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Menu:
                    if (toggele)
                    {
                        HotKeyBox.Text = "ALT + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case VirtualKey.LeftMenu:
                    if (toggele)
                    {
                        HotKeyBox.Text = "ALT + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                        
                    break;
                case VirtualKey.RightMenu:
                    if (toggele)
                    {
                        HotKeyBox.Text = "ALT + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case VirtualKey.Control:
                    if (toggele)
                    {
                        HotKeyBox.Text = "CTRL + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case VirtualKey.LeftControl:
                    if (toggele)
                    {
                        HotKeyBox.Text = "CTRL + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case VirtualKey.RightControl:
                    if (toggele)
                    {
                        HotKeyBox.Text = "CTRL + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case VirtualKey.Shift:
                    if (toggele)
                    {
                        HotKeyBox.Text = "Shift + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case VirtualKey.LeftShift:
                    if (toggele)
                    {
                        HotKeyBox.Text = "Shift + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case VirtualKey.RightShift:
                    if (toggele)
                    {
                        HotKeyBox.Text = "Shift + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
            }
            
        }
        //Reload descktop extation
        private async Task ChangeParamsInSysTray()
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
               await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
        }

        public void AddToLocalStore(int MainKey, string Key, List<Regex_note> regExNote)
        {
            ApplicationData.Current.LocalSettings.Values["mainkey"] = MainKey;
            ApplicationData.Current.LocalSettings.Values["key"] = Key;
            ApplicationData.Current.LocalSettings.Values["regexs"] =  JsonConvert.SerializeObject(regExNote);
            
        }
        private async void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            List<RegExItem> regExItems = new List<RegExItem>();
            foreach (var RegEx in RegExList.Items)
            {
                regExItems.Add((RegExItem)RegEx);
            }
            var ser = new string[] {" ", "-", "+972", "(",")" };
            foreach (var item in ser)
            {
                if(!regExItems.Select(r => r.SearchText).Contains(item))
                {
                    if (item == "+972")
                    {
                        RegExList.Items.Insert(0, new RegExItem(this, item, "0", false));
                    }
                    else 
                    {
                        RegExList.Items.Insert(0, new RegExItem(this, item, "", false));
                    }
                }


            }
            await UpdateRegExList();
        }

        private void ClickToCall_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
        public void AddEmpty(RegExItem regEx)
        {
            if (RegExList.Items.Last() == regEx)
            {
                RegExList.Items.Insert(0, new RegExItem(this, "", "", false));
            }

        }
        private void Navigate_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, Img.Margin.Top - 1, Img.Margin.Right, 0);
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void Navigate_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, Img.Margin.Top + 1, Img.Margin.Right, 0);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void RegExList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if (SelectItem != null)
            {
                MoveItem(1);
            }
        }
    }
}
