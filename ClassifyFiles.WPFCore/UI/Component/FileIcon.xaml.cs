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
            FileIcon fileIcon = obj as FileIcon;
            if (fileIcon.File.File.IsFolder)
            {
            }
            else
            {
                fileIcon.File.Load += fileIcon.Load;
                fileIcon.File.Display.PropertyChanged += (p1, p2) =>
                {
                    if (p2.PropertyName == nameof(UIFileDisplay.Image))
                    {
                        fileIcon.Load(p1, p2);
                    }
                };
            }
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

        public static ConcurrentDictionary<int, FrameworkElement> Caches { get; } = new ConcurrentDictionary<int, FrameworkElement>();
        private void Load(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                FrameworkElement item = null;
                if (File.File.IsFolder == false
                && Caches.ContainsKey(File.File.ID)
                && !(File.Display.Image != null && Caches[File.File.ID] is FontIcon))
                {
                    item = Caches[File.File.ID];
                }
                else
                {
                    if (File.Display.Image == null)
                    {
                        if (main.Content is Image)
                        {
                            return;
                        }
                        item = new FontIcon() { Glyph = File.Display.Glyph };
                    }
                    else
                    {
                        item = new Image()
                        {
                            Source = File.Display.Image,
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    item.HorizontalAlignment = HorizontalAlignment.Center;
                    item.VerticalAlignment = VerticalAlignment.Center;

                    if (File.File.IsFolder == false)
                    {
                        Caches.TryAdd(File.File.ID, item);
                    }
                }
                if (UseLargeIcon)
                {
                    item.SetBinding(WidthProperty, "File.Size.LargeIconSize");
                    item.SetBinding(HeightProperty, "File.Size.LargeIconSize");
                    item.SetBinding(FontIcon.FontSizeProperty, "File.Size.LargeFontIconSize");
                }
                else
                {
                    item.SetBinding(WidthProperty, "File.Size.SmallIconSize");
                    item.SetBinding(HeightProperty, "File.Size.SmallIconSize");
                    item.SetBinding(FontIcon.FontSizeProperty, "File.Size.SmallFontIconSize");
                }
                main.Content = item;
            });
        }
        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            Load(null, null);
        }
    }
}
