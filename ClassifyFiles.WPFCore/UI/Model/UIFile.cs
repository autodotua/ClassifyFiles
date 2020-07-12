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
using ClassifyFiles.WPFCore;
using System.Windows.Media;
using ClassifyFiles.UI.Component;

namespace ClassifyFiles.UI.Model
{
    public class UIFile : File
    {
        public UIFile()
        {
            if (font == null)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    font = App.Current.MainWindow.FontFamily;
                });
            }
        }
        public UIFile(File file) : this()
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

                PropertyChanged += async (p1, p2) =>
                 {
                     if (p2.PropertyName == nameof(Thumbnail))
                     {
                         this.Notify(/*nameof(IconVisibility), nameof(ImageVisibility),*/ nameof(Image));
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
        ///// <summary>
        ///// 图标是否显示
        ///// </summary>
        //public Visibility IconVisibility => Image == null ? Visibility.Visible : Visibility.Collapsed;
        ///// <summary>
        ///// 缩略图是否显示
        ///// </summary>
        //public Visibility ImageVisibility => Image == null ? Visibility.Collapsed : Visibility.Visible;

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
        public double SmallFontIconSize { get; private set; } = DefualtIconSize / 3;
        public double LargeFontIconSize { get; private set; } = DefualtIconSize / 1.5;
        public double FontSize { get; private set; } = 12;
        public double SmallFontSize { get; private set; } = 11;
        private static FontFamily font;
        public double TotalIconViewHeight => LargeIconSize + FontSize * font.LineSpacing * 2;
        public double TotalTileViewHeight => LargeIconSize + 32;
        public double TileTitleHeight => FontSize * font.LineSpacing * 2;
        public double TileDirHeight => SmallFontSize * font.LineSpacing * 2;

        public void UpdateIconSize()
        {
            LargeIconSize = DefualtIconSize;
            SmallIconSize = DefualtIconSize / 2;
            SmallFontIconSize = DefualtIconSize / 3;
            LargeFontIconSize = DefualtIconSize / 1.5;
            this.Notify(nameof(LargeIconSize), nameof(SmallIconSize), nameof(SmallFontIconSize), nameof(LargeFontIconSize), nameof(TotalIconViewHeight));
        }
        public File Raw { get; private set; }
        private bool loaded = false;
        public async Task LoadAsync(AppDbContext db = null)
        {
            if (!loaded)
            {
                loaded = true;
                IEnumerable<Class> classes = null;
                if (db == null)
                {
                    classes = await GetClassesOfFileAsync(ID);
                }
                else
                {
                    classes = await GetClassesOfFileAsync(db, ID);
                }
                Classes = new ObservableCollection<Class>(classes);
            }
            Load?.Invoke(this, new EventArgs());
        }
        public event EventHandler Load;

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
