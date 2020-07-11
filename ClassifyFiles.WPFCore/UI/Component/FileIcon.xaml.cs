using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClassifyFiles.UI.Component
{
    /// <summary>
    /// FileIcon.xaml 的交互逻辑
    /// </summary>
    public partial class FileIcon : UserControlBase
    {
        public FileIcon()
        {
            //DataContext = null;
            InitializeComponent();

        }
        public static readonly DependencyProperty FileProperty =
DependencyProperty.Register("File", typeof(UIFile), typeof(FileIcon), new PropertyMetadata(OnFileChanged));
        static void OnFileChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public UIFile File
        {
            get => GetValue(FileProperty) as UIFile; //file;
            set
            {
                SetValue(FileProperty, value);
            }
        }
        public static readonly DependencyProperty UseLargeIconProperty =
DependencyProperty.Register("UseLargeIcon", typeof(bool), typeof(FileIcon), new PropertyMetadata(OnUseLargeIconChanged));
        static void OnUseLargeIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public bool UseLargeIcon
        {
            get => (bool)GetValue(UseLargeIconProperty); //UseLargeIcon;
            set => SetValue(UseLargeIconProperty, value);
        }

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 2; i++)
            {
                FrameworkElement item = i == 0 ? icon : image as FrameworkElement;
                if (UseLargeIcon)
                {
                    item.SetBinding(WidthProperty, "File.LargeIconSize");
                    item.SetBinding(HeightProperty, "File.LargeIconSize");
                    item.SetBinding(FontIcon.FontSizeProperty, "File.LargeFontSize");
                }
                else
                {
                    item.SetBinding(WidthProperty, "File.SmallIconSize");
                    item.SetBinding(HeightProperty, "File.SmallIconSize");
                    item.SetBinding(FontIcon.FontSizeProperty, "File.SmallFontSize");
                }
            }
        }
    }
}
