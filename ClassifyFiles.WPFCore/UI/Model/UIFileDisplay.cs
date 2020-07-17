using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace ClassifyFiles.UI.Model
{
    public class UIFileDisplay : INotifyPropertyChanged
    {
        public UIFileDisplay(File file)
        {
            File = file;
            FileInfo = new System.IO.FileInfo(file.GetAbsolutePath());
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
        public File File { get; private set; }
        public System.IO.FileInfo FileInfo { get; private set; }
        public long? length = null;
        public string Length
        {
            get
            {
                if(File.IsFolder)
                {
                    return "文件夹";
                }
                if (length == null)
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
                if(length==-1)
                {
                    return "未知";
                }
                return FzLib.Basic.Number.ByteToFitString(length.Value);
            }
        }
        public string DisplayName => File.IsFolder ? new System.IO.DirectoryInfo(File.Dir).Name : File.Name;
        public string DisplayDir => File.IsFolder ? File.Dir.Substring(0, File.Dir.Length - DisplayName.Length) : File.Dir;
        public bool ShowIconViewNames => Configs.ShowIconViewNames;
        public bool ShowTileViewPaths => Configs.ShowTilePath;
        public const string FileGlyph = "\uED41";
        public const string FolderGlyph = "\uED43";

        public event PropertyChangedEventHandler PropertyChanged;

        public string Glyph { get; set; } = FileGlyph;
        public Symbol Symbol { get; set; } = Symbol.OpenFile;
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
                            return new BitmapImage(new Uri(FileUtility.GetThumbnailPath(File.ThumbnailGUID), UriKind.Absolute));
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
                            return new BitmapImage(new Uri(FileUtility.GetIconPath(File.IconGUID), UriKind.Absolute));
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
