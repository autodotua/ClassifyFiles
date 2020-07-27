using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;

namespace ClassifyFiles.UI.Model
{
    public class UIFileDisplay : INotifyPropertyChanged
    {
        public UIFileDisplay(Data.File file)
        {
            File = file;
            FileInfo = new FileInfo(file.GetAbsolutePath());
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
        public FileInfo FileInfo { get; }

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
                    try
                    {
                        length = File.GetFileInfo().Length;
                    }
                    catch
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
        public string DisplayDir => File.IsFolder ? File.Dir.Substring(0, File.Dir.Length - DisplayName.Length).Trim('\\') : File.Dir;
        public bool ShowIconViewNames => Configs.ShowIconViewNames;
        public bool ShowTileViewPaths => Configs.ShowTilePath;
        public const string FileGlyph = "\uED41";
        public const string FolderGlyph = "\uED43";

        public event PropertyChangedEventHandler PropertyChanged;

        public string Glyph { get; set; } = FileGlyph;
        public Symbol Symbol { get; set; } = Symbol.OpenFile;

        public BitmapImage RawImage
        {
            get
            {
                try
                {
                    if (System.IO.File.Exists(File.GetAbsolutePath()))
                    {
                        var bitmapImage = new BitmapImage(new Uri(File.GetAbsolutePath(), UriKind.Absolute));
                        return bitmapImage;
                    }
                }
                catch (Exception ex)
                {
                }
                return null;
            }
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
