using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Util;
using ClassifyFiles.Util;
using ClassifyFiles.Util.Win32;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
                DefaultDispatcherPriority = DispatcherPriority.Render;
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

        private static async void OnFileChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
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
            if (!File.File.IsFolder)
            {
                await File.LoadClassesAsync();
            }
            await LoadImageAsync();
            File.Display.PropertyChanged += async (s, e) =>
             {
                 if (e.PropertyName == nameof(UIFileDisplay.Image))
                 {
                     try
                     {
                         await LoadImageAsync();
                     }
                     catch
                     {
                     }
                 }
             };
            await Dispatcher.InvokeAsync(() =>
             {
                 RealtimeUpdate.AddTask(NonDPFile);
             }, DispatcherPriority.Background);
        }

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
                BitmapImage img = File.Display.Image;
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
                    item = GetImage(img);
                }
                item.HorizontalAlignment = HorizontalAlignment.Center;
                item.VerticalAlignment = VerticalAlignment.Center;

                IconContent = item;
            }, DefaultDispatcherPriority);
            return item is Image;
        }

        private Image GetImage(BitmapSource source)
        {
            Image image = new Image()
            {
                Source = source,
                SnapsToDevicePixels = true,
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            return image;
        }

    }
}