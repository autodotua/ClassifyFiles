using FzLib.Basic;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using ClassifyFiles.UI.Panel;
using System.Diagnostics;
using DImg = System.Drawing.Image;
using ModernWpf.Controls;
using ClassifyFiles.UI.Model;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using FzLib.Program;
using Microsoft.WindowsAPICodePack.Dialogs;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;
using ClassifyFiles.UI.Component;
using ClassifyFiles.UI.Util;

namespace ClassifyFiles.UI.Page
{
    public partial class ProjectSettingsPanel : ProjectPageBase
    {
        public string Splitter { get; set; } = "-";
        public ProjectSettingsPanel()
        {
            InitializeComponent();
        }


        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
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
        void ShowProgressMessage(Data.File file)
        {
            Dispatcher.Invoke(() => GetProgress().Message = "正在导出" + Environment.NewLine + Path.Combine(file.Dir, file.Name));
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
                string path = dialog.FileName;
                GetProgress().Show();
                await Task.Run(() =>
                {
                    FileUtility.Export(path, Project, ExportFormat, LinkUtility.CreateLink, Splitter, ShowProgressMessage);
                });

                GetProgress().Close();
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
                GetProgress().Show();
                await Task.Run(() =>
                {
                    FileUtility.Export(path, Project, ExportFormat, (from, to) => System.IO.File.Copy(from, to, true),
                                      Splitter, ShowProgressMessage);
                });

                GetProgress().Close();
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
                GetProgress().Show();
                await Task.Run(() => ExportProject(path, Project));
                GetProgress().Close();
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
            GetProgress().Show();
            await Task.Run(() =>
            {
                DeleteFilesOfProject(Project);
            });
            GetProgress().Close();
            await new MessageDialog().ShowAsync("删除成功", "删除文件");

        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            //无用
            GetProgress().Show();
            await Task.Run(() => Check(Project));
            GetProgress().Close();
        }

        private async void DeleteThumbnails_Click(object sender, RoutedEventArgs e)
        {
            GetProgress().Show();
            await Task.Run(() =>
            {
                try
                {
                    FileUtility.DeleteThumbnails(Project);
                }
                catch(Exception ex)
                {

                }
            });
            FileIcon.ClearCaches();
            GetProgress().Close();
            await new MessageDialog().ShowAsync("删除成功，部分物理文件可能无法删除，请从设置中进行修复", "删除缩略图");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            flyoutDeleteFileClasses.Hide();
            GetProgress().Show(); await Task.Run(() =>
            {
                DeleteAllFileClasses(Project);
            });
            GetProgress().Close();
            await new MessageDialog().ShowAsync("删除成功", "删除文件分类关系");
        }
    }
}


