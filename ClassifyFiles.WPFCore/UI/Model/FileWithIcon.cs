using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static ClassifyFiles.Data.Project;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;
using static ClassifyFiles.Util.DbUtility;

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
    public class UIFile : File
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
        public double FontSize { get; private set; } = DefualtIconSize / 3;

        public void UpdateIconSize()
        {
            LargeIconSize = DefualtIconSize;
            SmallIconSize = DefualtIconSize / 2;
            FontSize = DefualtIconSize / 3;
            this.Notify(nameof(LargeIconSize), nameof(SmallIconSize), nameof(FontSize));
        }
        public UIFile() { }
        public File Raw { get; private set; }
        public UIFile(File file)
        {
            Raw = file;
            ID = file.ID;
            Name = file.Name;
            Dir = file.Dir;
            Project = file.Project;
            ProjectID = file.ProjectID;
            SubFiles = file.SubFiles.Select(p => new UIFile(p)).Cast<File>().ToList();
            Thumbnail = file.Thumbnail;
            if (Dir == null)
            {
                Glyph = FolderGlyph;
            }
            else
            {
                string ext = System.IO.Path.GetExtension(Name).ToLower().TrimStart('.');
                FileType type = FileType.FileTypes.FirstOrDefault(p => p.Extensions.Contains(ext));
                if (type != null)
                {
                    Glyph = type.Glyph;
                }
            }
        }

        public async Task LoadTagsAsync(Project project)
        {
            IEnumerable<Class> classes = await GetClassesOfFileAsync(ID);
            classes = classes.OrderBy(p => p.Name);
            Classes = new ObservableCollection<Class>(classes);
        }

        public ObservableCollection<Class> Classes { get; private set; }

        public override string ToString()
        {
            return Name + (string.IsNullOrEmpty(Dir) ? "" : $" （{Dir}）");
        }
    }
}
