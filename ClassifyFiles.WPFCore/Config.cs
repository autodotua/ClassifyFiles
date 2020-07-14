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
        public static void InitializeAllValues()
        {
            var a = Theme;
            var b = AutoThumbnails;
            var c = ShowExplorerIcon;
            var d = showThumbnail;
            var e = RefreshThreadCount;
        }
        /// <summary>
        /// 两色暗色主题。1为亮色，-1为暗色，0为跟随系统
        /// </summary>
        private const string ThemeKey = "Theme";

        private const string AutoThumbnailsKey = "AutoThumbnails";
        private const string RefreshThreadCountKey = "RefreshThreadCount";
        private const string ShowExplorerIconKey = "ShowExplorerIcon";
        private const string ShowThumbnailKey = "ShowThumbnail";

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
        private static bool? showExplorerIcon = null;
        public static bool ShowExplorerIcon
        {
            get
            {
                if (showExplorerIcon == null)
                {
                    showExplorerIcon = ConfigUtility.GetBool(ShowExplorerIconKey, true);
                }
                return showExplorerIcon.Value;
            }
            set
            {
                showExplorerIcon = value;
                ConfigUtility.Set(ShowExplorerIconKey, value);
            }
        }

        private static bool? showThumbnail = null;
        public static bool ShowThumbnail
        {
            get
            {
                if (showThumbnail == null)
                {
                    showThumbnail = ConfigUtility.GetBool(ShowThumbnailKey, true);
                }
                return showThumbnail.Value;
            }
            set
            {
                showThumbnail = value;
                ConfigUtility.Set(ShowThumbnailKey, value);
            }
        }
    }
}
