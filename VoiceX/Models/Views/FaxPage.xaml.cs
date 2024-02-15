using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VoiceX.Items;
using VoiceX.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.WindowManagement.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;
using System.Linq;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.DataTransfer;
using NSwag.Collections;
using Windows.UI.Core;
using VoiceX.Models;
using VoiceX.Views.PhonePages;
using VoiceX.Views.ControlPages;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaxPage : Page
    {

        readonly FileOpenPicker fileOpenPicker;
        IReadOnlyList<StorageFile> files;
        readonly WebService webService;
        readonly ErrorService errorService;
        public static ObservableDictionary<string, byte[]> Files = new ObservableDictionary<string, byte[]>();
        public FaxPage()
        {
            this.InitializeComponent();
            fileOpenPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            fileOpenPicker.FileTypeFilter.Add(".pdf");
            webService = new WebService(App.userToken);
            Files.CollectionChanged += Files_CollectionChanged;
            this.Unloaded += App.RootFrame_Unloaded;
            this.SizeChanged += FaxPage_SizeChanged;
            errorService = new ErrorService(MainGrid);
        }

        private async void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.NewItems != null)
                {
                    if (e.NewItems.Count != 0)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            if (SelectFileViwe.Items.Count == 1)
                            {
                                SelectFileViwe.Items.Clear();
                                SelectFileViwe.Items.Insert(0, new FaxFileItem(this, false));
                            }
                        });
                        foreach (var item in e.NewItems)
                        {
                            var file = (KeyValuePair<string, byte[]>)item;
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SelectFileViwe.Items.Insert(0, new FaxFileItem(file, this)));
                        }
                    }
                }
            }
            catch
            {

            }
        }
        private void FaxPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Files.Count != 0)
            {
                try
                {
                    SelectFileViwe.Items.Clear();
                    SelectFileViwe.Items.Add(new FaxFileItem(this, false));
                    foreach (var file in Files)
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
                SelectFileViwe.Items.Add(new FaxFileItem(this, true));
            }

        }
        public async Task OpenFileSelector()
        {
            files = await fileOpenPicker.PickMultipleFilesAsync();
            await ParceFile(files);
        }
        private async void SelectFileViwe_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var files = items.OfType<StorageFile>().ToList().AsReadOnly();
                await ParceFile(files);
            }
        }
        private async Task ParceFile(IReadOnlyList<StorageFile> files)
        {
            if (files.Where(f => f.FileType == ".pdf").Count() != 0)
            {
                foreach (var file in files)
                {
                    using (Stream stream = await file.OpenStreamForReadAsync())
                    {
                        var prop = await file.GetBasicPropertiesAsync();
                        var name = file.Name + ";" + Math.Round((decimal)prop.Size / 1000, 1).ToString();
                        byte[] result = new byte[stream.Length];
                        await stream.ReadAsync(result, 0, (int)stream.Length);
                        if (!Files.Keys.Contains(name))
                        {
                            Files.Add(name, result);
                        }
                        else
                        {
                            int n = 1;
                            while (Files.Keys.Contains($"({n})" + name))
                            {
                                n++;
                            }
                            
                            Files.Add($"({n})" + name, result);
                        }
                    }
                }
            }
        }
        private async Task SendFile()
        {
            List<string> success = new List<string>();
            if (!String.IsNullOrEmpty(FaxNumber.Text))
            {
                if (!String.IsNullOrEmpty(FaxEmail.Text))
                {
                    List<FaxFileItem> faxFileItems = new List<FaxFileItem>();
                    foreach (var faxItem in SelectFileViwe.Items)
                    {
                        faxFileItems.Add((FaxFileItem)faxItem);
                    }
                    if (faxFileItems.Count != 0 && faxFileItems.Count() != 0)
                    {
                        foreach (var file in faxFileItems)
                        {
                            if (file.File.Value.Length != 0)
                            {
                                success.Add(await webService.PostToFax(App.AccountData.Data.User_Data.UserID, "", new string[] { FaxNumber.Text }, file.File.Value, App.UserPbx));
                                Files.Remove(file.File.Key);
                            }
                        }
                        var builder = new ToastContentBuilder()
                        .AddText("Fax Send", hintMaxLines: 2)
                        .AddText($"Success send files: ({success.Where(s => s.Contains("success")).Count()})", hintMaxLines: 1)
                        .AddText($"Warning send files: ({success.Where(s => s.Contains("warning")).Count()})");
                        builder.Show();
                    }
                    else
                    {
                        errorService.ShowWarning("Select Document");
                    }
                }
                else
                {
                    errorService.ShowWarning("Email Fild is Empty");
                }
            }
            else
            {
                errorService.ShowWarning("Number Fild is Empty");
            }
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
        private async void Navigate_Click(object sender, RoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
            var Navigate = (Button)sender;
            switch (Navigate.Name)
            {
                case "Contacts":
                    await OpenWindow(typeof(ContactsPage));
                    break;
                case "Phone":
                    await OpenWindow(typeof(PhonePage));
                    break;
                case "History":
                    await OpenWindow(typeof(HistoryPage));
                    break;
                case "Control":
                    await OpenWindow(typeof(ControlPage));
                    break;
                case "HotKeys":
                    await OpenWindow(typeof(HotKeyPage)).ConfigureAwait(true);
                    break;
            }
        }
        private async Task OpenWindow(Type Page)
        {
            if (App.AppWindows.Contains(Page.Name))
            {
                return;
            }
            else
            {
                AppWindow appWindow = await AppWindow.TryCreateAsync();
                Frame OpenPage1 = new Frame
                {
                    Name = Page.Name
                };
                OpenPage1.Navigate(Page);
                ElementCompositionPreview.SetAppWindowContent(appWindow, OpenPage1);
                appWindow.RequestMoveAdjacentToCurrentView();
                appWindow.TitleBar.BackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.InactiveBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonHoverForegroundColor = Colors.DarkGray;
                appWindow.TitleBar.ButtonHoverBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonPressedBackgroundColor = Colors.WhiteSmoke;
                appWindow.TitleBar.ButtonBackgroundColor = Colors.WhiteSmoke;
                WindowManagementPreview.SetPreferredMinSize(appWindow, App.Size);
                await appWindow.TryShowAsync();
                appWindow.Changed += App.AppWindow_Changed;
                App.AppWindows.Add(Page.Name);
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
        #endregion
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendFile();
        }
        private void SelectFileViwe_DragEnter(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
            
        }
        private void SelectFileViwe_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        public void RemoveFile(FaxFileItem faxFileItem)
        {
            if (faxFileItem != null)
            {
                SelectFileViwe.Items.Remove(faxFileItem);
                Files.Remove(faxFileItem.File.Key);
                if (SelectFileViwe.Items.Count == 1)
                {
                    SelectFileViwe.Items.Clear();
                    SelectFileViwe.Items.Add(new FaxFileItem(this, true));
                }
            }
        }
        private void Page_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            App.timeOut = DateTime.Now;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            PausesFild.Visibility = Visibility.Collapsed;
        }

        private async void Pauses_Click(object sender, RoutedEventArgs e)
        {
            PauseList.Items.Clear();
            if (ControlPage.getPauses == null)
            {
                ControlPage.getPauses = new Get_pauses
                {
                    ResponseData = new Status_pause()
                };
                ControlPage.getPauses.ResponseData.Pauses = new List<Pause>();
                ControlPage.getPauses = await webService.GetPauses(App.AccountData.Data.Sip_Settings.Sip_username, App.UserPbx);
                if (ControlPage.getPauses.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ControlPage.getPauses.ResponseData.Pause_active == 0));
                    foreach (var pause in ControlPage.getPauses.ResponseData.Pauses)
                    {
                        PauseList.Items.Add(new PauseItem(pause, pause.Id == ControlPage.getPauses.ResponseData.Pause_active));
                    }
                }
                else
                {
                    errorService.ShowWarning(ControlPage.getPauses.ResponseMessage);
                }
            }
            else
            {
                PauseList.Items.Add(new PauseItem(new Pause { Name = "Work", Id = 0 }, ControlPage.getPauses.ResponseData.Pause_active == 0));
                foreach (var pause in ControlPage.getPauses.ResponseData.Pauses)
                {
                    PauseList.Items.Add(new PauseItem(pause, pause.Id == ControlPage.getPauses.ResponseData.Pause_active));
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
                    if (ControlPage.getPauses.ResponseData.Pause_active != id)
                    {
                        var result = await webService.SetPause(App.AccountData.Data.Sip_Settings.Sip_username, id, App.UserPbx);
                        if (result.ResponseCode == System.Net.HttpStatusCode.OK)
                        {
                            ControlPage.getPauses.ResponseData.Pause_active = id;
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
    }
}
