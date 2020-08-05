using ClassifyFiles.Data;
using ClassifyFiles.UI.Util;
using ClassifyFiles.Util;
using FzLib.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static ClassifyFiles.Util.FileClassUtility;

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

        public Dictionary<string,string> DisplayNameFormatItem { get; } = new Dictionary<string, string>()
        {
            [nameof(FileInfo.Name)] = "文件名，不包含扩展名",
            [nameof(FileInfo.Extension)] = "文件扩展名",
        };



    }
}
