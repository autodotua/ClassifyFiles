using ClassifyFiles.Util;
using ClassifyFiles.WPFCore;
using FzLib.Extension;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static ClassifyFiles.Data.Project;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;
using static ClassifyFiles.Util.DbUtility;
using System.Collections.ObjectModel;
using ClassifyFiles.Data;
using FzLib.Basic;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : WindowBase
    {
        public ObservableCollection<Project> Projects { get; }

        public SettingWindow(ObservableCollection<Project> projects)
        {
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
            Projects = projects;
            //if(ConfigUtility.GetBool(ConfigKeys.IncludeThumbnailsWhenAddingFilesKey,true))
            //{
            //    chkIncludeThumbnailsWhenAddingFiles.IsChecked = true;
            //}
        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Configs.AutoThumbnails = chkAutoThumbnails.IsChecked.Value;
            //ConfigUtility.Set(ConfigKeys.IncludeThumbnailsWhenAddingFilesKey, chkIncludeThumbnailsWhenAddingFiles.IsChecked.Value);
        }


        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
            await DbUtility.ZipAsync();
            long nowLength = new FileInfo(DbUtility.DbPath).Length;
            await new MessageDialog().ShowAsync("压缩成功，当前数据库大小为" + FzLib.Basic.Number.ByteToFitString(nowLength)
                + "，" + System.Environment.NewLine + "共释放" + FzLib.Basic.Number.ByteToFitString(rawLength - nowLength), "压缩成功");
        }

        private async void DeleteProjects_Click(object sender, RoutedEventArgs e)
        {
            flyoutDeleteProjects.Hide();
            Progress.Show(true);
            foreach (var project in await GetProjectsAsync())
            {
                await DeleteProjectAsync(project);
            }
            Projects.Clear();
            Projects.Add(await AddProjectAsync());
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
                Progress.Show(true);
                var projects = await ImportAsync(path);
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
                Progress.Show(true);
                await ExportAllAsync(path);
                Progress.Close();
                await new MessageDialog().ShowAsync("导出成功", "导出");
            }

        }
    }
}
