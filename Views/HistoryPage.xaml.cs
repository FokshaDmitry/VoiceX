using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.DAL.Context;
using VoiceX.DAL.Entity;
using VoiceX.Interfeces;
using VoiceX.Items;
using VoiceX.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    public sealed partial class HistoryPage : Page, IMoreItems  
    {
        readonly AddDbContext addDbContext;
        readonly List<HistoryNote> histories;
        RadioButton filter;
        readonly WebService webService;
        
        public HistoryPage()
        {
            this.InitializeComponent();
            addDbContext = new AddDbContext();
            histories = new List<HistoryNote>();
            addDbContext = new AddDbContext();
            filter = new RadioButton();
            webService = new WebService();
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
            else if (filter.Name == "IgnoreCall" && (historyNote.StatusCall == Enums.StatusCall.Ignore || historyNote.StatusCall == Enums.StatusCall.IncomeIgnore))
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

        private void HistoryPage_Loaded(object sender, RoutedEventArgs e)
        {         
            List<HistoryNotes> historyNotes = addDbContext.GetNotes(50);
            HistoryList.Items.Clear();
            histories.Clear();
            var groupNote = historyNotes.GroupBy(h => h.EndDialog.Date).OrderByDescending(h => h.Key);
            foreach (var group in groupNote)
            {
                HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                foreach (var Note in group.OrderByDescending(h => h.EndDialog))
                {
                    string textTime = "";
                    if (Note.StartDialog != DateTime.MinValue)
                    {
                        var time = Note.EndDialog - Note.StartDialog;
                        textTime = time.Minutes == 0 ? $"({time.Seconds}s.)" : $"({time.Minutes}m. {time.Seconds}s.)";
                    }
                    else
                    {
                        textTime = "(0s.)";
                    }
                    var note = new HistoryNote(Note.Name, Note.Phone, Note.EndDialog, textTime, Note.StatusCall);
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
                foreach (var group in histories.Where(h => h.statusCall == Enums.StatusCall.Ignore || h.statusCall == Enums.StatusCall.IncomeIgnore).GroupBy(h => h.dateCall.Date).OrderByDescending(h => h.Key))
                {
                    HistoryList.Items.Add(new HeadingContactList(group.Key.ToString("d.M.yyyy")));
                    foreach (var note in group.OrderByDescending(g => g.dateCall))
                    {
                        HistoryList.Items.Add(note);
                    }
                }
            }
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
