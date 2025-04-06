using Microsoft.Win32;
using NSwag.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceX.Items;
using VoiceX.Services;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    public sealed partial class FaxPage : Page
    {
        readonly WebService webService;
        public static ObservableDictionary<string, byte[]>? Files;
        ProfilePage profilePage;
        public FaxPage(ProfilePage profilePage)
        {
            this.InitializeComponent();
            webService = new WebService();
            Files = new ObservableDictionary<string, byte[]>();
            Files.CollectionChanged += Files_CollectionChanged!;
            this.profilePage = profilePage;
        }

        private async void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.NewItems != null)
                {
                    if (e.NewItems.Count != 0)
                    {
                        await Dispatcher.InvokeAsync(() => {
                            if (SelectFileViwe.Items.Count == 1)
                            {
                                SelectFileViwe.Items.Clear();
                                SelectFileViwe.Items.Insert(0, new FaxFileItem(this, false));
                            }
                            profilePage.MainFrame.Navigate(this);
                        });
                        foreach (var item in e.NewItems)
                        {
                            var file = (KeyValuePair<string, byte[]>)item;
                            await Dispatcher.InvokeAsync(() => SelectFileViwe.Items.Insert(0, new FaxFileItem(file, this)));
                        }
                    }
                }
            }
            catch
            {

            }
        }
        public async Task OpenFileSelector()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Choose Files",
                Filter = "PDF files (*.pdf)|*.pdf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Multiselect = false 
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await ParceFile(openFileDialog.FileNames);
            }
        }
        private async void SelectFileViwe_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                await ParceFile(files);
            }
        }
        private async Task ParceFile(string[] files)
        {
            foreach (string filePath in files.Where(f => f.Contains(".pdf")))
            {
                string fileName = Path.GetFileName(filePath); // Получаем только имя файла без пути
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath); // Читаем файл в байты
                long fileSize = new FileInfo(filePath).Length;
                var name = fileName + ";" + Math.Round((decimal)fileSize / 1000, 1).ToString();
                if (!Files!.Keys.Contains(name))
                {
                    Files.Add(name, fileBytes);
                }
                else
                {
                    int n = 1;
                    while (Files.Keys.Contains($"({n})" + name))
                    {
                        n++;
                    }

                    Files.Add($"({n})" + name, fileBytes);
                }
            }

        }
        private async Task SendFile()
        {
            List<string> success = new List<string>();
            if (!String.IsNullOrEmpty(FaxNumber.Text) && FaxNumber.Text != "Fax number")
            {
                if (!String.IsNullOrEmpty(FaxEmail.Text) && FaxEmail.Text != "Notify Email")
                {
                    List<FaxFileItem> faxFileItems = new List<FaxFileItem>();
                    foreach (var faxItem in SelectFileViwe.Items)
                    {
                        faxFileItems.Add((FaxFileItem)faxItem);
                    }
                    if (faxFileItems.Count != 0 && faxFileItems.Count() != 0)
                    {
                        ProfilePage.window!.LoadIcone.Visibility = Visibility.Visible;
                        foreach (var file in faxFileItems)
                        {
                            if (file.File.Value.Length != 0)
                            {
                                success.Add(await webService.PostToFax(App.AccountData!.Data.User_Data.UserID, "", new string[] { FaxNumber.Text }, file.File.Value, App.UserPbx!));
                                Files?.Remove(file.File.Key);
                            }
                        }
                        ProfilePage.window!.LoadIcone.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ProfilePage.window?.ShowError("Select Document");
                    }
                }
                else
                {
                    ProfilePage.window?.ShowError("Email Fild is Empty");
                }
            }
            else
            {
                ProfilePage.window?.ShowError("Number Fild is Empty");
            }
        }
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendFile();
        }
        private void SelectFileViwe_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

        }
        private void SelectFileViwe_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        public void RemoveFile(FaxFileItem faxFileItem)
        {
            if (faxFileItem != null)
            {
                SelectFileViwe.Items.Remove(faxFileItem);
                Files?.Remove(faxFileItem.File.Key);
                if (SelectFileViwe.Items.Count == 1)
                {
                    SelectFileViwe.Items.Clear();
                    SelectFileViwe.Items.Add(new FaxFileItem(this, true));
                }
            }
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            if (Files?.Count != 0)
            {
                try
                {
                    SelectFileViwe.Items.Clear();
                    SelectFileViwe.Items.Add(new FaxFileItem(this, false));
                    foreach (var file in Files!)
                    {
                        SelectFileViwe.Items.Insert(0, new FaxFileItem(file, this));
                    }
                }
                catch
                {
                    return;
                }

            }
            else
            {
                if (SelectFileViwe.Items.Count == 0)
                {
                    SelectFileViwe.Items.Add(new FaxFileItem(this, true));
                }
            }
        }

        private void FaxNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            var num = (System.Windows.Controls.TextBox)sender;
            if (num.Text == "Fax number")
            {
                num.Text = "";
                num.Foreground = new SolidColorBrush(Color.FromArgb(255, 92, 102, 189));
            }
        }

        private void FaxEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            var email = (System.Windows.Controls.TextBox)sender;
            if (email.Text == "Notify Email")
            {
                email.Text = "";
                email.Foreground = new SolidColorBrush(Color.FromArgb(255, 92, 102, 189));
            }
        }

        private void FaxNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            var num = (System.Windows.Controls.TextBox)sender;
            if(num.Text == "")
            {
                num.Text = "Fax number";
                num.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            }
        }

        private void FaxEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            var email = (System.Windows.Controls.TextBox)sender;
            if (email.Text == "")
            {
                email.Text = "Notify Email";
                email.Foreground = new SolidColorBrush(Color.FromArgb(255, 195, 195, 196));
            }
        }
    }
}
