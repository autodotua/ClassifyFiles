using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClassifyFiles.UI.Model
{
    public class UIFileDisplay : INotifyPropertyChanged
    {
        public UIFileDisplay(Data.File file)
        {
            File = file;
            FileInfo = File.FileInfo;
            if (file.IsFolder)
            {
                Glyph = FolderGlyph;
            }
            else
            {
                string ext = System.IO.Path.GetExtension(file.Name).ToLower().TrimStart('.');
                FileType type = FileType.FileTypes.FirstOrDefault(p => p.Extensions.Contains(ext));
                if (type != null)
                {
                    Glyph = type.Glyph;
                }
            }

            if (DebugSwitch.OnDisplayPropertyChanged)
            {
                PropertyChanged += (p1, p2) =>
                {
                    System.Diagnostics.Debug.WriteLine("Display Prop Changed： " + p2.PropertyName);
                };
            }
        }

        public void NotifyIconChanged()
        {
            this.Notify(nameof(Image));
        }

        private string GetLengthString()
        {
            if (File.IsFolder)
            {
                return "文件夹";
            }
            long length = 0;
            if (System.IO.File.Exists(File.GetAbsolutePath()))
            {
                try
                {
                    length = FileInfo.Length;
                }
                catch
                {
                    length = -1;
                }
            }
            else
            {
                length = -1;
            }

            if (length == -1)
            {
                return "未知";
            }
            return FzLib.Basic.Number.ByteToFitString(length);
        }

        public async void BuildUI()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
           {
               DisplayDir = File.IsFolder ? File.Dir.Substring(0, File.Dir.Length - DefaultDisplayName.Length).Trim('\\') : File.Dir;
               if (Length != null)
               {
                   Length = GetLengthString();
               }
           },
          System.Windows.Threading.DispatcherPriority.Background);
        }

        public Data.File File { get; private set; }
        public FileInfo FileInfo { get; private set; }

        public string length;

        public string Length
        {
            get => length;
            set
            {
                length = value;
                this.Notify(nameof(Length));
            }
        }

        private string displayName;

        public string DisplayName
        {
            get
            {
                if (displayName == null)
                {
                    return DefaultDisplayName;
                }
                return displayName;
            }
            set
            {
                displayName = value;
                this.Notify(nameof(DisplayName));
            }
        }

        private string displayProperty1 = null;

        public string DisplayProperty1
        {
            get => displayProperty1;
            set
            {
                displayProperty1 = value;
                this.Notify(nameof(DisplayProperty1));
            }
        }

        private string displayProperty2 = null;

        public string DisplayProperty2
        {
            get => displayProperty2;
            set
            {
                displayProperty2 = value;
                this.Notify(nameof(DisplayProperty2));
            }
        }

        private string displayProperty3 = null;

        public string DisplayProperty3
        {
            get => displayProperty3;
            set
            {
                displayProperty3 = value;
                this.Notify(nameof(DisplayProperty3));
            }
        }

        public string DefaultDisplayName
        {
            get
            {
                if (File.IsFolder)
                {
                    return new DirectoryInfo(File.Dir).Name;
                }

                if (Configs.ShowFileExtension)
                {
                    return File.Name;
                }
                return Path.GetFileNameWithoutExtension(File.Name);
            }
        }

        public string DisplayLastWriteTime
        {
            get
            {
                if (!File.IsFolder && FileInfo.Exists)
                {
                    return FileInfo.LastWriteTime.ToString();
                }
                else if (File.IsFolder)
                {
                    DirectoryInfo dir = new DirectoryInfo(File.GetAbsolutePath());
                    if (dir.Exists)
                    {
                        return dir.LastWriteTime.ToString();
                    }
                    return "未知";
                }
                else
                {
                    return "未知";
                }
            }
        }

        public string DisplayCreationTime
        {
            get
            {
                if (!File.IsFolder && FileInfo.Exists)
                {
                    return FileInfo.CreationTime.ToString();
                }
                else if (File.IsFolder)
                {
                    DirectoryInfo dir = new DirectoryInfo(File.GetAbsolutePath());
                    if (dir.Exists)
                    {
                        return dir.CreationTime.ToString();
                    }
                    return "未知";
                }
                else
                {
                    return "未知";
                }
            }
        }

        public string displayDir;

        public string DisplayDir
        {
            get => displayDir;
            set
            {
                displayDir = value;
                this.Notify(nameof(DisplayDir));
            }
        }

        public const string FileGlyph = "\uED41";
        public const string FolderGlyph = "\uED43";

        public event PropertyChangedEventHandler PropertyChanged;

        public string Glyph { get; set; } = FileGlyph;
        public Symbol Symbol { get; set; } = Symbol.OpenFile;

        public async Task<BitmapImage> GetBetterImageAsync()
        {
            if (System.IO.File.Exists(File.GetAbsolutePath()))
            {
                BitmapImage bitmapImage = null;
                await Task.Run(() =>
                {
                    try
                    {
                        Bitmap bitmap = new Bitmap(File.GetAbsolutePath());
                        if (bitmap.Width * bitmap.Height < 5_000_000)//500万像素以下直接显示
                        {
                            bitmapImage = bitmap.ToBitmapImage();
                        }
                        else
                        {
                            Bitmap resized = new Bitmap(bitmap, new System.Drawing.Size(640, (int)(1.0 * bitmap.Height / bitmap.Width * 640)));
                            bitmapImage = resized.ToBitmapImage();
                        }
                    }
                    catch
                    {
                    }
                    //var bitmapImage = new BitmapImage(new Uri(File.GetAbsolutePath(), UriKind.Absolute));
                });
                return bitmapImage;
            }
            return null;
        }

        public BitmapImage Image
        {
            get
            {
                if (File.IsFolder)
                {
                    if (Configs.ThumbnailStrategy != ThumbnailStrategy.None)
                    {
                        var uri = new Uri("pack://application:,,,/Images/folder.png", UriKind.Absolute);
                        return new BitmapImage(uri);
                    }
                    return null;
                }

                if (Configs.ThumbnailStrategy == ThumbnailStrategy.Win10Icon)
                {//需要提取方法
                    if (File.HasWin10Icon())
                    {
                        try
                        {
                            return new BitmapImage(new Uri(File.GetWin10IconPath(), UriKind.Absolute));
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    return null;
                }
                else
                {
                    if (Configs.ThumbnailStrategy == ThumbnailStrategy.MediaThumbnailPrefer)
                    {
                        if (File.HasThumbnail())
                        {
                            try
                            {
                                return new BitmapImage(new Uri(File.GetThumbnailPath(), UriKind.Absolute));
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    if (Configs.ThumbnailStrategy == ThumbnailStrategy.WindowsExplorerIcon
                        || Configs.ThumbnailStrategy == ThumbnailStrategy.MediaThumbnailPrefer)
                    {
                        if (File.HasExplorerIcon())
                        {
                            try
                            {
                                return new BitmapImage(new Uri(File.GetExplorerIconPath(), UriKind.Absolute));
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    return null;
                }
            }
        }
    }
}