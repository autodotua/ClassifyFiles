using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClassifyFiles.UI.Model
{

    public class FileType
    {
        static FileType()
        {
            FileTypes.Add(new FileType("Video", "\uE714", "mp4", "avi", "mkv", "mov", "rm", "rmvb"));
            FileTypes.Add(new FileType("Photo", "\uEB9F", "mp4", "avi", "mkv", "mov", "rm", "rmvb"));
            FileTypes.Add(new FileType("Audio", "\uE8D6", "mp3", "acc", "ogg", "flac", "wav"));
            FileTypes.Add(new FileType("Program", "\uE756", "exe", "msi", "apk", "dll", "ini", "xml"));
            FileTypes.Add(new FileType("Document", "\uE8A5", "doc", "docx", "ppt", "pptx", "xls", "xlsx", "txt", "md"));
        }
        public string Name { get; set; }
        public IReadOnlyList<string> Extensions { get; }
        public string Glyph { get; set; }

        public FileType(string name, string glyph, params string[] exts)
        {
            Name = name;
            Extensions = exts.ToList().AsReadOnly();
            Glyph = glyph;
        }

        public static List<FileType> FileTypes { get; private set; } = new List<FileType>();
    }
    public class FileWithIcon : File
    {
        public const string FileGlyph = "\uED41";
        public const string FolderGlyph = "\uED43";
        public string Glyph { get; set; } = FileGlyph;
        /// <summary>
        /// 无缩略图时的图标样式
        /// </summary>
        public Symbol Symbol { get; set; } = Symbol.OpenFile;
        /// <summary>
        /// 缩略图
        /// </summary>
        public BitmapImage Image
        {
            get
            {
                if (Thumbnail == null)
                {
                    return null;
                }
                return ToImage(Thumbnail);
            }
        }
        /// <summary>
        /// 将字符数组转换为<see cref="BitmapImage"/>
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private BitmapImage ToImage(byte[] array)
        {
            using var ms = new System.IO.MemoryStream(array);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // here
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
        /// <summary>
        /// 图标是否显示
        /// </summary>
        public Visibility IconVisibility => Image == null ? Visibility.Visible : Visibility.Collapsed;
        /// <summary>
        /// 缩略图是否显示
        /// </summary>
        public Visibility ImageVisibility => Image == null ? Visibility.Collapsed : Visibility.Visible;

        private static double defualtIconSize = 60;
        /// <summary>
        /// 默认大图标的大小
        /// </summary>
        public static double DefualtIconSize
        {
            get => defualtIconSize;
            set
            {
                if (value < 16 || value > 200)
                {
                    return;
                }
                defualtIconSize = value;
            }
        }

        public double LargeIconSize { get; private set; } = DefualtIconSize;
        public double SmallIconSize { get; private set; } = DefualtIconSize / 2;

        public void UpdateIconSize()
        {
            LargeIconSize = DefualtIconSize;
            SmallIconSize = DefualtIconSize / 2;
            this.Notify(nameof(LargeIconSize), nameof(SmallIconSize));
        }
        public FileWithIcon() { }

        public FileWithIcon(File file, bool tags = false, Project project = null)
        {
            Name = file.Name;
            Dir = file.Dir;
            SubFiles = file.SubFiles.Select(p => new FileWithIcon(p)).Cast<File>().ToList();
            Thumbnail = file.Thumbnail;
            if (Dir == null)
            {
                Glyph = FolderGlyph;
            }
            else
            {
                string ext = System.IO.Path.GetExtension(Name);
                if (ext.Length > 1)
                {
                    ext = ext.Substring(1);
                }
                else
                {

                }
                FileType type = FileType.FileTypes.FirstOrDefault(p => p.Extensions.Contains(ext));
                if (type != null)
                {
                    Glyph = type.Glyph;
                }
            }
            if (tags)
            {
                Tags.AddRange(DbUtility.GetTagsOfFile(project, Dir, Name).Result);
            }
        }

        public List<Tag> Tags { get; } = new List<Tag>();
        public string TagsString => string.Join(", ", Tags.Select(p => p.Name));

    }
}
