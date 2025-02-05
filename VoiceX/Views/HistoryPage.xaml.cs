using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VoiceX.DAL.Context;
using VoiceX.DAL.Entity;
using VoiceX.Interfeces;
using VoiceX.Items;
using VoiceX.Models;
using VoiceX.Services;
using VoiceX.Views.ClientPages;
using VoiceX.Views.ControlPages;
using VoiceX.Views.PhonePages;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page, IMoreItems  
    {
        readonly AddDbContext addDbContext;
        readonly List<HistoryNote> histories;
        RadioButton filter;
        readonly WebService webService;
        readonly ErrorService errorService;
        
        public HistoryPage()
        {
            this.InitializeComponent();
            addDbContext = new AddDbContext();
            histories = new List<HistoryNote>();
            addDbContext = new AddDbContext();
            filter = new RadioButton();
            webService = new WebService(App.userToken);
            errorService = new ErrorService(MainGrid);
            this.SizeChanged += HistoryPage_SizeChanged;
            AddDbContext.ChangeHystory += AddDbContext_ChangeHystory;
        }

        private void AddDbContext_ChangeHystory(HistoryNotes historyNote)
        {
            var time = historyNote.EndDialog - historyNote.StartDialog;
            string textTime = time.Minutes == 0 ? $"({time.Seconds}s.)" : $"({time.Minutes}m. {time.Seconds}s.)";
            var note = new HistoryNote(historyNote.Name, historyNote.Phone, historyNote.StartDialog, textTime, historyNote.StatusCall);
            if (filter.Name == "OutCall" && historyNote.StatusCall == Enums.StatusCall.Outgoing)
            {
                if (HistoryList.Items.Count == 0)
                {
                    HistoryList.Items.Add(note);
                }
                else
                {
                    HistoryList.Items.Insert(1, note);
                }
            } 
            else if (filter.Name == "InCall" && historyNote.StatusCall == Enums.StatusCall.Incoming)
            {
                if (HistoryList.Items.Count == 0)
                {
                    HistoryList.Items.Add(note);
                }
                else
                {
                    HistoryList.Items.Insert(1, note);
                }
            }
            else if (filter.Name == "IgnoreCall" && historyNote.StatusCall == Enums.StatusCall.Ignore)
            {
                if (HistoryList.Items.Count == 0)
                {
                    HistoryList.Items.Add(note);
                }
                else
                {
                    HistoryList.Items.Insert(1, note);
                }
            }
            else if (filter.Name == "AllCall")
            {
                if (HistoryList.Items.Count == 0)
                {
                    HistoryList.Items.Add(note);
                }
                else
                {
                    HistoryList.Items.Insert(1, note);
                }
            }
            
        }

        private void HistoryPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void HistoryPage_Loaded(object sender, RoutedEventArgs e)
        {         
            List<HistoryNotes> historyNotes = addDbContext.GetNotes(50);
            HistoryList.Items.Clear();
            histories.Clear();
            var groupNote = historyNotes.GroupBy(h => h.StartDialog.Date).OrderByDescending(h => h.Key);
            foreach (var group in groupNote)
            {
                HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                foreach (var Note in group.OrderByDescending(h => h.StartDialog))
                {
                    var time = Note.EndDialog - Note.StartDialog;
                    string textTime = time.Minutes == 0 ? $"({time.Seconds}s.)" : $"({time.Minutes}m. {time.Seconds}s.)";

                    var note = new HistoryNote(Note.Name, Note.Phone, Note.StartDialog, textTime, Note.StatusCall);
                    HistoryList.Items.Add(note);
                    histories.Add(note);
                }
            }
            if (histories.Count == 50)
            {
                HistoryList.Items.Add(new MoreItems(this));
            }
            filter.Name = "AllCall";
        }
        #region Navigate
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
        private void Navigate_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top - 1, 0, 0);
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }

        private void Navigate_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var Img = (Button)sender;
            Img.Margin = new Thickness(Img.Margin.Left, Img.Margin.Top + 1, 0, 0);
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private async void Navigate_Click(object sender, RoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Control":
                    Frame.Navigate(typeof(ProfilePage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    
                    break;
                case "Phone":
                    Frame.Navigate(typeof(DialpadPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "Contacts":
                    await App.OpenWindow(typeof(ClientsPage), "").ConfigureAwait(true);
                    break;
                case "Fax":
                    Frame.Navigate(typeof(FaxPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
                case "HotKeys":
                    Frame.Navigate(typeof(HotKeyPage), "", new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                    break;
            }
        }
        #endregion
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            filter = (RadioButton)sender;
            if (HistoryList == null)
            {
                return;
            }
            var blueLine = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            var textCalor = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            var darkTextColor = new SolidColorBrush(Color.FromArgb(255, 37, 36, 34));
            var whiteLine = new SolidColorBrush(Color.FromArgb(255, 245, 246, 247));
            HistoryList.Items.Clear();
            if (filter.Name == "OutCall")
            {
                AllText.Foreground = textCalor;
                InText.Foreground = textCalor;
                OutText.Foreground = darkTextColor;
                MinusText.Foreground = textCalor;

                
                CheckOut.Background = blueLine;
                ChekIn.Background = whiteLine;
                CheckAll.Background = whiteLine;
                ChekIgnore.Background = whiteLine;
                foreach (var group in histories.Where(h => h.statusCall == Enums.StatusCall.Outgoing).GroupBy(h => h.dateCall.Date).OrderByDescending(h => h.Key))
                {
                    HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                    foreach (var note in group.OrderByDescending(g => g.dateCall))
                    {
                        HistoryList.Items.Add(note);
                    }
                }
            }
            else if(filter.Name == "InCall")
            {
                AllText.Foreground = textCalor;
                InText.Foreground = darkTextColor;
                OutText.Foreground = textCalor;
                MinusText.Foreground = textCalor;

                CheckOut.Background = whiteLine;
                ChekIn.Background = blueLine;
                CheckAll.Background = whiteLine;
                ChekIgnore.Background = whiteLine;
                foreach (var group in histories.Where(h => h.statusCall == Enums.StatusCall.Incoming).GroupBy(h => h.dateCall.Date).OrderByDescending(h => h.Key))
                {
                    HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                    foreach (var note in group.OrderByDescending(g => g.dateCall))
                    {
                        HistoryList.Items.Add(note);
                    }
                }
            }
            else if(filter.Name == "AllCall")
            {
                AllText.Foreground = darkTextColor;
                InText.Foreground = textCalor;
                OutText.Foreground = textCalor;
                MinusText.Foreground = textCalor;

                CheckOut.Background = whiteLine;
                ChekIn.Background = whiteLine;
                CheckAll.Background = blueLine;
                ChekIgnore.Background = whiteLine;
                foreach (var group in histories.GroupBy(h => h.dateCall.Date).OrderByDescending(h => h.Key))
                {
                    HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                    foreach (var note in group.OrderByDescending(g => g.dateCall))
                    {
                        HistoryList.Items.Add(note);
                    }
                }
            }
            else if (filter.Name == "IgnoreCall")
            {
                AllText.Foreground =  textCalor;
                InText.Foreground =   textCalor;
                OutText.Foreground = textCalor;
                MinusText.Foreground = darkTextColor;

                CheckOut.Background = whiteLine;
                ChekIn.Background = whiteLine;
                CheckAll.Background = whiteLine;
                ChekIgnore.Background = blueLine;
                foreach (var group in histories.Where(h => h.statusCall == Enums.StatusCall.Ignore).GroupBy(h => h.dateCall.Date).OrderByDescending(h => h.Key))
                {
                    HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                    foreach (var note in group.OrderByDescending(g => g.dateCall))
                    {
                        HistoryList.Items.Add(note);
                    }
                }
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            PausesFild.Visibility = Visibility.Collapsed;
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
        private void Cursor_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Arrow;
        }
        private void Cursor_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = App.Hand;
        }
        private void HistoryPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }

        public void AddMoreNotes()
        {
            int currentItems = histories.Count;
            List<HistoryNotes> historyNotes = addDbContext.GetNotes(currentItems + 50);
            HistoryList.Items.Clear();
            histories.Clear();
            var groupNote = historyNotes.GroupBy(h => h.StartDialog.Date).OrderByDescending(h => h.Key);
            foreach (var group in groupNote)
            {
                HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                foreach (var Note in group.OrderByDescending(h => h.StartDialog))
                {
                    var time = Note.EndDialog - Note.StartDialog;
                    string textTime = time.Minutes == 0 ? $"({time.Seconds}s.)" : $"({time.Minutes}m. {time.Seconds}s.)";

                    var note = new HistoryNote(Note.Name, Note.Phone, Note.StartDialog, textTime, Note.StatusCall);
                    HistoryList.Items.Add(note);
                    histories.Add(note);
                }
            }
            if (histories.Count == currentItems + 50)
            {
                HistoryList.Items.Add(new MoreItems(this));
            }
        }
    }
}
