using FzLib.Extension;
using ClassifyFiles.WPFCore;
using System.Windows.Media;
using System.ComponentModel;

namespace ClassifyFiles.UI.Model
{
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
        private static double defualtIconSize = Configs.IconSize;
        /// <summary>
        /// 默认图标的大小
        /// </summary>
        public static double DefualtIconSize
        {
            get => defualtIconSize;
            set
            {
                if (value < 16 || value > 256)
                {
                    return;
                }
                defualtIconSize = value;
            }
        }

        public double IconSize { get; private set; } = DefualtIconSize;
        public double FontIconSize { get; private set; } = DefualtIconSize / 1.5;
        public double FontSize { get; private set; } = 12;
        public double SmallFontSize { get; private set; } = 11;
        private static FontFamily font;

        public event PropertyChangedEventHandler PropertyChanged;

        public double TotalIconViewHeight => Configs.ShowIconViewNames ? IconSize*2 + 16 * 2 + 8 : IconSize * 2;
        public double TotalTileViewHeight => IconSize * 2 + 32;
        public double TileTitleHeight => FontSize * font.LineSpacing * 2;
        public double TileDirHeight => SmallFontSize * font.LineSpacing * 2;

        public void UpdateIconSize()
        {
            IconSize = DefualtIconSize ;
            FontIconSize = DefualtIconSize / 1.5;
            this.Notify(nameof(IconSize), nameof(FontIconSize),nameof(TotalIconViewHeight),nameof(TotalTileViewHeight));
        }
    }
}
