using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace ClassifyFiles.Data
{
    [DebuggerDisplay("{Type} {Value}")]
    public class MatchCondition : DbModelBase
    {
        public int Index { get; set; }
        public bool Not { get; set; }
        public Logic ConnectionLogic { get; set; } = Logic.And;
        private MatchType type = MatchType.InFileName;
        public MatchType Type
        {
            get => type;
            set
            {
                type = value;
                this.Notify(nameof(Type));
            }
        }

        [Required]
        public string Value { get; set; } = "";
        public Class Class { get; set; }
        public int ClassID { get; set; }
    }

    /// <summary>
    /// 连接逻辑方式
    /// </summary>
    public enum Logic
    {
        [Description("并且")]
        And=1,
        [Description("或者")]
        Or=2,
    }

    /// <summary>
    /// 匹配方式
    /// </summary>
    public enum MatchType
    {
        [Description("在文件名中包含")]
        InFileName=1,
        [Description("在目录名中包含")]
        InDirName=2,
        [Description("在路径中包含")]
        InPath=3,
        [Description("文件名正则匹配")]
        InFileNameWithRegex=4,
        [Description("目录名正则匹配")]
        InDirNameWithRegex=5,
        [Description("路径正则匹配")]
        InPathWithRegex=6,
        [Description("后缀名为")]
        WithExtension=7,
        [Description("文件尺寸小于")]
        SizeSmallerThan=8,
        [Description("文件尺寸大于")]
        SizeLargerThan=9,
        [Description("修改时间早于")]
        TimeEarlierThan=10,
        [Description("修改时间晚于")]
        TimeLaterThan=11
    }
}
