using ClassifyFiles.Util;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace ClassifyFiles
{
    public static class Configs
    {
        /// <summary>
        /// 两色暗色主题。1为亮色，-1为暗色，0为跟随系统
        /// </summary>
        private const string ThemeKey = "Theme";

        private const string AutoThumbnailsKey = "AutoThumbnails";
        private const string RefreshThreadCountKey = "RefreshThreadCount";

        private static int? theme = null;
        public static int Theme
        {
            get
            {
                if (theme == null)
                {
                    theme = ConfigUtility.GetInt(ThemeKey, 0);
                }
                return theme.Value;
            }
            set
            {
                theme = value;
                ConfigUtility.Set(ThemeKey, value);
            }
        }
        private static bool? autoThumbnails = null;
        public static bool AutoThumbnails
        {
            get
            {
                if (autoThumbnails == null)
                {
                    autoThumbnails = ConfigUtility.GetBool(AutoThumbnailsKey, true);
                }
                return autoThumbnails.Value;
            }
            set
            {
                autoThumbnails = value;
                ConfigUtility.Set(AutoThumbnailsKey, value);
            }
        }
        private static int? refreshThreadCount = null;
        public static int RefreshThreadCount
        {
            get
            {
                if (refreshThreadCount == null)
                {
                    refreshThreadCount = ConfigUtility.GetInt(RefreshThreadCountKey, 4);
                }
                return refreshThreadCount.Value;
            }
            set
            {
                refreshThreadCount = value;
                ConfigUtility.Set(RefreshThreadCountKey, value);
            }
        }
    }
}
