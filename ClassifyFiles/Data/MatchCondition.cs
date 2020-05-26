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
        And,
        [Description("或者")]
        Or,
    }

    /// <summary>
    /// 匹配方式
    /// </summary>
    public enum MatchType
    {
        [Description("在文件名中包含")]
        InFileName,
        [Description("在目录名中包含")]
        InDirName,
        [Description("在路径中包含")]
        InPath,
        [Description("文件名正则匹配")]
        InFileNameWithRegex,
        [Description("目录名正则匹配")]
        InDirNameWithRegex,
        [Description("路径正则匹配")]
        InPathWithRegex,
        [Description("后缀名为")]
        WithExtension,
        [Description("文件尺寸小于")]
        SizeSmallerThan,
        [Description("文件尺寸大于")]
        SizeLargerThan,
        [Description("修改时间早于")]
        TimeEarlierThan,
        [Description("修改时间晚于")]
        TimeLaterThan
    }
}
