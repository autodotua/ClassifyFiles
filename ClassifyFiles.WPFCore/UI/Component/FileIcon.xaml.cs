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
        static FileIcon()
        {
            Configs.StaticPropertyChanged += (p1, p2) =>
            {
                if (p2.PropertyName == nameof(Configs.FluencyFirst))
                {
                    ResetDefaultDispatcherPriority();
                }
            };
            ResetDefaultDispatcherPriority();
        }
        private static void ResetDefaultDispatcherPriority()
        {
            if (Configs.FluencyFirst)
            {
                DefaultDispatcherPriority = DispatcherPriority.Background;
            }
            else
            {
                DefaultDispatcherPriority = DispatcherPriority.Normal;
            }
        }
        public FileIcon()
        {
            InitializeComponent();
        }

        private static DispatcherPriority DefaultDispatcherPriority;
        public bool DisplayBetterImage { get; set; } = false;
        public static readonly DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(UIFile), typeof(FileIcon), new PropertyMetadata(OnFileChanged));
        static async void OnFileChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

            FileIcon fileIcon = obj as FileIcon;
            bool loaded = fileIcon.IsLoaded;
            fileIcon.IconContent = null;
            await fileIcon.Dispatcher.InvokeAsync(() =>
            {
                fileIcon.NonDPFile = fileIcon.File;
                fileIcon.File.Display.BuildUI();
            }, DefaultDispatcherPriority);
            if (loaded)
            {

            }
            else
            {
                await fileIcon.LoadAsync();
            }
        }

        private UIFile NonDPFile { get; set; }
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
            File.Display.PropertyChanged +=async (s, e) =>
            {
                if (e.PropertyName == nameof(UIFileDisplay.Image))
                {
                    try
                    {
                        await Dispatcher.InvokeAsync(() => LoadImageAsync(), DefaultDispatcherPriority);
                    }
                    catch
                    {

                    }
                }
            };
            await Tasks.Enqueue(() => RealtimeUpdate.UpdateFileIcon(NonDPFile));
            if (File.Class != null && (!string.IsNullOrEmpty(File.Class.DisplayNameFormat)
                || !string.IsNullOrEmpty(File.Class.DisplayProperty1)
                || !string.IsNullOrEmpty(File.Class.DisplayProperty2)
                || !string.IsNullOrEmpty(File.Class.DisplayProperty3)))
            {
                await Tasks.Enqueue(() => RealtimeUpdate.UpdateDisplay(NonDPFile));
            }
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

        public async Task<bool> LoadImageAsync()
        {
            FrameworkElement item = null;

            //对于ToolTip
            if (DisplayBetterImage)
            {
                BitmapImage rawImage = null;
                if (File.File.FileInfo.Exists)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        item = new Image()
                        {
                            Source = rawImage ?? File.Display.Image,
                        };
                        IconContent = item;
                    }, DefaultDispatcherPriority);
                    //先显示缩略图，然后再显示更好的图片
                    rawImage = await File.Display.GetBetterImageAsync();
                }
                item = new Image()
                {
                    Source = rawImage ?? File.Display.Image,
                };
                await Dispatcher.InvokeAsync(() =>
                    IconContent = item, DefaultDispatcherPriority);
                return true;
            }


            await Dispatcher.InvokeAsync(() =>
            {
                if (File.File.IsFolder && (
                    Configs.ThumbnailStrategy == ThumbnailStrategy.MediaThumbnailPrefer
                    || Configs.ThumbnailStrategy == ThumbnailStrategy.WindowsExplorerIcon
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
                            return;
                        }
                        item = new FontIcon() { Glyph = File.Display.Glyph };
                    }
                    else
                    {
                        img.Freeze();
                        item = new Image()
                        {
                            Source = img,
                        };
                    }
                    item.HorizontalAlignment = HorizontalAlignment.Center;
                    item.VerticalAlignment = VerticalAlignment.Center;
                }
                IconContent = item;
            }, DefaultDispatcherPriority);
            return item is Image;
        }

        public static TaskQueue Tasks { get; private set; } = new TaskQueue();


    }

}
