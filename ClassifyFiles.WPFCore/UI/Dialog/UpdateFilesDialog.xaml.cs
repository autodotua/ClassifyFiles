﻿using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static ClassifyFiles.Util.FileClassUtility;

namespace ClassifyFiles.UI.Dialog
{
    /// <summary>
    /// LogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateFilesDialog : DialogWindowBase
    {
        public UpdateFilesDialog(Project project)
        {
            InitializeComponent();
            Project = project;
        }

        public Project Project { get; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (working)
            {
                e.Cancel = true;
                if (stopping)
                {
                    return;
                }
                stopping = true;
            }
        }

        private void RegardOneSideParseErrorAsNotSameCheckBox_Click(object sender, RoutedEventArgs e)
        {
        }

        private bool working = false;
        private bool stopping = false;

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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            stkSettings.IsEnabled = false;
            UpdateFilesArgs args = new UpdateFilesArgs()
            {
                Project = Project,
                Research = chkResearch.IsChecked.Value,
                Reclassify = chkReclassify.IsChecked.Value,
                GenerateThumbnailsMethod = chkIncludeThumbnails.IsChecked.Value ? FileIconUtility.TryGenerateAllFileIcons : (Action<File>)null,
                DeleteNonExistentItems = chkDeleteNonExistentItems.IsChecked.Value,
                Callback = Callback
            };
            working = true;
            Message = "正在初始化";
            try
            {
                await Task.Run(() => UpdateFilesOfClasses(args));
                working = false;
                Updated = true;
            }
            catch (Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "更新失败");
                working = false;
                Updated = false;
            }
            Close();
        }
    }
}