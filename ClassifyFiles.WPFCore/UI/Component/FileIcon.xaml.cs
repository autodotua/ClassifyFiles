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
        /// <summary>
        /// 是否启用缓存的全局开关
        /// </summary>
        /// <remarks>
        /// 因为比较容易出错，而且对性能提升不大，所以禁用了
        /// 可能对减少内存有点作用
        /// </remarks>
        public static bool StaticEnableCache { get; set; } = false;
        public bool EnableCache { get; set; } = true;
        public bool DisplayBetterImage { get; set; } = false;
        public static readonly DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(UIFile), typeof(FileIcon), new PropertyMetadata(OnFileChanged));
        static async void OnFileChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if ((obj as FileIcon).IsLoaded)
            {

            }
            else
            {
                await (obj as FileIcon).LoadAsync();
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
   
        public Stretch? stretch;
        public Stretch? Stretch
        {
            get => stretch;
            set
            {
                stretch = value;
                this.Notify(nameof(Stretch),nameof(ActualStretch));
            }
        }   
        public Stretch ActualStretch
        {
            get
            {
                if(Stretch==null)
                {
                    if(Configs.FileIconUniformToFill)
                    {
                        return System.Windows.Media.Stretch.UniformToFill;
                    }
                    return System.Windows.Media.Stretch.Uniform;
                }
                return Stretch.Value;
            }
        }
        private async Task LoadAsync()
        {
            await File.LoadClassesAsync();
            await LoadImageAsync();
            if (Configs.AutoThumbnails)
            {
                Tasks.Enqueue(RefreshIcon());
            }
            RegisterEvents();
        }
        private void RegisterEvents()
        {
            File.Display.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UIFileDisplay.Image))
                {
                    try
                    {
                        Dispatcher.InvokeAsync(() => LoadImageAsync());
                    }
                    catch
                    {

                    }
                }
            };
        }

        private static ConcurrentDictionary<int, FrameworkElement> caches = new ConcurrentDictionary<int, FrameworkElement>();
        public static void ClearCaches()
        {
            caches.Clear();
            RealtimeIcon.ClearCahces();
        }
        private static string folderIconPath = null;
        private object iconContent;
        public object IconContent
        {
            get => iconContent;
            set
            {
                if(value==iconContent)
                {
                    return;
                }
                iconContent = value;
                this.Notify(nameof(IconContent));
            }
        }
        public async Task<bool> RefreshIcon()
        {
            if (File.File.ThumbnailGUID != null && File.File.IconGUID != null)
            {
                return true;
            }
            UIFile file = null;
            Dispatcher.Invoke(() =>
            {
                file = File;
            });
            bool result = await RealtimeIcon.RefreshIcon(file);
            return result;
        }
        public async Task<bool> LoadImageAsync()
        {
            FrameworkElement item;
            if (DisplayBetterImage)
            {
                BitmapImage rawImage = null;
                if (File.File.FileInfo.Exists)
                {
                    item = new Image()
                    {
                        Source = rawImage ?? File.Display.Image,
                    };
                    IconContent = item;
                    //先显示缩略图，然后再显示更好的图片
                    rawImage = await File.Display.GetBetterImageAsync();
                }
                item = new Image()
                {
                    Source = rawImage ?? File.Display.Image,
                };
                IconContent = item;
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
                        if (IconContent is Image)
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
            IconContent = item;
            return item is Image;
        }

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public static TaskQueue Tasks { get; private set; } = new TaskQueue();


    }

}
