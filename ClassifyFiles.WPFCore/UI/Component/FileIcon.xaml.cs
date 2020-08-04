using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using ClassifyFiles.UI.Util;
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
                this.Notify(nameof(Stretch), nameof(ActualStretch));
            }
        }
        public Stretch ActualStretch
        {
            get
            {
                if (Stretch == null)
                {
                    if (Configs.FileIconUniformToFill)
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
            await Tasks.Enqueue(RefreshIcon);
            await Tasks.Enqueue(RefreshTexts);
        }

        private static string folderIconPath = null;
        private object iconContent;
        public object IconContent
        {
            get => iconContent;
            set
            {
                if (value == iconContent)
                {
                    return;
                }
                iconContent = value;
                this.Notify(nameof(IconContent));
            }
        }
        public void RefreshIcon()
        {
            UIFile file = null;
            Dispatcher.Invoke(() =>
            {
                file = File;
            });
            RealtimeUpdate.UpdateFileIcon(file);
        }
        public void RefreshTexts()
        {
            UIFile file = null;
            Dispatcher.Invoke(() =>
            {
                file = File;
            });
            RealtimeUpdate.UpdateDisplay(file);
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
            if (File.File.IsFolder &&(
                Configs.ThumbnailStrategy == ThumbnailStrategy.MediaThumbnailPrefer
                ||Configs.ThumbnailStrategy == ThumbnailStrategy.WindowsExplorerIcon
                ))
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
