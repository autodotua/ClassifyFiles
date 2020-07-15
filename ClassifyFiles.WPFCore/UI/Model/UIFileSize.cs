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
