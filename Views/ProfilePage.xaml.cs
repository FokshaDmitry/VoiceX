using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VoiceX.DAL.Context;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;

namespace VoiceX.Views
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Grid
    {
        public static List<Regex_note> regexNotes;
        readonly WebService webService;
        readonly DispatcherTimer timer;
        readonly AddDbContext addDbContext;
        public static Get_pauses getPauses;
        readonly ErrorService errorService;
        public ProfilePage()
        {
            this.InitializeComponent();

            webService = new WebService(App.userToken);
            //Context
            addDbContext = new AddDbContext();
            //Active session
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            regexNotes = new List<Regex_note>();
            errorService = new ErrorService(ControlMainGrid);
            this.SizeChanged += ControlPage_SizeChanged;
        }

        private void ControlPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {


        }
        private async void ControlPage_Loaded(object sender, RoutedEventArgs e)
        {
            
            try
            {
                //User RegEx
                //if (ApplicationData.Current.LocalSettings.Values["regexs"] != null)
                //{
                //    regexNotes = JsonConvert.DeserializeObject<List<Regex_note>>(ApplicationData.Current.LocalSettings.Values["regexs"].ToString());
                //}
            }
            catch
            {
                regexNotes = new List<Regex_note>();
            }
            // Include Auto Answer List
            //if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("AACallList"))
            //{
            //    if (DialpadPage.AutoAnswerNumbers != null)
            //    {
            //        DialpadPage.AutoAnswerNumbers = JsonConvert.DeserializeObject<List<string>>(ApplicationData.Current.LocalSettings.Values["AACallList"].ToString());
            //    }
            //    else
            //    {
            //        DialpadPage.AutoAnswerNumbers = new List<string>();
            //        DialpadPage.AutoAnswerNumbers = JsonConvert.DeserializeObject<List<string>>(ApplicationData.Current.LocalSettings.Values["AACallList"].ToString());
            //    }
            //}
            //if (!String.IsNullOrEmpty(result))
            //{
            //    errorService.ShowError(result);
            //}
            //general setting content
            //ContentControl.Content = new GeneralSettingPage();
            //if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("MyComputer"))
            //{
            //    if (!String.IsNullOrEmpty(ApplicationData.Current.LocalSettings.Values["MyComputer"].ToString()))
            //    {
            //        //if my computer true app work only one hour if it is not used
            //        App.MyComputer = ApplicationData.Current.LocalSettings.Values["MyComputer"].ToString() == "On";
            //    }
            //    else
            //    {
            //        App.MyComputer = false;
            //    }
            //}
            //else
            //{
            //    App.MyComputer = false;
            //}
            //if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("MicrophoneDevice"))
            //{
            //    //foreach (var device in CoreService.Instance.Core?.ExtendedAudioDevices)
            //    //{
            //    //    if (device.Id == ApplicationData.Current.LocalSettings.Values["MicrophoneDevice"].ToString())
            //    //    {
            //    //        CoreService.Instance.Core.DefaultInputAudioDevice = device;
            //    //    }
            //    //}
            //}
            //if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("AudioDevice"))
            //{
            //    //foreach (var device in CoreService.Instance.Core?.ExtendedAudioDevices)
            //    //{
            //    //    if (device.Id == ApplicationData.Current.LocalSettings.Values["AudioDevice"].ToString())
            //    //    {
            //    //        CoreService.Instance.Core.DefaultOutputAudioDevice = device;
            //    //    }
            //    //}
            //}
        }
        //HotKey, Fax, Systray receve
        
        //Timer out off app. Defolt one hour
        private async void Timer_Tick(object sender, object e)
        {
            //TimeSpan difference = DateTime.Now - App.timeOut;
            //if (difference.TotalHours > 1 && !App.MyComputer)
            //{
            //    localSettings.Values.Clear();
            //    CoreService.Instance.LogOut();
            //    timer.Stop();
            //    await addDbContext.DropDatabaseAsync();
            //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //    {
            //        rootFrame.Navigate(typeof(RegistrationPage), null, null);
            //    });
            //}
        }
        #region Navigete Button
        private async void Navigate_Click(object sender, RoutedEventArgs e)
        {
            //App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Contacts":
                    //await App.OpenWindow(typeof(ClientsPage), "");
                    break;
                case "Phone":
                    //Frame.Navigate(typeof(DialpadPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "History":
                    //Frame.Navigate(typeof(HistoryPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "Fax":
                    //Frame.Navigate(typeof(FaxPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "HotKeys":
                    //Frame.Navigate(typeof(HotKeyPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
            }
        }
        #endregion
        //General Page Navigate
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton filter = (RadioButton)sender;
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 253, 254, 255));

            if (filter.Name == "General")
            {
                if (GeneralCheck != null)
                {
                    GeneralCheck.Background = blueLine;
                    C2CCheck.Background = whiteLine;
                    AdditionChek.Background = whiteLine;
                }

                //ContentControl.Navigate(typeof(GeneralSettingPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if (filter.Name == "C2C")
            {
                //var NTrasform = ContentControl.Content.ToString() == "VoiceX.Views.ControlPages.GeneralSettingPage" ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
                if (C2CCheck != null)
                {
                    GeneralCheck.Background = whiteLine;
                    C2CCheck.Background = blueLine;
                    AdditionChek.Background = whiteLine;
                }
                //ContentControl.Navigate(typeof(ClickToCallPage), "", new SlideNavigationTransitionInfo() { Effect = NTrasform });
            }
            else if (filter.Name == "Addition")
            {
                if (AdditionChek != null)
                {
                    GeneralCheck.Background = whiteLine;
                    C2CCheck.Background = whiteLine;
                    AdditionChek.Background = blueLine;
                }
                //ContentControl.Navigate(typeof(AdditionPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
        private void Navigate_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top - 1, 0, 0);
        }

        private void Navigate_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top + 1, 0, 0);
        }
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Margin.Bottom == -50)
            {
                Menu.Margin = new Thickness(0, 0, 0, 0);
                Butter.Visibility = Visibility.Collapsed;
                Cross.Visibility = Visibility.Visible;
            }
            else
            {
                Menu.Margin = new Thickness(0, 0, 0, -50);
                Butter.Visibility = Visibility.Visible;
                Cross.Visibility = Visibility.Collapsed;
            }
        }
        private void ControlPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //App.timeOut = DateTime.Now;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            PausesFild.Visibility = Visibility.Collapsed;
        }

        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            if (getPauses == null)
            {
                getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                getPauses.ResponseData.Pauses = new List<Pause>();
                getPauses = await webService.GetPauses(App.AccountData.Data.Sip_Settings.Sip_username, App.UserPbx);
                if (getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, getPauses.ResponseData.Pause_active == 0));
                    foreach (var pause in getPauses.ResponseData.Pauses)
                    {
                        PauseList.Items.Add(new PauseItem(pause, pause.Id == getPauses.ResponseData.Pause_active));
                    }
                }
                else
                {
                    errorService.ShowWarning(getPauses.ResponseMessage);
                }
            }
            else
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, getPauses.ResponseData.Pause_active == 0));
                foreach (var pause in getPauses.ResponseData.Pauses)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == getPauses.ResponseData.Pause_active));
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
                    if (getPauses.ResponseData.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData.Data.Sip_Settings.Sip_username, id, App.UserPbx);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            getPauses.ResponseData.Pause_active = id;
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
    }
}
