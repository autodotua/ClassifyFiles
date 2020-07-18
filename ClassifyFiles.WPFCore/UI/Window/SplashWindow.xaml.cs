using ClassifyFiles.WPFCore;
using FzLib.Extension;
using ModernWpf;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// SplashWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SplashWindow : Window,INotifyPropertyChanged
    {
        private static SplashWindow instance = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public static void TryShow()
        {
            if (instance == null)
            {
                instance = new SplashWindow();
                instance.Show();
            }
        }

        public static void TryClose()
        {
            if (instance != null)
            {
                instance.Close();
            }
        }

        public Uri image = null;
        public Uri Image => image;
        public SplashWindow()
        {
            DataContext = this;
            App.SetTheme(this);
            InitializeComponent();
            var theme = ThemeManager.GetActualTheme(this); 
            image=new Uri(theme == ElementTheme.Dark ? "../../Images/icon_dark.png" : "../../Images/icon_light.png", UriKind.Relative);
            this.Notify(nameof(Image));
        }
    }
}
