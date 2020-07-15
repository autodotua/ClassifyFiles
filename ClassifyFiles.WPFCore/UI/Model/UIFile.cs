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
using System.ComponentModel;

namespace ClassifyFiles.UI.Model
{
    public class UIFile : INotifyPropertyChanged
    {
        public UIFile()
        {

        }
        public UIFile(File file) : this()
        {
            File = file;
            SubUIFiles = file.SubFiles.Select(p => new UIFile(p)).ToList();
            Display = new UIFileDisplay(file);
            Size = new UIFileSize();

        }
        public List<UIFile> SubUIFiles { get; private set; } = new List<UIFile>();


        public File File { get; private set; }
        private bool loaded = false;
        public UIFileSize Size { get; set; }
        public UIFileDisplay Display { get; set; }
        public async Task LoadAsync(AppDbContext db = null)
        {
            if (!loaded)
            {
                loaded = true;
                IEnumerable<Class> classes;
                if (db == null)
                {
                    classes = await GetClassesOfFileAsync(File.ID);
                }
                else
                {
                    classes = await GetClassesOfFileAsync(db, File.ID);
                }
                Classes = new ObservableCollection<Class>(classes);
            }
            Load?.Invoke(this, new EventArgs());
        }
        public event EventHandler Load;
        public event PropertyChangedEventHandler PropertyChanged;

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
            return File.Name + (string.IsNullOrEmpty(File.Dir) ? "" : $" （{File.Dir}）");
        }
    }

    public class UIFileDisplay:INotifyPropertyChanged
    {
        public UIFileDisplay(File file)
        {
            File = file;

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
        public string DisplayName => File.IsFolder ? new System.IO.DirectoryInfo(File.Dir).Name : File.Name;
        public string DisplayDir => File.IsFolder ? File.Dir.Substring(0, File.Dir.Length - DisplayName.Length) : File.Dir;
        public bool ShowIconViewNames => Configs.ShowIconViewNames;
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
    public class UIFileSize : INotifyPropertyChanged
    {
        public UIFileSize()
        {
            if (font == null)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    font = App.Current.MainWindow.FontFamily;
                });
            }
        }
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

        public event PropertyChangedEventHandler PropertyChanged;

        public double TotalIconViewHeight => Configs.ShowIconViewNames ? LargeIconSize + 16 * 2 + 8 : LargeIconSize;
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
    }
}
