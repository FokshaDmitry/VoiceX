using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.DAL.Context;
using VoiceX.Enums;
using VoiceX.Services;
using VoiceX.Views;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HotKeyItem : ListBoxItem
    {
        AddDbContext addDbContext;
        public string HotKeyName;
        public string HotKeyPhone;
        private ListBox hotkeyList;
        Guid hotKeyGuid;
        public HotKeyItem(Guid hotKeyGuid, string Name, string Phone, ListBox hotkeyList)
        {
            this.InitializeComponent();
            this.Phone.Text = Phone;
            this.UserName.Text = Name;
            this.FirstWord.Text = Name.ToUpper().Substring(0, 1);
            HotKeyName = Name;
            HotKeyPhone = Phone;
            this.hotkeyList = hotkeyList;
            this.hotKeyGuid = hotKeyGuid;
            addDbContext = new AddDbContext();
        }
        public void SetState(HotKeyStatus hotKeyStatus)
        {
            switch (hotKeyStatus)
            {
                case HotKeyStatus.Online:
                    this.Activ.Background = new SolidColorBrush(Color.FromArgb(255, 76, 176, 78));
                    break;
                case HotKeyStatus.Offline:
                    this.Activ.Background = new SolidColorBrush(Color.FromArgb(255, 243, 30, 56));
                    break;
                case HotKeyStatus.Busy:
                    this.Activ.Background = new SolidColorBrush(Color.FromArgb(255, 255, 217, 0));
                    break;
                default:
                    break;
            }
        }
        private async void Trash_Click(object sender, RoutedEventArgs e)
        {
            hotkeyList.Items.Remove(this);
            await addDbContext.RemoveHotKeyUserAsync(hotKeyGuid);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var call = CoreService.Instance.MakeCall(HotKeyPhone, App.AccountData?.Data.Sip_Settings.Sip_server!);
            if (call == null)
            {
                ProfilePage.window?.ShowError("Call not create. Please check connection and audio.");
            }
        }
    }
}
