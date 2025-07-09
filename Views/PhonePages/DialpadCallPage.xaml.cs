using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VoiceX.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views.PhonePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public sealed partial class DialpadCallPage : Page
    {
        private bool isRightButtonDown = false;
        private DispatcherTimer rightClickTimer;
        String title;
        public DialpadCallPage()
        {
            this.InitializeComponent();
            // Инициализация таймера
            rightClickTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Интервал между событиями (100 мс)
            };
            rightClickTimer.Tick += RightClickTimer_Tick;
            DataObject.AddPastingHandler(NumberFild, OnTextBoxPasting);
            title = "";
        }
        private void OnTextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string pastedText = e.DataObject.GetData(DataFormats.Text) as string;
                if (!String.IsNullOrEmpty(pastedText))
                {
                    if (pastedText.Length >= 8 && pastedText.Length <= 10)
                    {
                        if (pastedText[0] != '0')
                        {
                            NumberFild.Text = "0" + pastedText;
                            e.CancelCommand();
                        }
                    }
                }
            }
        }
        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(NumberFild.Text))
            {
                return;
            }
            CallNumber(NumberFild.Text);
        }
        private void CallNumber(string phone)
        {
            if (CoreService.activeCall == null)
            {
                foreach (var regex in ProfilePage.regexNotes?.Where(r => r.Check)!)
                {
                    phone = phone.Replace(regex.Search!, regex.Replace);
                }
                phone = Regex.Replace(phone, @"[^0-9*#]", "");
                try
                {
                    CoreService.Instance.MakeCall(phone, App.AccountData?.Data.Sip_Settings.Sip_server!);
                }
                catch
                {
                    ProfilePage.window!.ShowError("Microphone not found");
                    return;
                }
                NumberFild.Text = "";
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            NumberFild.Text += b.Content;
        }

        private void NumberFild_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (String.IsNullOrEmpty(NumberFild.Text))
                {
                    return;
                }
                CallNumber(NumberFild.Text);
            }
        }
        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(NumberFild.Text))
            {
                NumberFild.Text = NumberFild.Text.Substring(0, NumberFild.Text.Length - 1);
            }
        }

        private void NumberFild_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(NumberFild.Text))
                {
                    if (Backspace != null && Backspace.Visibility != Visibility.Collapsed)
                    {
                        Backspace.Visibility = Visibility.Collapsed;
                        isRightButtonDown = false;
                        NumberFild.Text = title;
                        NumberFild.Foreground = new SolidColorBrush(Color.FromArgb(255, 194, 194, 195));
                    }
                }
                else if(NumberFild.Text != title)
                {
                    if (Backspace != null && Backspace.Visibility != Visibility.Visible)
                    {
                        Backspace.Visibility = Visibility.Visible;
                        NumberFild.Text = NumberFild.Text.Replace(title, "");
                        NumberFild.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    }
                    string phone = NumberFild.Text;
                    if (!String.IsNullOrEmpty(phone))
                    {
                        foreach (var regex in ProfilePage.regexNotes?.Where(r => r.Check)!)
                        {
                            phone = phone.Replace(regex.Search!, regex.Replace);
                        }
                        phone = Regex.Replace(phone, @"[^0-9*#]", "");
                        NumberFild.Text = phone;
                    }
                }
                NumberFild.CaretIndex = NumberFild.Text.Length;

                // Предотвращаем стандартное поведение
                e.Handled = true;
            }
            catch
            {
                return;
            }
        }

        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            NumberFild.Focus();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            title = this.TryFindResource("m_EnterPhoneNumber") as String;
            NumberFild.Text = title;
            NumberFild.Focus();
        }

        private void Backspace_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Thread.Sleep(100);
            if (!isRightButtonDown)
            {
                isRightButtonDown = true; 
                rightClickTimer.Start(); // Запуск таймера
            }
        }

        private void RightClickTimer_Tick(object? sender, EventArgs e)
        {
            if (isRightButtonDown)
            {
                if (!String.IsNullOrEmpty(NumberFild.Text))
                {
                    NumberFild.Text = NumberFild.Text.Substring(0, NumberFild.Text.Length - 1);
                }
            }
        }

        private void Backspace_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isRightButtonDown = false;
            rightClickTimer.Stop();
        }
        private void NumberFild_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Delete)
            {
                if (!isRightButtonDown)
                {
                    if (!String.IsNullOrEmpty(NumberFild.Text))
                    {
                        NumberFild.Text = "";
                    }
                }
            }
        }

        private void NumberFild_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Добавляем текст в конец
                textBox.Text += e.Text;

                // Устанавливаем курсор в конец
                textBox.CaretIndex = textBox.Text.Length;

                // Предотвращаем стандартное поведение
                e.Handled = true;
            }
        }
    }
}
