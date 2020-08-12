using System.ComponentModel;

namespace ClassifyFiles.Util
{
    public enum ExportFormat
    {
        [Description("文件名")]
        FileName,

        [Description("路径")]
        Path,

        [Description("树型")]
        Tree
    }
}