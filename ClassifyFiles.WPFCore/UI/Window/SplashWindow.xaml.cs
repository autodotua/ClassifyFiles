using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// SplashWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SplashWindow : Window
    {
        private static SplashWindow instance = null;
        public static void ShowScreen()
        {
            instance = new SplashWindow();
            instance.Show();
        }

        public static void CloseScreen()
        {
            instance.Close();
        }
        public SplashWindow()
        {
            InitializeComponent();
            var bytes = File.ReadAllBytes("Images/icon.png");
            (Content as Grid).Background = new ImageBrush(ToImage(bytes));
            //image.Source = new BitmapImage(new Uri("./icon.png", UriKind.Relative));
        }
        /// <summary>
        /// 将字符数组转换为<see cref="BitmapImage"/>
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private BitmapImage ToImage(byte[] array)
        {
            using var ms = new System.IO.MemoryStream(array);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // here
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
