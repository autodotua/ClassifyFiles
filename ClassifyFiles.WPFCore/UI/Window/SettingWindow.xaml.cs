using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.WPFCore;
using FzLib.Basic;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using static ClassifyFiles.Util.ProjectUtility;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// LogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : WindowBase
    {
        public ObservableCollection<Project> Projects { get; }
        public static SettingWindow Current { get; private set; }

        public SettingWindow(ObservableCollection<Project> projects)
        {
            Current = this;
            InitializeComponent();

            int theme = Configs.Theme;
            switch (theme)
            {
                case 0: rbtnThemeAuto.IsChecked = true; break;
                case -1: rbtnThemeDark.IsChecked = true; break;
                case 1: rbtnThemeLight.IsChecked = true; break;
                default:
                    break;
            }
            chkAutoThumbnails.IsChecked = Configs.AutoThumbnails;
            numThread.Value = Configs.RefreshThreadCount;
            Projects = projects;
        }

        public int ProcessorCount => Environment.ProcessorCount;

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Configs.AutoThumbnails = chkAutoThumbnails.IsChecked.Value;
            //ConfigUtility.Set(ConfigKeys.IncludeThumbnailsWhenAddingFilesKey, chkIncludeThumbnailsWhenAddingFiles.IsChecked.Value);
        }


        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Current = null;
        }
        private void ThemeRButton_Click(object sender, RoutedEventArgs e)
        {
            int theme = rbtnThemeAuto.IsChecked.Value ?
                0 : (rbtnThemeLight.IsChecked.Value ? 1 : -1);
            Configs.Theme = theme;
            App.SetTheme();
        }

        private async void ZipDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            long rawLength = new FileInfo(DbUtility.DbPath).Length;
            await Task.Run(() => DbUtility.Zip());
            long nowLength = new FileInfo(DbUtility.DbPath).Length;
            await new MessageDialog().ShowAsync("压缩成功，当前数据库大小为" + FzLib.Basic.Number.ByteToFitString(nowLength)
                + "，" + System.Environment.NewLine + "共释放" + FzLib.Basic.Number.ByteToFitString(rawLength - nowLength), "压缩成功");
        }

        private async void DeleteProjects_Click(object sender, RoutedEventArgs e)
        {
            flyoutDeleteProjects.Hide();
            Progress.Show();
            Project project = null;
            await Task.Run(() =>
            {
                foreach (var p in GetProjects())
                {
                    DeleteProject(p);
                }
            });
            Projects.Clear();
            Projects.Add(project);
            Progress.Close();
            await new MessageDialog().ShowAsync("删除成功", "删除");
        }

        private async void ImportMenu_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Title = "请选择导入的文件",
            };
            dialog.Filters.Add(new CommonFileDialogFilter("SQLite数据库", "db"));
            dialog.Filters.Add(new CommonFileDialogFilter("所有文件", "*"));
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                Progress.Show();
                List<Project> projects = null;
                await Task.Run(() => projects = Import(path));
                Progress.Close();
                await new MessageDialog().ShowAsync("导入成功", "导出");
                projects.ForEach(p => Projects.Add(p));
            }

        }

        private async void ExportMenu_Click(object sender, RoutedEventArgs e)
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog()
            {
                Title = "请选择导出的位置",
                DefaultFileName = "文件分类"
            };
            dialog.Filters.Add(new CommonFileDialogFilter("SQLite数据库", "db"));
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                Progress.Show();
                await Task.Run(() => ExportAll(path));
                Progress.Close();
                await new MessageDialog().ShowAsync("导出成功", "导出");
            }

        }

        private void numThread_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            Configs.RefreshThreadCount = (int)numThread.Value;
            numThread.Value = (int)numThread.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new LogsWindow().Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Configs.AutoAddFiles = false;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //由于删除缩略图可能会影响正在显示的缩略图，因此先关闭窗体
            App.Current.Windows.Cast<Window>().Where(p => p != this).ForEach(p => p.Close());
            Progress.Show();
            (int DeleteFromDb, int DeleteFromDisk, int RemainsCount) result=(-1,-1,-1);
           await Task.Run(() => result = FileUtility.OptimizeThumbnails());
            App.Current.MainWindow = new MainWindow();
            App.Current.MainWindow.Show();
            BringToFront();
            Progress.Close();
            await new MessageDialog().ShowAsync($"修复成功，{Environment.NewLine}" +
                $"从数据库中删除了{result.DeleteFromDb}张缩略图，{Environment.NewLine}" +
                $"从磁盘中删除了{result.DeleteFromDisk}张缩略图，{Environment.NewLine}" +
                $"现存{result.RemainsCount}张缩略图，{Environment.NewLine}", "修复缩略图");
        }
    }
}
