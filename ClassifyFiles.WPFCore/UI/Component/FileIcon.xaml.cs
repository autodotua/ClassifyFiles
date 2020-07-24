using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using ClassifyFiles.Util;
using ClassifyFiles.Util.Win32;
using ClassifyFiles.Util.Win32.Shell;
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
        public static bool StaticEnableCache { get; set; } = true;
        public bool EnableCache { get; set; } = true;
        public bool DisplayRawImage { get; set; } = false;
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
            if (DisplayRawImage)
            {
                var rawImage = File.Display.RawImage;
                item = new Image()
                {
                    Source = rawImage ?? File.Display.Image,
                };
                main.Content = item;
                return true;
            }
            if (File.File.IsFolder && Configs.ShowExplorerIcon)
            {
                if (folderIconPath == null)
                {
                    var bitmap = ExplorerIcon.GetBitmapFromFolderPath(File.File.GetAbsolutePath());
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
                    && StaticEnableCache
                && caches.ContainsKey(File.File.ID)
                && !(File.Display.Image != null && caches[File.File.ID] is FontIcon))
                {
                    item = caches[File.File.ID];
                }
                else
                {
                    var img = File.Display.Image;
                    if (img == null)
                    {
                        if (main.Content is Image)
                        {
                            return true;
                        }
                        item = new FontIcon() { Glyph = File.Display.Glyph };
                    }
                    else
                    {
                        item = new Image()
                        {
                            Source = img,
                        };
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


        private void BindingSize()
        {
            //由于静态属性的绑定实在是感觉有问题，反正是单向绑定，那就手动使用初值设置+接受事件进行修改就好了
            if (Scale < 0)
            {
                view.Width = -Scale;
                if (Square)
                {
                    view.Height = -Scale;
                }
            }
            else
            {

                Set();
                Configs.StaticPropertyChanged += (p1, p2) =>
                {
                    if (p2.PropertyName == nameof(Configs.IconSize))
                    {
                        Set();
                    }
                };
            }
            void Set()
            {
                view.Width = Configs.IconSize * Scale;
                if (Square)
                {
                    view.Height = Configs.IconSize * Scale;
                }
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
            BindingSize();
        }

        public static TaskQueue Tasks { get; private set; } = new TaskQueue();


    }

}
