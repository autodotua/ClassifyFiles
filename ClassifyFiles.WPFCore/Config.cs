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
    public static class ConfigKeys
    {
        /// <summary>
        /// 两色暗色主题。1为亮色，-1为暗色，0为跟随系统
        /// </summary>
        public const string ThemeKey = "Theme";

        public const string IncludeThumbnailsWhenAddingFilesKey = "IncludeThumbnailsWhenAddingFiles";
    }
}
