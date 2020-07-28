using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using ClassifyFiles.WPFCore;
using System.Drawing;
using System.Threading.Tasks;

namespace ClassifyFiles.UI.Model
{
    public class UIFileDisplay : INotifyPropertyChanged
    {
        public UIFileDisplay(Data.File file)
        {
            File = file;
            fileInfo = File.FileInfo;
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

                file.PropertyChanged += (p1, p2) =>
                {
                    if (p2.PropertyName == nameof(file.ThumbnailGUID) || p2.PropertyName == nameof(file.IconGUID))
                    {
                        this.Notify(nameof(Image));
                    }
                };
            }
        }
        public Data.File File { get; private set; }
        public FileInfo fileInfo;

        public long? length = null;
        public string Length
        {
            get
            {
                if (File.IsFolder)
                {
                    return "文件夹";
                }
                if (length == null)
                {
                    if (System.IO.File.Exists(File.GetAbsolutePath()))
                    {
                        try
                        {
                            length = fileInfo.Length;
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
                }
                if (length == -1)
                {
                    return "未知";
                }
                return FzLib.Basic.Number.ByteToFitString(length.Value);
            }
        }
        public string DisplayName
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
                if (!File.IsFolder && fileInfo.Exists)
                {
                    return fileInfo.LastWriteTime.ToString();
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
                if (!File.IsFolder && fileInfo.Exists)
                {
                    return fileInfo.CreationTime.ToString();
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
        public string DisplayDir => File.IsFolder ? File.Dir.Substring(0, File.Dir.Length - DisplayName.Length).Trim('\\') : File.Dir;
        public bool ShowTileViewPaths => Configs.ShowTilePath;
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
                        if (bitmap.Width * bitmap.Height< 5_000_000)//500万像素以下直接显示
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
                if (Configs.ShowThumbnail)
                {
                    if (!string.IsNullOrEmpty(File.ThumbnailGUID))
                    {
                        try
                        {
                            string path = FileUtility.GetThumbnailPath(File.ThumbnailGUID);
                            if (System.IO.File.Exists(FileUtility.GetThumbnailPath(File.ThumbnailGUID)))
                            {
                                return new BitmapImage(new Uri(path, UriKind.Absolute));
                            }
                            else
                            {
                                File.ThumbnailGUID = null;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                if (Configs.ShowExplorerIcon)
                {
                    if (!string.IsNullOrEmpty(File.IconGUID))
                    {
                        try
                        {
                            string path = FileUtility.GetIconPath(File.IconGUID);
                            if (System.IO.File.Exists(FileUtility.GetIconPath(File.IconGUID)))
                            {
                                return new BitmapImage(new Uri(path, UriKind.Absolute));
                            }
                            else
                            {
                                File.IconGUID = null;
                            }
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
