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
    public class UIFile : File
    {
        public UIFile() { }
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
            if (IsFolder)
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

                PropertyChanged +=async (p1, p2) =>
                {
                    if (p2.PropertyName == nameof(Thumbnail))
                    {
                        await Task.Delay(100);
                        this.Notify(nameof(IconVisibility), nameof(ImageVisibility),nameof(Image));
                    }
                };
            }
        }

        public string DisplayName => IsFolder ? new System.IO.DirectoryInfo(Dir).Name : Name;
        public string DisplayDir => IsFolder ? Dir.Substring(0, Dir.Length - DisplayName.Length) : Dir;
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
        public File Raw { get; private set; }

        public async Task LoadTagsAsync(AppDbContext db)
        {
            IEnumerable<Class> classes = await GetClassesOfFileAsync(db,ID);
            Classes = new ObservableCollection<Class>(classes);
        }

        private ObservableCollection<Class> classes;
        public ObservableCollection<Class> Classes
        {
            get => classes;
            set
            {
                classes = value;
                this.Notify(nameof(Classes));
            }
        }

        public override string ToString()
        {
            return Name + (string.IsNullOrEmpty(Dir) ? "" : $" （{Dir}）");
        }
    }
}
