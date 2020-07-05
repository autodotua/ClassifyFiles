using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.WPFCore;
using FzLib.Extension;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateFilesWindow : WindowBase
    {
        public UpdateFilesWindow(Project project)
        {
            InitializeComponent();
            Project = project;
        }


        public Project Project { get; }

        private  async void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(working)
            {
                e.Cancel = true;
                if(stopping)
                {
                    return;
                }
                if(await new ConfirmDialog().ShowAsync("正在更新文件，是否强制停止？","停止"))
                {
                    stopping = true;
                }
            }
        }

        private void RegardOneSideParseErrorAsNotSameCheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
        bool working = false;
        bool stopping = false;  
        private bool Callback(double per, Data.File file)
        {
            if(stopping)
            {
                Dispatcher.Invoke(() =>
                {
                    working = false;
                    Close();
                });
                return false;
            }
            Percentage = per;
            Message = $"正在处理（{100 * per:N2}%）：" + file.GetAbsolutePath();
            return true;
        }
        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                this.Notify(nameof(Message));
            }
        }
        private double percentage;
        public double Percentage
        {
            get => percentage;
            set
            {
                percentage = value;
                this.Notify(nameof(Percentage));
            }
        }
        public bool Updated { get; private set; }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            stkSettings.IsEnabled = false;
            UpdateFilesArgs args = new UpdateFilesArgs()
            {
                Project = Project,
                Research = chkResearch.IsChecked.Value,
                Reclassify = chkReclassify.IsChecked.Value,
                IncludeThumbnails = chkIncludeThumbnails.IsChecked.Value,
                Callback = Callback
            };
            working = true;
            Message = "正在初始化";
            try
            {
                await DbUtility.UpdateFilesOfClassesAsync(args);
                working = false;
                Updated = true;
            }
            catch(Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "更新失败");
                working = false;
                Updated = true;
            }
            Close();
        }
    }
}
