using ClassifyFiles.Data;
using ClassifyFiles.UI.Util;
using ClassifyFiles.Util;
using ClassifyFiles.Util.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;

namespace ClassifyFiles.UI.Page
{
    public partial class ProjectSettingsPanel : ProjectPageBase
    {
        public string Splitter { get; set; } = "-";

        public ProjectSettingsPanel()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SmoothScrollViewerHelper.Regist(scr);
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
            int filesCount = 0;
            int classesCount = 0;
            int fileClassesCount = 0;
            await Task.Run(() =>
            {
                filesCount = GetFilesCount(project);
                classesCount = GetClassesCount(project);
                fileClassesCount = GetFileClassesCount(project);
            });
            tbkFilesCount.Text = filesCount.ToString();
            tbkClassesCount.Text = classesCount.ToString();
            tbkFileClassesCount.Text = fileClassesCount.ToString();
        }

        public ExportFormat ExportFormat { get; set; }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            flyoutDelete.Hide();
            MainWindow mainWin = (Window.GetWindow(this) as MainWindow);
            await mainWin.DeleteSelectedProjectAsync();
        }

        private void ShowProgressMessage(Data.File file)
        {
            Dispatcher.Invoke(() =>
                MainWindow.Current.SetProcessRingMessage(
                    "正在导出" + Environment.NewLine + Path.Combine(file.Dir, file.Name)));
        }

        private async void ExportLinkButton_Click(object sender, RoutedEventArgs e)
        {
            CommonFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "请选择用于存放快捷方式的文件夹"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName; await MainWindow.Current.DoProcessAsync(
                     Task.Run(() =>
                {
                    FileUtility.Export(path, Project, ExportFormat, LinkUtility.CreateLink, Splitter, ShowProgressMessage);
                }));
            }
        }

        private async void ExportFileButton_Click(object sender, RoutedEventArgs e)
        {
            CommonFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "请选择导出的目标文件夹"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                await MainWindow.Current.DoProcessAsync(Task.Run(() =>
                {
                    FileUtility.Export(path, Project, ExportFormat, (from, to) => System.IO.File.Copy(from, to, true),
                                      Splitter, ShowProgressMessage);
                }));
            }
        }

        private async void ExportProjectButton_Click(object sender, RoutedEventArgs e)
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog()
            {
                Title = "请选择导出的位置",
                DefaultFileName = Project.Name
            };
            dialog.Filters.Add(new CommonFileDialogFilter("SQLite数据库", "db"));
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                await MainWindow.Current.DoProcessAsync(Task.Run(() => ExportProject(path, Project)));
                await new MessageDialog().ShowAsync("导出成功", "导出");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "请选择文件夹"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                Project.RootPath = dialog.FileName;
            }
        }

        private async void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            flyoutDeleteFiles.Hide();
            await MainWindow.Current.DoProcessAsync(Task.Run(() =>
            {
                DeleteFilesOfProject(Project);
            }));
            await new MessageDialog().ShowAsync("删除成功", "删除文件");
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<Data.File> files = null;
            await MainWindow.Current.DoProcessAsync(Task.Run(() => files = CheckFiles(Project)));
            if (files.Count == 0)
            {
                await new MessageDialog().ShowAsync("没有发现不存在的文件", "文件完整性检查");
            }
            else
            {
                await new ErrorDialog().ShowAsync($"发现{files.Count}个不存在的文件（夹）", "文件完整性检查",
                    string.Join(Environment.NewLine, files.Select(p => Path.Combine(p.Dir, p.Name))));
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            flyoutDeleteFileClasses.Hide();
            await MainWindow.Current.DoProcessAsync(Task.Run(() =>
            {
                DeleteAllFileClasses(Project);
            }));
            await new MessageDialog().ShowAsync("删除成功", "删除文件分类关系");
        }
    }
}