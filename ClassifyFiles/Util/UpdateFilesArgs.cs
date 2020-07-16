using ClassifyFiles.Data;
using System;
using System.Collections.Generic;

namespace ClassifyFiles.Util
{
    public class UpdateFilesArgs
    {
        public IList<string> Files { get; set; }
        public bool Research { get; set; }
        public Project Project { get; set; }
        public bool IncludeThumbnails { get; set; }
        public bool IncludeExplorerIcons { get; set; }
        /// <summary>
        /// 第一个参数是百分比（0~1），第二个参数是当前的文件，返回值为是否继续
        /// </summary>
        public Func<double, File, bool> Callback { get; set; }
        public bool Reclassify { get; set; }
        public Class Class { get; set; }
    }
}
