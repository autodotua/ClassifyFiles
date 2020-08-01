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
using System.ComponentModel;
using static ClassifyFiles.Data.Project;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;
using static ClassifyFiles.Util.DbUtility;
using System.Windows.Media.Animation;
using System.Windows.Media;
using ClassifyFiles.UI.Page;
using System.Windows.Media.Imaging;
using System.IO;
using ClassifyFiles.UI.Component;
using System.Threading;

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
                if (value != null)
                {
                    if (value.ID != Configs.LastProjectID)
                    {
                        Task.Run(() => Configs.LastProjectID = value.ID);
                    }
                }

                LoadPanelAsync();
            }
        }

        public MainWindow()
        {
            Projects = new ObservableCollection<Project>(GetProjects());
            if (Projects.Count == 0)
            {
                Projects.Add(AddProject());
            }
            if (Projects.Any(p => p.ID == Configs.LastProjectID))
            {
                selectedProject = Projects.First(p => p.ID == Configs.LastProjectID);
            }
            else
            {
                selectedProject = Projects[0];
            }
            InitializeComponent();
            var width = SystemParameters.WorkArea.Width;
            var height = SystemParameters.WorkArea.Height;
            Width = width * 0.8;
            Height = height * 0.8;

            DbSavingException += async (p1, p2) =>
            {
                await Dispatcher.Invoke(async () =>
                {
                    await new ErrorDialog().ShowAsync(p2.ExceptionObject as Exception, "发生数据库保存错误");
                });
            };

            mainPage = fileBrowserPanel;
            frame.Content = mainPage;
        }

        public async Task DeleteSelectedProjectAsync()
        {
            Progress.Show();
            List<Project> projects = null;
            await Task.Run(() =>
            {
                DeleteProject(SelectedProject);
                projects = GetProjects();
                if (projects.Count == 0)
                {
                    var newProject = AddProject();
                    projects.Add(newProject);
                }
            });
            await Task.Delay(1000);
            Projects = new ObservableCollection<Project>(projects);
            SelectedProject = Projects[0];
            //RadioButton_Checked(btnModeView, null);

            Progress.Close();
        }

        FileBrowserPanel fileBrowserPanel = new FileBrowserPanel();
        ClassSettingPanel classSettingPanel = new ClassSettingPanel();
        ProjectSettingsPanel projectSettingsPanel = new ProjectSettingsPanel();

        System.Windows.Controls.Page emptyPage = new System.Windows.Controls.Page();

        private async Task NavigateToAsync(object view)
        {
            if (frame.Content == view)
            {
                //两次页面不同，才能够有动画
                emptyPage.Content = new Image();
                (emptyPage.Content as Image).Source = ImageUtility.CreateScreenshotOfFrameworkElement(mainPage as FrameworkElement);
                //首先设置Content，这个是没有动画的
                //但是如果直接来，那么画面会先黑一下，效果不好
                //所以先给页面截一张图，放到空白的Page上，然后再进行设置和动画
                frame.Content = emptyPage;
                await Task.Delay(100);
            }
            frame.Navigate(view);
        }
        private async Task LoadPanelAsync()
        {
            if (mainPage != null)
            {
                await NavigateToAsync(mainPage);
                Progress.Show();
                try
                {
                    await mainPage.LoadAsync(SelectedProject);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                Progress.Close();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPanelAsync();
        }

        private ILoadable mainPage;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //退出前还要做一些工作，暂时先隐藏窗体，暂缓退出
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
            BeforeClosing();
        }

        public async Task BeforeClosing()
        {
            if (mainPage is ClassSettingPanel p)
            {
                await p.SaveClassesAsync();
            }
            else if (mainPage is ProjectSettingsPanel)
            {
                await SaveChangesAsync();
            }
            await FileIcon.Tasks.Stop();
            Application.Current.Shutdown();
        }

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //确保只有一个SettingWindow
            if (SettingWindow.Current == null)
            {
                SettingWindow win = new SettingWindow(Projects);
                win.Show();
            }
            else
            {
                SettingWindow.Current.BringToFront();
            }
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            Progress.Show();
            ToggleButton btn = sender as ToggleButton;
            foreach (var t in cmdBarPanel.PrimaryCommands.OfType<ToggleButton>())
            {
                t.IsChecked = btn == t;
            }
            ILoadable page = btn.Name switch
            {
                nameof(btnModeView) => fileBrowserPanel,
                nameof(btnModeClasses) => classSettingPanel,
                nameof(btnModeProjectSettings) => projectSettingsPanel,
                _ => throw new Exception()
            };
            if (mainPage is ClassSettingPanel p)
            {
                await p.SaveClassesAsync();
            }
            else if (mainPage is ProjectSettingsPanel)
            {
                await SaveChangesAsync();
            }

            mainPage = page;
            await LoadPanelAsync();

            Progress.Close();
        }

        private async Task<int> SaveChangesAsync()
        {
            int result = 0;
            await Task.Run(() => result = SaveChanges());
            return result;
        }

        private async void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Progress.Show();
            Project project = null;
            await Task.Run(() => project = AddProject());
            Projects.Add(project);
            SelectedProject = project;
            Progress.Close();
        }
    }
}
