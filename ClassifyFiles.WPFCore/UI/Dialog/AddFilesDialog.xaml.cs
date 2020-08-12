using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static ClassifyFiles.Util.FileClassUtility;

namespace ClassifyFiles.UI.Dialog
{
    /// <summary>
    /// LogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddFilesDialog : DialogWindowBase
    {
        public AddFilesDialog(Class c, IList<string> files)
        {
            InitializeComponent();
            Class = c;
            Files = files;

            if (Configs.AddFilesOptionJson.Length == 0)
            {
                return;
            }
            dynamic obj = JObject.Parse(Configs.AddFilesOptionJson);
            switch ((int)obj.FolderMode.Value)
            {
                case 1:
                    rbtnFolderAsFile.IsChecked = true;
                    break;

                case 2:
                    rbtnFolderIgnore.IsChecked = true;
                    break;

                case 3:
                    rbtnFolderImportFiles.IsChecked = true;
                    break;
            }
            chkIncludeThumbnails.IsChecked = obj.IncludeThumbnails.Value;
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
        public Class Class { get; }
        public IList<string> Files { get; }

        private async Task Do()
        {
            btnStart.IsEnabled = false;
            stkSettings.IsEnabled = false;
            IList<string> files = null;

            if (rbtnFolderAsFile.IsChecked.Value)
            {
                files = Files;
            }
            else if (rbtnFolderIgnore.IsChecked.Value)
            {
                files = Files.Where(p => System.IO.File.Exists(p)).ToList();
            }
            else
            {
                files = new List<string>();
                foreach (var file in Files)
                {
                    if (System.IO.File.Exists(file))
                    {
                        files.Add(file);
                    }
                    else if (System.IO.Directory.Exists(file))
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
                GenerateThumbnailsMethod = chkIncludeThumbnails.IsChecked.Value ? FileIconUtility.TryGenerateAllFileIcons : (Action<File>)null,
                Callback = Callback
            };
            working = true;
            Message = "正在初始化";
            try
            {
                await Task.Run(() =>
                {
                    AddedFiles = AddFilesToClass(args);
                });
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(new
            {
                FolderMode = rbtnFolderAsFile.IsChecked.Value ? 1 :
                (rbtnFolderIgnore.IsChecked.Value ? 2 : 3),
                IncludeThumbnails = chkIncludeThumbnails.IsChecked.Value
            });
            Configs.AddFilesOptionJson = json;
            await Do();
        }

        public IReadOnlyList<File> AddedFiles { get; private set; }

        private async void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (Configs.AutoAddFiles)
            {
                await Do();
            }
        }
    }
}