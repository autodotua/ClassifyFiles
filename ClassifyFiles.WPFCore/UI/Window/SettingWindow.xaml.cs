using ClassifyFiles.Data;
using ClassifyFiles.UI.Component;
using ClassifyFiles.UI.Util;
using ClassifyFiles.Util;
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
using F = System.IO.File;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// LogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : WindowBase, IWithProcessRing
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
                default: throw new Exception();
            }

            if (!FileUtility.CanWriteInCurrentDirectory())
            {
                swtDbInAppDataFolder.IsOn = true;
                swtDbInAppDataFolder.IsEnabled = false;
            }
            else if (F.Exists(DbUtility.DbInAppDataFolderMarkerFileName))
            {
                swtDbInAppDataFolder.IsOn = true;
            }
            Projects = projects;
            RefreshCacheText();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Current = null;
        }
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
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
            Project project = null;
            await DoProcessAsync(Do());
            async Task Do()
            {
                await Task.Run(() =>
            {
                foreach (var p in GetProjects())
                {
                    DeleteProject(p);
                }
            });
                Projects.Clear();
                Projects.Add(project);
            }
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
                List<Project> projects = null;
                await DoProcessAsync(Task.Run(() => projects = Import(path)));

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
                await DoProcessAsync(Task.Run(() => ExportAll(path)));
                await new MessageDialog().ShowAsync("导出成功", "导出");
            }

        }

        private void OpenLogButton_Click(object sender, RoutedEventArgs e)
        {
            new LogsWindow().Show();
        }

        private void ResetAutoAddFilesButton_Click(object sender, RoutedEventArgs e)
        {
            Configs.AutoAddFiles = false;
        }

        private async void OptimizeThumbnailsAndIconsButton_Click(object sender, RoutedEventArgs e)
        {
            (int DeleteFromDb, int DeleteFromDisk, int RemainsCount, List<string> FailedToDelete) result = (-1, -1, -1, new List<string>());

            //由于删除缩略图可能会影响正在显示的缩略图，因此先关闭窗体
            result = await DoSthNeedToCloseOtherWindowsAsync(() => FileUtility.OptimizeThumbnailsAndIcons(FileIconUtility.ThumbnailFolderPath));
            await new MessageDialog().ShowAsync($"修复成功，{Environment.NewLine}" +
                $"从数据库中删除了{result.DeleteFromDb}张缩略图，{Environment.NewLine}" +
                $"从磁盘中删除了{result.DeleteFromDisk}张缩略图，{Environment.NewLine}" +
                $"现存{result.RemainsCount}张缩略图，{Environment.NewLine}" +
                $"有{result.FailedToDelete.Count}张缩略图删除失败", "修复缩略图");
        }

        /// <summary>
        /// 执行需要关闭其他窗体的任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private async Task<T> DoSthNeedToCloseOtherWindowsAsync<T>(Func<T> func)
        {
            await DoProcessAsync(Do());
            T result = default;
            async Task Do()
            {
                Owner = null;
                if (!MainWindow.Current.IsClosed)
                {
                    await MainWindow.Current.BeforeClosing(false);
                    MainWindow.Current.Close();
                }
                var windows = App.Current.Windows.Cast<Window>()
                    .Where(p => p != this && !(p is MainWindow));
                windows.ForEach(p => p.Close());
                while (FileIcon.Tasks.IsExcuting)
                {
                    //等待任务结束
                    await Task.Delay(1);
                }
                await Task.Run(() =>
                {
                    result = func();
                });
                await DbUtility.ReplaceDbContextAsync();
                App.Current.MainWindow = new MainWindow();
                bool canReturn = false;
                App.Current.MainWindow.ContentRendered += (p1, p2) =>
                {
                    BringToFront();
                    canReturn = true;
                };
                App.Current.MainWindow.Show();
                while (!canReturn)
                {
                    await Task.Delay(20);
                }
            }
            return result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SmoothScrollViewerHelper.Regist(scr);
        }

        private async void ChangeThumbnailPositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (await new ConfirmDialog().ShowAsync("更改缓存目录需要删除所有的缓存，是否继续？", "修改缓存目录"))
            {
                await DoSthNeedToCloseOtherWindowsAsync(Do);
                RefreshCacheText();
                await new MessageDialog().ShowAsync("更改完成。正在浏览的项目可能部分图标会无法显示，建议进行删除缩略图操作", "更改完成");
            };
            object Do()
            {
                RealtimeUpdate.ClearCahces();
                FileUtility.DeleteAllThumbnails();
                FileIconUtility.DeleteAllThumbnails();
                Configs.CacheInTempDir = !Configs.CacheInTempDir;
                FileIconUtility.UpdateSettings();

                return null;
            }
        }
        private async void DeleteThumbnailButton_Click(object sender, RoutedEventArgs e)
        {

            await DoSthNeedToCloseOtherWindowsAsync(Do);

            static object Do()
            {
                RealtimeUpdate.ClearCahces();
                FileUtility.DeleteAllThumbnails();
                FileIconUtility.DeleteAllThumbnails();

                return null;
            }
        }

        private void RefreshCacheText()
        {
            if (Configs.CacheInTempDir)
            {
                tbkCachePath.Text = "临时目录";
                runCachePathTo.Text = "默认位置";
            }
            else
            {

                tbkCachePath.Text = "默认位置";
                runCachePathTo.Text = "临时目录";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", FileIconUtility.ThumbnailFolderPath);
        }

        public async Task DoProcessAsync(Task task)
        {
            ring.Show();
            try
            {
                await task;
            }
            catch
            {

            }
            finally
            {
                ring.Close();
            }
        }

        public void SetProcessRingMessage(string message)
        {
            Dispatcher.BeginInvoke((Action)(() =>
               ring.Message = message));
        }

        private void swtDbInAppDataFolder_Toggled(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            string path = DbUtility.DbInAppDataFolderMarkerFileName;
            if (F.Exists(path) && !swtDbInAppDataFolder.IsOn)
            {
                F.Delete(path);
            }
            else if (!F.Exists(path) && swtDbInAppDataFolder.IsOn)
            {
                F.Create(path);
            }
        }
    }
}
