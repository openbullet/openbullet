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

namespace OpenBullet.Views.Main.Settings.OpenBullet
{

    /// <summary>
    /// Logica di interazione per Themes.xaml
    /// </summary>
    public partial class Themes : Page
    {
        public Themes()
        {
            InitializeComponent();
            DataContext = OB.OBSettings.Themes;
            
            // Load all the saved colors
            SetColors();
            SetColorPreviews();
            SetImagePreviews();
            OB.MainWindow.AllowsTransparency = OB.OBSettings.Themes.AllowTransparency;
        }

        public void SetColors()
        {
            SetAppColor("BackgroundMain", OB.OBSettings.Themes.BackgroundMain);
            SetAppColor("BackgroundSecondary", OB.OBSettings.Themes.BackgroundSecondary);
            SetAppColor("ForegroundMain", OB.OBSettings.Themes.ForegroundMain);
            SetAppColor("ForegroundGood", OB.OBSettings.Themes.ForegroundGood);
            SetAppColor("ForegroundBad", OB.OBSettings.Themes.ForegroundBad);
            SetAppColor("ForegroundCustom", OB.OBSettings.Themes.ForegroundCustom);
            SetAppColor("ForegroundRetry", OB.OBSettings.Themes.ForegroundRetry);
            SetAppColor("ForegroundToCheck", OB.OBSettings.Themes.ForegroundToCheck);
            SetAppColor("ForegroundMenuSelected", OB.OBSettings.Themes.ForegroundMenuSelected);

            // This sets the background for the mainwindow (alternatively solid or image)
            OB.MainWindow.SetStyle();
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
                backgroundImagePreview.Source = GetImageBrush(OB.OBSettings.Themes.BackgroundImage);
                backgroundLogoPreview.Source = GetImageBrush(OB.OBSettings.Themes.BackgroundLogo);
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
            OB.OBSettings.Themes.BackgroundMain = "#222";
            OB.OBSettings.Themes.BackgroundSecondary = "#111";
            OB.OBSettings.Themes.ForegroundMain = "#dcdcdc";
            OB.OBSettings.Themes.ForegroundGood = "#adff2f";
            OB.OBSettings.Themes.ForegroundBad = "#ff6347";
            OB.OBSettings.Themes.ForegroundCustom = "#ff8c00";
            OB.OBSettings.Themes.ForegroundRetry = "#ffff00";
            OB.OBSettings.Themes.ForegroundToCheck = "#7fffd4";
            OB.OBSettings.Themes.ForegroundMenuSelected = "#1e90ff";

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
            OB.OBSettings.Themes.BackgroundImage = ofd.FileName;

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
            OB.OBSettings.Themes.BackgroundLogo = ofd.FileName;

            SetColors();
            SetImagePreviews();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
                OB.OBSettings.Themes.GetType().GetProperty(((ColorPicker)sender).Name.ToString()).SetValue(OB.OBSettings.Themes, ColorToHtml(e.NewValue.Value), null);

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
            OB.MainWindow.SetStyle();
        }
    }
}
