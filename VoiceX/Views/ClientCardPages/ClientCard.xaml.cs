using System;
using System.Collections.Generic;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views.ControlPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.ClientCardPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClientCard : Page
    {
        public static string Number { get; set; }
        public static string UserName { get; set; }
        readonly ErrorService errorService;
        readonly WebService webService;
        public ClientCard()
        {
            this.InitializeComponent();
            errorService = new ErrorService(MainGrid);
            webService = new WebService(App.userToken);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!String.IsNullOrEmpty(e?.Parameter.ToString()))
            {
                var Params = e?.Parameter.ToString().Split(';');
                Number = Params[0];
                UserName = Params[1];
            }
            CardContent.Navigate(typeof(InfoContent), CardContent);
        }

        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            if (ProfilePage.getPauses == null)
            {
                ProfilePage.getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                ProfilePage.getPauses.ResponseData.Pauses = new List<Pause>();
                ProfilePage.getPauses = await webService.GetPauses(App.AccountData.Data.Sip_Settings.Sip_username, App.UserPbx);
                if (ProfilePage.getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ProfilePage.getPauses.ResponseData.Pause_active == 0));
                    foreach (var pause in ProfilePage.getPauses.ResponseData.Pauses)
                    {
                        PauseList.Items.Add(new PauseItem(pause, pause.Id == ProfilePage.getPauses.ResponseData.Pause_active));
                    }
                }
                else
                {
                    errorService.ShowWarning(ProfilePage.getPauses.ResponseMessage);
                }
            }
            else
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ProfilePage.getPauses.ResponseData.Pause_active == 0));
                foreach (var pause in ProfilePage.getPauses.ResponseData.Pauses)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == ProfilePage.getPauses.ResponseData.Pause_active));
                }
            }
            PausesFild.Visibility = Visibility.Visible;
        }

        private void PauseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            foreach (var item in list.Items)
            {
                var pause = (PauseItem)item;
                pause.SelectChange(pause == list.SelectedItem);
            }

        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pause = (PauseItem)PauseList.SelectedItem;
                if (pause != null)
                {
                    int id = pause.pause.Id;
                    if (ProfilePage.getPauses.ResponseData.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData.Data.Sip_Settings.Sip_username, id, App.UserPbx);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            ProfilePage.getPauses.ResponseData.Pause_active = id;
                        }
                        else
                        {
                            errorService.ShowWarning(result.ResponseMessage);
                            PauseList.SelectedIndex = -1;
                        }
                    }
                }

            }
            catch
            {
                PausesFild.Visibility = Visibility.Collapsed;
                return;
            }
            PausesFild.Visibility = Visibility.Collapsed;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            PausesFild.Visibility = Visibility.Collapsed;
        }
        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
    }
}
