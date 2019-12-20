using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit;

namespace OpenBullet.Views.Main.Settings.OB
{

    /// <summary>
    /// Logica di interazione per Themes.xaml
    /// </summary>
    public partial class Themes : Page
    {
        public Themes()
        {
            InitializeComponent();
            DataContext = Globals.obSettings.Themes;
            
            // Load all the saved colors
            SetColors();
            SetColorPreviews();
            SetImagePreviews();
            Globals.mainWindow.AllowsTransparency = Globals.obSettings.Themes.AllowTransparency;
        }

        public void SetColors()
        {
            SetAppColor("BackgroundMain", Globals.obSettings.Themes.BackgroundMain);
            SetAppColor("BackgroundSecondary", Globals.obSettings.Themes.BackgroundSecondary);
            SetAppColor("ForegroundMain", Globals.obSettings.Themes.ForegroundMain);
            SetAppColor("ForegroundGood", Globals.obSettings.Themes.ForegroundGood);
            SetAppColor("ForegroundBad", Globals.obSettings.Themes.ForegroundBad);
            SetAppColor("ForegroundCustom", Globals.obSettings.Themes.ForegroundCustom);
            SetAppColor("ForegroundRetry", Globals.obSettings.Themes.ForegroundRetry);
            SetAppColor("ForegroundToCheck", Globals.obSettings.Themes.ForegroundToCheck);
            SetAppColor("ForegroundMenuSelected", Globals.obSettings.Themes.ForegroundMenuSelected);

            // This sets the background for the mainwindow (alternatively solid or image)
            Globals.mainWindow.SetStyle();
        }

        private void SetColorPreviews()
        {
            BackgroundMain.SelectedColor = GetAppColor("BackgroundMain");
            BackgroundSecondary.SelectedColor = GetAppColor("BackgroundSecondary");
            ForegroundMain.SelectedColor = GetAppColor("ForegroundMain");
            ForegroundGood.SelectedColor = GetAppColor("ForegroundGood");
            ForegroundBad.SelectedColor = GetAppColor("ForegroundBad");
            ForegroundCustom.SelectedColor = GetAppColor("ForegroundCustom");
            ForegroundRetry.SelectedColor = GetAppColor("ForegroundRetry");
            ForegroundToCheck.SelectedColor = GetAppColor("ForegroundToCheck");
            ForegroundMenuSelected.SelectedColor = GetAppColor("ForegroundMenuSelected");
        }

        public void SetAppColor(string resourceName, string color)
        {
            App.Current.Resources[resourceName] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public Color GetAppColor(string resourceName)
        {
            return ((SolidColorBrush)App.Current.Resources[resourceName]).Color;
        }

        private void SetImagePreviews()
        {
            try
            {
                backgroundImagePreview.Source = GetImageBrush(Globals.obSettings.Themes.BackgroundImage);
                backgroundLogoPreview.Source = GetImageBrush(Globals.obSettings.Themes.BackgroundLogo);
            }
            catch { }
        }

        private BitmapImage GetImageBrush(string file)
        {
            try
            {
                if (File.Exists(file))
                    return new BitmapImage(new Uri(file));
                else
                    return new BitmapImage(new Uri(@"pack://application:,,,/"
                        + Assembly.GetExecutingAssembly().GetName().Name
                        + ";component/"
                        + "Images/Themes/empty.png", UriKind.Absolute));
            }
            catch { return null; }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.obSettings.Themes.BackgroundMain = "#222";
            Globals.obSettings.Themes.BackgroundSecondary = "#111";
            Globals.obSettings.Themes.ForegroundMain = "#dcdcdc";
            Globals.obSettings.Themes.ForegroundGood = "#adff2f";
            Globals.obSettings.Themes.ForegroundBad = "#ff6347";
            Globals.obSettings.Themes.ForegroundCustom = "#ff8c00";
            Globals.obSettings.Themes.ForegroundRetry = "#ffff00";
            Globals.obSettings.Themes.ForegroundToCheck = "#7fffd4";
            Globals.obSettings.Themes.ForegroundMenuSelected = "#1e90ff";

            SetColors();
            SetColorPreviews();
            SetImagePreviews();
        }

        private void loadBackgroundImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|"
       + "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            ofd.FilterIndex = 4;
            ofd.ShowDialog();
            Globals.obSettings.Themes.BackgroundImage = ofd.FileName;

            SetColors();
            SetImagePreviews();
        }

        private void loadBackgroundLogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|"
       + "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            ofd.FilterIndex = 4;
            ofd.ShowDialog();
            Globals.obSettings.Themes.BackgroundLogo = ofd.FileName;

            SetColors();
            SetImagePreviews();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
                Globals.obSettings.Themes.GetType().GetProperty(((ColorPicker)sender).Name.ToString()).SetValue(Globals.obSettings.Themes, ColorToHtml(e.NewValue.Value), null);

            SetColors();
        }

        private string ColorToHtml(Color color)
        {
            return $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}";
        }

        private void useImagesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            SetColors();
        }

        private void useImagesCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetColors();
        }

        private void backgroundImageOpacityUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Globals.mainWindow.SetStyle();
        }
    }
}
