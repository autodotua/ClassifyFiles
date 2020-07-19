using FzLib.Extension;
using ClassifyFiles.WPFCore;
using System.Windows.Media;
using System.ComponentModel;
using System;

namespace ClassifyFiles.UI.Model
{
    public static class UIFileSize
    {
        private static double defualtIconSize = Configs.IconSize;
        /// <summary>
        /// 默认图标的大小
        /// </summary>
        public static double DefaultIconSize
        {
            get => defualtIconSize;
            set
            {
                if (value < 16 || value > 256)
                {
                    return;
                }
                defualtIconSize = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DefaultIconSize)));
            }
        }
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

    }
}
