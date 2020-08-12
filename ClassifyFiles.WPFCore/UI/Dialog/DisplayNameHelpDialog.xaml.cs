using ClassifyFiles.UI.Util;
using System.Collections.Generic;
using System.IO;

namespace ClassifyFiles.UI.Dialog
{
    /// <summary>
    /// LogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DisplayNameHelpDialog : DialogWindowBase
    {
        public DisplayNameHelpDialog()
        {
            InitializeComponent();
            SmoothScrollViewerHelper.Regist(scr);
        }

        public Dictionary<string, string> DisplayNameFormatItem { get; } = new Dictionary<string, string>()
        {
            [nameof(FileInfo.Name)] = "文件名，不包含扩展名",
            [nameof(FileInfo.Extension)] = "文件扩展名",
        };
    }
}