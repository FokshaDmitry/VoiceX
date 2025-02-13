using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using System.Linq;
using VoiceX.DAL.Context;
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClickToCallPage : Page
    {
        private int MainTab;
        string KeyLetter;
        private bool toggele;
        public RegExItem? SelectItem { get; set; }
        LocalStoreService storeService;
        AddDbContext dbContext;
        public ClickToCallPage()
        {
            this.InitializeComponent();
            MainTab = 3;
            KeyLetter = "S";
            toggele = false;
            storeService = new LocalStoreService();
            dbContext = new AddDbContext();
        }
        private async void ClickToCall_Loaded(object sender, RoutedEventArgs e)
        {
            RegExList.Items.Clear();
            var mainTab = await storeService.LoadDataAsync("mainkey");
            var mainKey = await storeService.LoadDataAsync("key");
            if(!String.IsNullOrEmpty(mainTab) && !String.IsNullOrEmpty(mainKey))
            {
                MainTab = int.Parse(mainTab);
                KeyLetter = mainKey;
                switch (MainTab)
                {
                    case 1:
                        HotKeyBox.Text = "ALT + " + KeyLetter;
                        break;
                    case 2:
                        HotKeyBox.Text = "CTRL + " + KeyLetter;
                        break;
                    case 4:
                        HotKeyBox.Text = "Shift + " + KeyLetter;
                        break;
                    default:
                        break;
                }
                foreach (var note in ProfilePage.regexNotes)
                {
                    RegExList.Items.Insert(0, new RegExItem(this, note.Search, note.Replace, note.Check));
                }
            }
            else
            {
                switch (MainTab)
                {
                    case 1:
                        HotKeyBox.Text = "ALT + " + KeyLetter;
                        break;
                    case 2:
                        HotKeyBox.Text = "CTRL + " + KeyLetter;
                        break;
                    case 4:
                        HotKeyBox.Text = "Shift + " + KeyLetter;
                        break;
                    default:
                        break;
                }
            }
            RegExList.Items.Add(new RegExItem(this));

        }
        public async Task UpdateRegExList()
        {
            ProfilePage.regexNotes?.Clear();
            foreach (RegExItem regExItem in RegExList.Items.Cast<RegExItem>())
            {
                if (!String.IsNullOrEmpty(regExItem.SearchText))
                {
                    ProfilePage.regexNotes?.Add(new Regex_note() { Check = regExItem.Check, Replace = regExItem.ReplaceText, Search = regExItem.SearchText });
                    
                }
            }
            await storeService.SaveDataAsync("regexs", JsonConvert.SerializeObject(ProfilePage.regexNotes));
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
        private async void HotKeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    HotKeyBox.Text = "CTRL + ";
                    MainTab = 2;
                    toggele = true;
                    break;
                case Key.RightCtrl:
                    HotKeyBox.Text = "CTRL + ";
                    MainTab = 2;
                    toggele = true;
                    break;
                case Key.LeftShift:
                    HotKeyBox.Text = "Shift + ";
                    MainTab = 4;
                    toggele = true;
                    break;
                case Key.RightShift:
                    HotKeyBox.Text = "Shift + ";
                    MainTab = 4;
                    toggele = true;
                    break;
                default:
                    if (toggele)
                    {
                        HotKeyBox.Text += e.Key.ToString();
                        KeyLetter = e.Key.ToString();
                        toggele = false;
                        await storeService.SaveDataAsync("mainkey", MainTab.ToString());
                        await storeService.SaveDataAsync("key", KeyLetter);
                    }
                    else
                    {
                        HotKeyBox.Text = e.Key.ToString() + " (invalid)";
                    }
                    break;
            }
        }
        private void HotKeyBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    if (toggele)
                    {
                        HotKeyBox.Text = "CTRL + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case Key.RightCtrl:
                    if (toggele)
                    {
                        HotKeyBox.Text = "CTRL + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case Key.LeftShift:
                    if (toggele)
                    {
                        HotKeyBox.Text = "Shift + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
                case Key.RightShift:
                    if (toggele)
                    {
                        HotKeyBox.Text = "Shift + (invalid)";
                        MainTab = 0;
                        toggele = false;
                    }
                    break;
            }
            
        }
        private async void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            List<RegExItem> regExItems = new List<RegExItem>();
            foreach (var RegEx in RegExList.Items)
            {
                regExItems.Add((RegExItem)RegEx);
            }
            var ser = new string[] { " ", "-", "+972", "(", ")" };
            foreach (var item in ser)
            {
                if (!regExItems.Select(r => r.SearchText).Contains(item))
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
        public void AddEmpty(RegExItem regEx)
        {
            if (RegExList.Items.Cast<RegExItem>().Last() == regEx)
            {
                RegExList.Items.Insert(0, new RegExItem(this, "", "", false));
            }

        }
        private void Navigate_PointerEntered(object sender, RoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, Img.Margin.Top - 1, Img.Margin.Right, 0);
        }

        private void Navigate_PointerExited(object sender, RoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(0, Img.Margin.Top + 1, Img.Margin.Right, 0);
        }
        private void RegExList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
