using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using FzLib.Extension;
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
DependencyProperty.Register("File", typeof(FileWithIcon), typeof(FileIcon), new PropertyMetadata(OnFileChanged));
        static void OnFileChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public FileWithIcon File
        {
            get => GetValue(FileProperty) as FileWithIcon; //file;
            set
            {
                SetValue(FileProperty, value);
                //file = value;
                //this.Notify(nameof(File));
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
            if(UseLargeIcon)
            {
                foreach (FrameworkElement item in (Content as Grid).Children)
                {
                    item.SetBinding(WidthProperty, "File.LargeIconSize");
                    item.SetBinding(HeightProperty, "File.LargeIconSize");
                }
            }
        }
    }
}
