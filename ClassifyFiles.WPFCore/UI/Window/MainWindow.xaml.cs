using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ClassifyFiles.Data;
using System;
using System.Windows.Input;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ClassifyFiles.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FzLib.Extension;
using System.Threading.Tasks;
using ClassifyFiles.UI.Panel;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls;
using ModernWpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using FzLib.Basic;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        private ObservableCollection<Project> projects;
        public ObservableCollection<Project> Projects
        {
            get => projects;
            set
            {
                projects = value;
                this.Notify(nameof(Projects));
            }
        }
        private Project selectedProject;
        public Project SelectedProject
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                this.Notify(nameof(SelectedProject));
                LoadProjectAsync();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }


        public async Task DeleteSelectedProjectAsync()
        {
            await DbUtility.DeleteProjectAsync(SelectedProject);

            Projects.Remove(SelectedProject);
            Projects = new ObservableCollection<Project>(await DbUtility.GetProjectsAsync());
            if (Projects.Count == 0)
            {
                Projects.Add(await DbUtility.AddProjectAsync());
            }
            SelectedProject = Projects[0];
            RadioButton_Checked(btnModeView, null);
        }

        private async Task LoadProjectAsync()
        {
            if (MainPanel != null)
            {
                Progress.Show(false);
                try
                {
                    await MainPanel.LoadAsync(SelectedProject);
                }
                catch (Exception ex)
                { }
                Progress.Close();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Projects = new ObservableCollection<Project>(await DbUtility.GetProjectsAsync());
            if (Projects.Count == 0)
            {
                Projects.Add(await DbUtility.AddProjectAsync());
            }
            SelectedProject = Projects[0];

        }

        public ILoadable mainPanel = new FileBrowserPanel();
        public ILoadable MainPanel
        {
            get => mainPanel;
            set
            {
                mainPanel = value;
                this.Notify(nameof(MainPanel));
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow win = new SettingWindow() { Owner = this };
            win.ShowDialog();
        }


        private void MenuToggleButton_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            ToggleButton btn = sender as ToggleButton;
            var a = cmdBarPanel.Content;
            foreach (var t in cmdBarPanel.PrimaryCommands)
            {
                (t as ToggleButton).IsChecked = btn == t;
            }
            if (MainPanel is ClassSettingPanel)
            {
                await (MainPanel as ClassSettingPanel).SaveClassAsync();
            }
            switch (btn.Name)
            {
                case nameof(btnModeView):
                    MainPanel = new FileBrowserPanel();
                    break;
                case nameof(btnModeClasses):
                    MainPanel = new ClassSettingPanel();
                    break;
                case nameof(btnModeProjectSettings):
                    MainPanel = new ProjectSettingsPanel();
                    break;
                case null:
                    return;
            }

            await LoadProjectAsync();
        }

        private void SettingMenu_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow() { Owner = this }.ShowDialog();
        }

        private async void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Project project = await DbUtility.AddProjectAsync();
            Projects.Add(project);
            SelectedProject = project;
        }


        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ImportMenu_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Title = "请选择导入的文件",
            };
            dialog.Filters.Add(new CommonFileDialogFilter("SQLite数据库", "db"));
            dialog.Filters.Add(new CommonFileDialogFilter("所有文件", "*"));
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                Progress.Show(true);
                var projects = await DbUtility.Import(path);
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
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                Progress.Show(true);
                await DbUtility.ExportAll(path);
                Progress.Close();
                await new MessageDialog().ShowAsync("导出成功", "导出");
            }

        }

        private async void DeleteAllMenu_Click(object sender, RoutedEventArgs e)
        {
            if(await new ConfirmDialog().ShowAsync("真的要删除所有项目吗？", "删除"))
            {
                foreach (var project in Projects.ToArray())
                {
                   await DbUtility.DeleteProjectAsync(project);
                    Projects.Remove(project);
                }
                await new MessageDialog().ShowAsync("删除成功", "删除");
            }
        }
    }
}
