using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace ClassifyFiles.Data
{
    [DebuggerDisplay("{Type} {Value}")]
    public class MatchCondition:DbModelBase
    {
        public int Index { get; set; }

        public Logic ConnectionLogic { get; set; } = Logic.And;
        public MatchType Type { get; set; } = MatchType.InFileName;
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
        [Description("后缀名为")]
        WithExtension,
    }
}
