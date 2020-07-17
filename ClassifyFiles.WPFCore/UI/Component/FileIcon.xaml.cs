using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClassifyFiles.UI.Component
{
    /// <summary>
    /// FileIcon.xaml 的交互逻辑
    /// </summary>
    public partial class FileIcon : UserControlBase
    {
        public FileIcon()
        {
            InitializeComponent();
        }
        public bool EnableCache { get; set; } = true;
        public bool Square { get; set; } = true;
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
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(FileIcon), new PropertyMetadata(1.0));

        public double Scale
        {
            get => (double)GetValue(ScaleProperty); //UseLargeIcon;
            set => SetValue(ScaleProperty, value);
        }

        private static ConcurrentDictionary<int, FrameworkElement> caches = new ConcurrentDictionary<int, FrameworkElement>();
        public static void ClearCaches()
        {
            caches.Clear();
            RealtimeIcon.ClearCahces();
        }
        private static string folderIconPath = null;
        public async Task<bool> RefreshIcon()
        {
            UIFile file = null;
            Dispatcher.Invoke(() =>
            {
                file = File;
            });
            bool result = await RealtimeIcon.RefreshIcon(file);
            Dispatcher.Invoke(() =>
            {
                Load();
            });
            return result;
        }
        private bool Load()
        {

            FrameworkElement item;
            if (File.File.IsFolder && Configs.ShowExplorerIcon)
            {
                if (folderIconPath == null)
                {
                    var bitmap = FileUtility.GetFolderExplorerIcon();
                    string tempFileName = System.IO.Path.GetTempFileName() + ".png";
                    bitmap.Save(tempFileName);
                    folderIconPath = tempFileName;
                }

                item = new Image() { Source = new BitmapImage(new Uri(folderIconPath, UriKind.Absolute)) };
            }
            else
            {
                if (File.File.IsFolder == false
                    && EnableCache
                && caches.ContainsKey(File.File.ID)
                && !(File.Display.Image != null && caches[File.File.ID] is FontIcon))
                {
                    item = caches[File.File.ID];
                    if(item is Image image)
                    {
                        BindingSize(image);
                    }
                    else if(item is FontIcon icon)
                    {
                        BindingSize(icon);
                    }
                }
                else
                {
                    if (File.Display.Image == null)
                    {
                        if (main.Content is Image)
                        {
                            return true;
                        }
                        item = new FontIcon() { Glyph = File.Display.Glyph };
                        BindingSize(item as FontIcon);
                    }
                    else
                    {
                        item = new Image()
                        {
                            Source = File.Display.Image,
                        };
                        if (Square)
                        {
                            (item as Image).Stretch = Stretch.UniformToFill;
                        }
                        BindingSize(item as Image);
                    }
                    item.HorizontalAlignment = HorizontalAlignment.Center;
                    item.VerticalAlignment = VerticalAlignment.Center;

                    if (File.File.IsFolder == false && EnableCache)
                    {
                        caches.TryAdd(File.File.ID, item);
                    }
                }
            }
            main.Content = item;
            return item is Image;
        }

        MagnificationConverter mc = new MagnificationConverter();
        private void BindingSize(Image image)
        {
            if (Scale == 1)
            {
                image.SetBinding(WidthProperty, "File.Size.IconSize");
                if (Square)
                {
                    image.SetBinding(HeightProperty, "File.Size.IconSize");
                }

            }
            else if (Scale < 0)
            {
                image.Width = -Scale;
                if (Square)
                {
                    image.Height = -Scale;
                }
            }
            else
            {
                image.SetBinding(WidthProperty, new Binding("File.Size.IconSize") { Converter = mc, ConverterParameter = Scale });
                if (Square)
                {
                    image.SetBinding(HeightProperty, new Binding("File.Size.IconSize") { Converter = mc, ConverterParameter = Scale });
                }
            }
        }
        private void BindingSize(FontIcon icon)
        {
            if (Scale == 1)
            {
                icon.SetBinding(FontIcon.FontSizeProperty, "File.Size.FontIconSize");
            }
            else if (Scale < 0)
            {
                icon.FontSize = -Scale * 0.8;
            }
            else
            {
                icon.SetBinding(FontIcon.FontSizeProperty, new Binding("File.Size.FontIconSize") { Converter = mc, ConverterParameter = Scale });
            }
        }
        private async void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {

            await File.LoadAsync();
            if (!Load())
            {
                if (Configs.AutoThumbnails)
                {
                    Tasks.Enqueue(RefreshIcon);
                }
            }
        }

        public static TaskQueue Tasks { get; private set; } = new TaskQueue();


    }

}
