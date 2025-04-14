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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    public sealed partial class HistoryPage : Page, IMoreItems  
    {
        readonly AddDbContext addDbContext;
        RadioButton filter;
        public HistoryPage()
        {
            this.InitializeComponent();
            addDbContext = new AddDbContext();
            addDbContext = new AddDbContext();
            filter = new RadioButton();
            AddDbContext.ChangeHystory += AddDbContext_ChangeHystory;
        }

        private void AddDbContext_ChangeHystory(HistoryNotes historyNote)
        {
            var time = historyNote.EndDialog - historyNote.StartDialog;
            string textTime = time.Minutes == 0 ? $"({time.Seconds}s.)" : $"({time.Minutes}m. {time.Seconds}s.)";
            var note = new HistoryNote(historyNote.Name!, historyNote.Phone!, historyNote.StartDialog, textTime, historyNote.StatusCall);
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
            FillListBox(historyNotes, 0);
            filter.Name = "AllCall";
        }
        public void FillListBox(List<HistoryNotes> historyNotes, int quality)
        {
            HistoryList.Items.Clear();
            var groupNote = historyNotes.GroupBy(h => h.EndDialog.Date).OrderByDescending(h => h.Key);
            foreach (var group in groupNote)
            {
                quality++;
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
                    var note = new HistoryNote(Note.Name!, Note.Phone!, Note.EndDialog, textTime, Note.StatusCall);
                    HistoryList.Items.Add(note);
                }
            }
            if (HistoryList.Items.Count == quality + 50)
            {
                HistoryList.Items.Add(new MoreItems(this));
            }
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

                List<HistoryNotes> historyNotes = addDbContext.GetNotes(50, Enums.StatusCall.Outgoing);
                FillListBox(historyNotes, 0);
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

                List<HistoryNotes> historyNotes = addDbContext.GetNotes(50, Enums.StatusCall.Incoming);
                FillListBox(historyNotes, 0);
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

                List<HistoryNotes> historyNotes = addDbContext.GetNotes(50);
                FillListBox(historyNotes, 0);
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

                List<HistoryNotes> historyNotes = addDbContext.GetNotes(50, Enums.StatusCall.Ignore);
                FillListBox(historyNotes, 0);
            }
        }

        public void AddMoreNotes()
        {
            int currentItems = HistoryList.Items.Count;
            if (filter.Name == "OutCall")
            {
                List<HistoryNotes> historyNotes = addDbContext.GetNotes(currentItems + 50, Enums.StatusCall.Outgoing);
                FillListBox(historyNotes, currentItems);
            }
            else if (filter.Name == "InCall")
            {
                List<HistoryNotes> historyNotes = addDbContext.GetNotes(currentItems + 50, Enums.StatusCall.Incoming);
                FillListBox(historyNotes, currentItems);
            }
            else if (filter.Name == "AllCall")
            {
                List<HistoryNotes> historyNotes = addDbContext.GetNotes(currentItems + 50);
                FillListBox(historyNotes, currentItems);
            }
            else if (filter.Name == "IgnoreCall")
            {
                List<HistoryNotes> historyNotes = addDbContext.GetNotes(currentItems + 50, Enums.StatusCall.Ignore);
                FillListBox(historyNotes, currentItems);
            }
        }
    }
}
