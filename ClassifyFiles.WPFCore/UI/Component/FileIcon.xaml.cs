using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Collections.Concurrent;
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
            (obj as FileIcon).File.Load += (obj as FileIcon).Load;
            (obj as FileIcon).File.PropertyChanged += (p1, p2) =>
            {
                if (p2.PropertyName == nameof(UIFile.Image))
                {
                    (obj as FileIcon).Load(p1, p2);
                }
            };
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
            DependencyProperty.Register("UseLargeIcon", typeof(bool), typeof(FileIcon));

        public bool UseLargeIcon
        {
            get => (bool)GetValue(UseLargeIconProperty); //UseLargeIcon;
            set => SetValue(UseLargeIconProperty, value);
        }

        private static ConcurrentDictionary<int, FrameworkElement> caches = new ConcurrentDictionary<int, FrameworkElement>();
        private void Load(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                FrameworkElement item = null;
                if (caches.ContainsKey(File.ID) && !(File.Image != null && caches[File.ID] is FontIcon))
                {
                    item = caches[File.ID];
                }
                else
                {
                    if (File.Image == null)
                    {
                        if (main.Content != null)
                        {
                            return;
                        }
                        item = new FontIcon();
                        item.SetBinding(FontIcon.GlyphProperty, "File.Glyph");
                    }
                    else
                    {
                        item = new Image()
                        {
                            Source = File.Image,
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    item.HorizontalAlignment = HorizontalAlignment.Center;
                    item.VerticalAlignment = VerticalAlignment.Center;

                    caches.TryAdd(File.ID, item);
                }
                if (UseLargeIcon)
                {
                    item.SetBinding(WidthProperty, "File.LargeIconSize");
                    item.SetBinding(HeightProperty, "File.LargeIconSize");
                    item.SetBinding(FontIcon.FontSizeProperty, "File.LargeFontIconSize");
                }
                else
                {
                    item.SetBinding(WidthProperty, "File.SmallIconSize");
                    item.SetBinding(HeightProperty, "File.SmallIconSize");
                    item.SetBinding(FontIcon.FontSizeProperty, "File.SmallFontIconSize");
                }
                main.Content = item;
            });
        }
        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
