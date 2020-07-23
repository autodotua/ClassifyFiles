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
        public static void InitializeAllValues()
        {
            var a = Theme;
            var b = AutoThumbnails;
            var c = ShowExplorerIcon;
            var d = showThumbnail;
            var e = RefreshThreadCount;
        }

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
        private static bool? showExplorerIcon = null;
        public static bool ShowExplorerIcon
        {
            get => Get(ref showExplorerIcon, GetBool, false, nameof(ShowExplorerIcon));
            set => Set(ref showExplorerIcon, value, nameof(ShowExplorerIcon));
        }

        private static bool? showThumbnail = null;
        public static bool ShowThumbnail
        {
            get => Get(ref showThumbnail, GetBool, true, nameof(ShowThumbnail));
            set => Set(ref showThumbnail, value, nameof(ShowThumbnail));
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
                if(value < 16 || value > 256)
                {
                    return;
                }
                Set(ref iconSize, value, nameof(IconSize));
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
        private static string addFilesOptionJson = null;
        public static string AddFilesOptionJson
        {
            get => Get(ref addFilesOptionJson, GetString, "", nameof(AddFilesOptionJson));
            set => Set(ref addFilesOptionJson, value, nameof(AddFilesOptionJson));
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
}
