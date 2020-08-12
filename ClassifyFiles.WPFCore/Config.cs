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
using static ClassifyFiles.Util.ConfigUtility;

namespace ClassifyFiles
{
    public static class Configs
    {
        private static int? theme = null;
        public static int Theme
        {
            get
            {
                if (theme == null)
                {
                    theme = ConfigUtility.GetInt(nameof(Theme), 0);
                }
                return theme.Value;
            }
            set
            {
                theme = value;
                ConfigUtility.Set(nameof(Theme), value);
            }
        }
        private static bool? autoThumbnails = null;
        public static bool AutoThumbnails
        {
            get => Get(ref autoThumbnails, GetBool, true, nameof(AutoThumbnails));
            set => Set(ref autoThumbnails, value, nameof(AutoThumbnails));
        }
        private static int? refreshThreadCount = null;
        public static int RefreshThreadCount
        {
            get => Get(ref refreshThreadCount, GetInt, 4, nameof(RefreshThreadCount));
            set => Set(ref refreshThreadCount, value, nameof(RefreshThreadCount));
        }

        private static bool? showIconViewNames = null;
        public static bool ShowIconViewNames
        {
            get => Get(ref showIconViewNames, GetBool, true, nameof(ShowIconViewNames));
            set => Set(ref showIconViewNames, value, nameof(ShowIconViewNames));
        }
        private static bool? showClassTags = null;
        public static bool ShowClassTags
        {
            get => Get(ref showClassTags, GetBool, true, nameof(ShowClassTags));
            set => Set(ref showClassTags, value, nameof(ShowClassTags));
        }
        private static int? lastProjectID = null;
        public static int LastProjectID
        {
            get => Get(ref lastProjectID, GetInt, 1, nameof(LastProjectID));
            set => Set(ref lastProjectID, value, nameof(LastProjectID));
        }
        private static int? lastClassID = null;


        public static int LastClassID
        {
            get => Get(ref lastClassID, GetInt, 1, nameof(LastClassID));
            set => Set(ref lastClassID, value, nameof(LastClassID));
        }
        private static int? lastViewType = null;


        public static int LastViewType
        {
            get => Get(ref lastViewType, GetInt, 1, nameof(LastViewType));
            set => Set(ref lastViewType, value, nameof(LastViewType));
        }

        private static int? sortType = null;

        public static ThumbnailStrategy ThumbnailStrategy
        {
            get => (ThumbnailStrategy)Get(ref thumbnailStrategy, GetInt, 3, nameof(ThumbnailStrategy));
            set => Set(ref thumbnailStrategy, (int)value, nameof(ThumbnailStrategy));
        }

        private static int? thumbnailStrategy = null;

        public static int SortType
        {
            get => Get(ref sortType, GetInt, 0, nameof(SortType));
            set => Set(ref sortType, value, nameof(SortType));
        }
        private static double? iconSize = null;
        public static double IconSize
        {
            get => Get(ref iconSize, GetDouble, 32d, nameof(IconSize));
            set
            {
                if (value < 16)
                {
                    Set(ref iconSize, 16, nameof(IconSize));
                }
                else if (value > 256)
                {
                    Set(ref iconSize, 256, nameof(IconSize));
                }
                else
                {
                    Set(ref iconSize, value, nameof(IconSize));
                }
            }
        }

        private static bool? showTilePath = null;
        public static bool ShowTilePath
        {
            get => Get(ref showTilePath, GetBool, true, nameof(ShowTilePath));
            set => Set(ref showTilePath, value, nameof(ShowTilePath));
        }
        private static bool? groupByDir = null;
        public static bool GroupByDir
        {
            get => Get(ref groupByDir, GetBool, true, nameof(GroupByDir));
            set => Set(ref groupByDir, value, nameof(GroupByDir));
        }
        private static bool? showFileExtension = null;
        public static bool ShowFileExtension
        {
            get => Get(ref showFileExtension, GetBool, true, nameof(ShowFileExtension));
            set => Set(ref showFileExtension, value, nameof(ShowFileExtension));
        }
        private static bool? showFileTime = null;
        public static bool ShowFileTime
        {
            get => Get(ref showFileTime, GetBool, false, nameof(ShowFileTime));
            set => Set(ref showFileTime, value, nameof(ShowFileTime));
        }
        private static bool? showToolTip = null;
        public static bool ShowToolTip
        {
            get => Get(ref showToolTip, GetBool, true, nameof(ShowToolTip));
            set => Set(ref showToolTip, value, nameof(ShowToolTip));
        }
        private static bool? showToolTipImage = null;
        public static bool ShowToolTipImage
        {
            get => Get(ref showToolTipImage, GetBool, true, nameof(ShowToolTipImage));
            set => Set(ref showToolTipImage, value, nameof(ShowToolTipImage));
        }
        private static bool? autoAddFiles = null;
        public static bool AutoAddFiles
        {
            get => Get(ref autoAddFiles, GetBool, false, nameof(AutoAddFiles));
            set => Set(ref autoAddFiles, value, nameof(AutoAddFiles));
        }
        private static bool? cacheInTempDir = null;
        public static bool CacheInTempDir
        {
            get => Get(ref cacheInTempDir, GetBool, false, nameof(CacheInTempDir));
            set => Set(ref cacheInTempDir, value, nameof(CacheInTempDir));
        }
        private static string addFilesOptionJson = null;
        public static string AddFilesOptionJson
        {
            get => Get(ref addFilesOptionJson, GetString, "", nameof(AddFilesOptionJson));
            set => Set(ref addFilesOptionJson, value, nameof(AddFilesOptionJson));
        }
        private static bool? fileIconUniformToFill = null;
        public static bool FileIconUniformToFill
        {
            get => Get(ref fileIconUniformToFill, GetBool, false, nameof(FileIconUniformToFill));
            set => Set(ref fileIconUniformToFill, value, nameof(FileIconUniformToFill));
        }
        private static bool? treeSimpleTemplate = null;
        public static bool TreeSimpleTemplate
        {
            get => Get(ref treeSimpleTemplate, GetBool, true, nameof(TreeSimpleTemplate));
            set => Set(ref treeSimpleTemplate, value, nameof(TreeSimpleTemplate));
        }
        private static bool? smoothScroll = null;
        public static bool SmoothScroll
        {
            get => Get(ref smoothScroll, GetBool, false, nameof(SmoothScroll));
            set => Set(ref smoothScroll, value, nameof(SmoothScroll));
        }


        private static T Get<T>(ref T? field, Func<string, T, T> dbGet, T defultValue, string key) where T : struct
        {
            if (field == null)
            {
                field = dbGet(key, defultValue);
            }
            return field.Value;
        }
        private static T Get<T>(ref T field, Func<string, T, T> dbGet, T defultValue, string key) where T : class
        {
            if (field == null)
            {
                field = dbGet(key, defultValue);
            }
            return field;
        }

        private static bool? hasOpened = null;
        public static bool HasOpened
        {
            get => Get(ref hasOpened, GetBool, false, nameof(HasOpened));
            set => Set(ref hasOpened, value, nameof(HasOpened));
        }
        private static bool? paneDisplayLeftMinimal = null;
        public static bool PaneDisplayLeftMinimal
        {
            get => Get(ref paneDisplayLeftMinimal, GetBool, false, nameof(PaneDisplayLeftMinimal));
            set => Set(ref paneDisplayLeftMinimal, value, nameof(PaneDisplayLeftMinimal));
        }
        private static bool? fluencyFirst = null;
        public static bool FluencyFirst
        {
            get => Get(ref fluencyFirst, GetBool, false, nameof(FluencyFirst));
            set => Set(ref fluencyFirst, value, nameof(FluencyFirst));
        }


        public static TimeSpan AnimationDuration { get; } = TimeSpan.FromSeconds(0.2);

        private static void Set<T>(ref T? field, T value, string key) where T : struct
        {
            field = value;
            ConfigUtility.Set(key, value);
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(key));
        }
        private static void Set<T>(ref T field, T value, string key) where T : class
        {
            field = value;
            ConfigUtility.Set(key, value);
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(key));
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
    }

    public enum ThumbnailStrategy
    {
        [Description("默认图标")]
        None,
        [Description("资源管理器图标")]
        WindowsExplorerIcon,
        [Description("优先显示多媒体缩略图")]
        MediaThumbnailPrefer,
        [Description("Windows10的缩略图API")]
        Win10Icon
    }
}
