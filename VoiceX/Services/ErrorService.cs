using System;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VoiceX.Services
{
    public class ErrorService
    {
        readonly Grid MainGrid;
        readonly Grid background;
        readonly Grid window;
        readonly Button close;
        readonly Button copy;
        readonly Grid closeFrame;
        readonly Grid closePlan1;
        readonly Grid closePlan2;
        readonly Image ErrorIcone;
        readonly Image copyImage;
        readonly TextBlock StatusError;
        readonly TextBlock errorMessageBlock;
        readonly ResourceDictionary stylesDictionary;
        public Button Continue;
        readonly Button Cansel;
        public ErrorService(Grid pageGrid)
        {

            Continue = new Button();
            Cansel = new Button();
            stylesDictionary = new ResourceDictionary();
            stylesDictionary.Source = new Uri("ms-appx:///Style/Style.xaml");
            MainGrid = pageGrid;
            background = new Grid();
            window = new Grid();
            close = new Button();
            copy = new Button();
            copyImage = new Image();

            closeFrame = new Grid();
            closePlan1 = new Grid();
            closePlan2 = new Grid();
            closeFrame = new Grid();
            ErrorIcone = new Image();
            StatusError = new TextBlock();
            errorMessageBlock = new TextBlock();

            close.Height = 16;
            close.Width = 16;
            close.Padding = new Thickness(0, 0, 0, 0);
            close.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            close.HorizontalAlignment = HorizontalAlignment.Right;
            close.VerticalAlignment = VerticalAlignment.Top;
            close.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            close.VerticalContentAlignment = VerticalAlignment.Stretch;
            close.Margin = new Thickness(0, 5, 6, 0);
            close.BorderThickness = new Thickness(0);
            close.PointerEntered += Close_PointerEntered;
            close.PointerExited += Close_PointerExited;
            close.Click += Close_Click;
            close.Style = (Style)stylesDictionary["ButtonStyleMenu"];

            copy.Height = 15;
            copy.Width = 15;
            copy.Padding = new Thickness(0, 0, 0, 0);
            copy.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            copy.HorizontalAlignment = HorizontalAlignment.Right;
            copy.VerticalAlignment = VerticalAlignment.Top;
            copy.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            copy.VerticalContentAlignment = VerticalAlignment.Stretch;
            copy.Margin = new Thickness(0, 5, 24, 0);
            copy.BorderThickness = new Thickness(0);
            copy.PointerEntered += Close_PointerEntered;
            copy.PointerExited += Close_PointerExited;
            copy.Click += Copy_Click;
            copy.Style = (Style)stylesDictionary["ButtonStyleMenu"];

            Continue.Style = (Style)stylesDictionary["ButtonStyleStandart"];
            Continue.Height = 32;
            Continue.Width = 148;
            Continue.Margin = new Thickness(0, 154, 0, 0);
            Continue.VerticalAlignment = VerticalAlignment.Top;
            Continue.HorizontalAlignment = HorizontalAlignment.Center;
            Continue.FontFamily = new FontFamily("ms-appx:/Assets/Fonts/NunitoSans_10pt-Regular.ttf");
            Continue.FontSize = 18;
            Continue.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            Continue.Content = "Continue";
            Continue.Background = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            Continue.BorderThickness = new Thickness(0);
            Continue.PointerEntered += Close_PointerEntered;
            Continue.PointerExited += Close_PointerExited;
            Continue.Click += Continue_Click;

            Cansel.Style = (Style)stylesDictionary["ButtonStyleStandart"];
            Cansel.Height = 32;
            Cansel.Width = 148;
            Cansel.Margin = new Thickness(0, 191, 0, 0);
            Cansel.VerticalAlignment = VerticalAlignment.Top;
            Cansel.HorizontalAlignment = HorizontalAlignment.Center;
            Cansel.FontSize = 18;
            Cansel.FontFamily = new FontFamily("ms-appx:/Assets/Fonts/NunitoSans_10pt-Regular.ttf");
            Cansel.Content = "Cansel";
            Cansel.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            Cansel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 138, 99, 251));
            Cansel.BorderThickness = new Thickness(1);
            Cansel.PointerEntered += Close_PointerEntered;
            Cansel.PointerExited += Close_PointerExited;
            Cansel.Click += Cansel_Click; ;

            copyImage.Source = new BitmapImage(new Uri("ms-appx:/Assets/Icone_v2/Copy.png"));

            closeFrame.Padding = new Thickness(-1, 0, 0, 0);

            closePlan1.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            closePlan1.Height = 1;
            closePlan1.Width = 9.4;
            closePlan1.CornerRadius = new CornerRadius(0.5);
            closePlan1.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            closePlan1.RenderTransform = new CompositeTransform() { Rotation = -45 };

            closePlan2.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            closePlan2.Height = 1;
            closePlan2.Width = 9.4;
            closePlan2.CornerRadius = new CornerRadius(0.5);
            closePlan2.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            closePlan2.RenderTransform = new CompositeTransform() { Rotation = 45 };

            close.Content = closeFrame;
            copy.Content = copyImage;

            ErrorIcone.Height = 54;
            ErrorIcone.Width = 54;
            ErrorIcone.HorizontalAlignment = HorizontalAlignment.Center;
            ErrorIcone.VerticalAlignment = VerticalAlignment.Top;

            StatusError.HorizontalAlignment = HorizontalAlignment.Center;
            StatusError.VerticalAlignment = VerticalAlignment.Top;
            StatusError.Width = 125;
            StatusError.Height = 50;
            StatusError.FontSize = 20;
            StatusError.TextWrapping = TextWrapping.Wrap;
            StatusError.TextAlignment = TextAlignment.Center;

            background.Background = new SolidColorBrush(Color.FromArgb(204, 32, 34, 68));
            window.Width = 205;
            window.Height = 150;
            window.CornerRadius = new CornerRadius(12);
            window.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            window.Margin = new Thickness(0, -20, 0, 0);

            closeFrame.Children.Add(closePlan2);
            closeFrame.Children.Add(closePlan1);
            window.Children.Add(ErrorIcone);
            window.Children.Add(close);
            window.Children.Add(StatusError);
            window.Children.Add(copy);
            window.Children.Add(errorMessageBlock);
            background.Children.Add(window);
            
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(errorMessageBlock.Text))
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(errorMessageBlock.Text);
                Clipboard.SetContent(dataPackage);
            }
        }

        public void ShowSuccess()
        {
            StatusError.Margin = new Thickness(0, 86, 0, 0);
            StatusError.FontFamily = new FontFamily("ms-appx:/Assets/Fonts/NunitoSans_10pt-Regular.ttf");
            StatusError.Text = "Successfully completed";
            window.Children.Add(ErrorIcone);
            window.Children.Add(StatusError);
            MainGrid.Children.Add(background);
        }
        public void ShowError(string errorMessage)
        {
            try
            {
                errorMessageBlock.TextAlignment = TextAlignment.Center;
                errorMessageBlock.Width = 143;
                errorMessageBlock.Height = 40;
                errorMessageBlock.FontSize = 14;
                errorMessageBlock.Margin = new Thickness(0, 106, 0, 0);
                errorMessageBlock.TextWrapping = TextWrapping.Wrap;
                errorMessageBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                errorMessageBlock.Text = errorMessage;
                ToolTipService.SetToolTip(errorMessageBlock, errorMessage);

                StatusError.Margin = new Thickness(0, 80, 0, 0);
                StatusError.FontFamily = new FontFamily("ms-appx:/Assets/Fonts/NunitoSans_10pt-Regular.ttf");
                StatusError.Text = "Attention";

                ErrorIcone.Height = 54;
                ErrorIcone.Width = 54;
                ErrorIcone.Margin = new Thickness(0, 18, 0, 0);
                ErrorIcone.Source = new BitmapImage(new Uri("ms-appx:/Assets/Icone_v2/Error.png"));

                MainGrid.Children.Add(background);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }
        public void ShowWarning(string warningMessage)
        {
            try
            {
                errorMessageBlock.TextAlignment = TextAlignment.Center;
                errorMessageBlock.Width = 143;
                errorMessageBlock.Height = 40;
                errorMessageBlock.FontSize = 14;
                errorMessageBlock.Margin = new Thickness(0, 106, 0, 0);
                errorMessageBlock.TextWrapping = TextWrapping.Wrap;
                errorMessageBlock.Text = warningMessage;
                errorMessageBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                ToolTipService.SetToolTip(errorMessageBlock, warningMessage);

                StatusError.Margin = new Thickness(0, 80, 0, 0);
                StatusError.FontFamily = new FontFamily("ms-appx:/Assets/Fonts/NunitoSans_10pt-Regular.ttf");
                StatusError.Text = "Attention";

                ErrorIcone.Height = 90;
                ErrorIcone.Width = 90;
                ErrorIcone.Margin = new Thickness(0, 2, 0, 0);
                ErrorIcone.Source = new BitmapImage(new Uri("ms-appx:/Assets/Icone_v2/Warning.png"));

                MainGrid.Children.Add(background);

            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }
        public void ShowWarningWithButton(string warningMessage)
        {
            try
            {
                window.Width = 205;
                window.Height = 240;
                window.Children.Add(Continue);
                window.Children.Add(Cansel);
                errorMessageBlock.VerticalAlignment = VerticalAlignment.Top;
                errorMessageBlock.TextAlignment = TextAlignment.Center;
                errorMessageBlock.Width = 143;
                errorMessageBlock.Height = 40;
                errorMessageBlock.FontSize = 14;
                errorMessageBlock.Margin = new Thickness(0, 110, 0, 0);
                errorMessageBlock.TextWrapping = TextWrapping.Wrap;
                errorMessageBlock.Text = warningMessage;
                errorMessageBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                ToolTipService.SetToolTip(errorMessageBlock, warningMessage);

                StatusError.Margin = new Thickness(0, 75, 0, 0);
                StatusError.FontFamily = new FontFamily("ms-appx:/Assets/Fonts/NunitoSans_10pt-Regular.ttf");
                StatusError.Text = "Are you sure?";

                ErrorIcone.Height = 90;
                ErrorIcone.Width = 90;
                ErrorIcone.Margin = new Thickness(0, 2, 0, 0);
                ErrorIcone.Source = new BitmapImage(new Uri("ms-appx:/Assets/Icone_v2/Warning.png"));

                close.Visibility = Visibility.Collapsed;
                copy.Visibility = Visibility.Collapsed;
                MainGrid.Children.Add(background);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            window.Width = 205;
            window.Height = 150;
            window.Children.Remove(Continue);
            window.Children.Remove(Cansel);
            close.Visibility = Visibility.Visible;
            copy.Visibility = Visibility.Visible;
            MainGrid.Children.Remove(background);
        }

        private void Cansel_Click(object sender, RoutedEventArgs e)
        {
            window.Width = 205;
            window.Height = 150;
            window.Children.Remove(Continue);
            window.Children.Remove(Cansel);
            close.Visibility = Visibility.Visible;
            copy.Visibility = Visibility.Visible;
            MainGrid.Children.Remove(background);
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Remove(background);
        }

        private void Close_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            close.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            copy.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        }

        private void Close_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
            close.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            copy.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        }
    }
}
