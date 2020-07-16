using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static ClassifyFiles.Util.FileClassUtility;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddFilesWindow : DialogWindowBase
    {
        public AddFilesWindow(Class c, IList<string> files)
        {
            InitializeComponent();
            Class = c;
            Files = files;
        }

        private async void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (working)
            {
                e.Cancel = true;
                if (stopping)
                {
                    return;
                }
                if (await new ConfirmDialog().ShowAsync("正在更新文件，是否强制停止？", "停止"))
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
            if (stopping)
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
        public Class Class { get; }
        public IList<string> Files { get; }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            stkSettings.IsEnabled = false;
            IList<string> files = null;

            if (rbtnFolderAsFile.IsChecked.Value)
            {
                files = Files;
            }
            else if(rbtnFolderIgnore.IsChecked.Value)
            {
                files = Files.Where(p => System.IO.File.Exists(p)).ToList();
            }
            else
            {
                files = new List<string>();
                foreach (var file in Files)
                {
                    if(System.IO.File.Exists(file))
                    {
                        files.Add(file);
                    }
                    else if(System.IO.Directory.Exists(file))
                    {
                        (files as List<string>).AddRange(System.IO.Directory.EnumerateFiles(file, "*", System.IO.SearchOption.AllDirectories)); ;
                    }
                }
            }
            UpdateFilesArgs args = new UpdateFilesArgs()
            {
                Project = Class.Project,
                Class = Class,
                Files = files,
                IncludeThumbnails = chkIncludeThumbnails.IsChecked.Value,
                IncludeExplorerIcons= chkIncludeExplorerIcons.IsChecked.Value,
                Callback = Callback
            };
            working = true;
            Message = "正在初始化";
            try
            {
                AddedFiles= await AddFilesToClassAsync(args);
                working = false;
                Updated = true;
            }
            catch (Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "更新失败");
                working = false;
                Updated = true;
            }
            Close();
        }

        public IReadOnlyList<File> AddedFiles { get; private set; }
    }
}
