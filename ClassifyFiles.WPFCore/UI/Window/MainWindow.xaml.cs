using ClassifyFiles.Data;
using ClassifyFiles.UI.Component;
using ClassifyFiles.UI.Page;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ClassifyFiles.Util.DbUtility;
using static ClassifyFiles.Util.ProjectUtility;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowBase, IWithProcessRing
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
            Application.Current.MainWindow = this;

            InitializeComponent();

            Projects = new ObservableCollection<Project>(GetProjects());
            if (Projects.Count == 0)
            {
                Projects.Add(AddProject());
            }
            if (Projects.Any(p => p.ID == Configs.LastProjectID))
            {
                SelectedProject = Projects.First(p => p.ID == Configs.LastProjectID);
            }
            else
            {
                SelectedProject = Projects[0];
            }
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

            SetNavViewPaneDisplayMode();
            Configs.StaticPropertyChanged += (p1, p2) =>
            {
                switch (p2.PropertyName)
                {
                    case nameof(Configs.PaneDisplayLeftMinimal):
                        SetNavViewPaneDisplayMode();
                        break;
                }
            };
            SetNavViewPaneBackground();
            ThemeManager.AddActualThemeChangedHandler(this, (p1, p2) => SetNavViewPaneBackground());
        }

        private void SetNavViewPaneBackground()
        {
            Color color = default;
            if (ThemeManager.GetActualTheme(this) == ElementTheme.Light)
            {
                color = Color.FromArgb(0xFF, 0xD8, 0xD8, 0xD8);
            }
            else
            {
                color = Color.FromArgb(0xFF, 0x28, 0x28, 0x28);
            }
            Resources["NavigationViewDefaultPaneBackground"] = new SolidColorBrush(color);
        }

        public void SetNavViewPaneDisplayMode()
        {
            if (Configs.PaneDisplayLeftMinimal)
            {
                navView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
            }
            else
            {
                navView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
            }
        }

        public static MainWindow Current => App.Current.MainWindow as MainWindow;

        public Task DeleteSelectedProjectAsync()
        {
            return DoProcessAsync(Do());
            async Task Do()
            {
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
            }
        }

        private FileBrowserPanel fileBrowserPanel = new FileBrowserPanel();
        private ClassSettingPanel classSettingPanel = new ClassSettingPanel();
        private ProjectSettingsPanel projectSettingsPanel = new ProjectSettingsPanel();

        private System.Windows.Controls.Page emptyPage = new System.Windows.Controls.Page();

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
                await DoProcessAsync(mainPage.LoadAsync(SelectedProject));
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Configs.HasOpened)
            {
                Configs.HasOpened = true;
                ContentRendered += async (p1, p2) =>
                 {
                     if (await new ConfirmDialog().ShowAsync("第一次使用，需要生成示例数据吗？", "首次使用"))
                     {
                         Project project = null;
                         await DoProcessAsync(Task.Run(() =>
                         {
                             string db = "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";
                             string zipTemp = Path.GetTempFileName() + ".zip";
                             System.IO.File.WriteAllBytes(zipTemp, Convert.FromBase64String(db));
                             string dbTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                             ZipFile.ExtractToDirectory(zipTemp, dbTemp);
                             project = Import(Path.Combine(dbTemp, "电影.db"))[0];
                         }));
                         Projects.Add(project);
                         SelectedProject = project;
                     }
                     else
                     {
                         await LoadPanelAsync();
                     }
                 };
            }
            else
            {
                await LoadPanelAsync();
            }
        }

        private ILoadable mainPage;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ring.Showing)
            {
                e.Cancel = true;
                return;
            }
            if (canClose)
            {
                return;
            }
            //退出前还要做一些工作，暂时先隐藏窗体，暂缓退出
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
            BeforeClosing(true);
        }

        private bool canClose = false;

        public async Task BeforeClosing(bool shutDownApp)
        {
            canClose = true;
            if (mainPage is ClassSettingPanel p)
            {
                await p.SaveClassesAsync();
            }
            else if (mainPage is ProjectSettingsPanel)
            {
                await SaveChangesAsync();
            }
            await FileIcon.Tasks.Stop();
            if (shutDownApp)
            {
                Application.Current.Shutdown();
            }
        }

        private async Task<int> SaveChangesAsync()
        {
            int result = 0;
            await Task.Run(() => result = SaveChanges());
            return result;
        }

        private async void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            navView.IsPaneOpen = false;
            await DoProcessAsync(Do());
            async Task Do()
            {
                Project project = null;
                await Task.Run(() => project = AddProject());
                Projects.Add(project);
                SelectedProject = project;
            }
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

        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            await DoProcessAsync(Do());
            async Task Do()
            {
                ILoadable page = (sender as ListBox).SelectedIndex switch
                {
                    0 => fileBrowserPanel,
                    1 => classSettingPanel,
                    2 => projectSettingsPanel,
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
            }
        }

        private bool ignoreNavViewSelectionChanged = false;

        private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (!IsLoaded || ignoreNavViewSelectionChanged)
            {
                return;
            }
            if (sender.MenuItems.IndexOf(args.SelectedItem) == -1)
            {      //确保只有一个SettingWindow
                if (SettingWindow.Current == null)
                {
                    SettingWindow win = new SettingWindow(Projects) { Owner = this };
                    win.Show();
                    await Task.Delay(500);
                    ignoreNavViewSelectionChanged = true;
                    if (mainPage is FileBrowserPanel)
                    {
                        navView.SelectedItem = navView.MenuItems[0];
                    }
                    else if (mainPage is ClassSettingPanel)
                    {
                        navView.SelectedItem = navView.MenuItems[1];
                    }
                    else if (mainPage is ProjectSettingsPanel)
                    {
                        navView.SelectedItem = navView.MenuItems[2];
                    }

                    ignoreNavViewSelectionChanged = false;
                    //win.BringToFront();
                }
                else
                {
                    SettingWindow.Current.BringToFront();
                }
            }
            await DoProcessAsync(Do());
            async Task Do()
            {
                ILoadable page = sender.MenuItems.IndexOf(args.SelectedItem) switch
                {
                    0 => fileBrowserPanel,
                    1 => classSettingPanel,
                    2 => projectSettingsPanel,
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
            }
        }

        private void ProjectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isOpen = (grdProject.FindResource("flyoutProject") as Flyout).IsOpen;
            if (isOpen)
            {
                (grdProject.FindResource("flyoutProject") as Flyout).Hide();
            }
            navView.IsPaneOpen = false;
        }

        private async void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Project project = (sender as Button).Tag as Project;
            if (await new ConfirmDialog().ShowAsync("是否删除项目：" + project.Name + "？", "删除项目"))
            {
                await DoProcessAsync(Task.Run(() => ProjectUtility.DeleteProject(project)));
                Project newProject = Projects.FirstOrDefault(p => p != project);
                SelectedProject = newProject;
                Projects.Remove(project);
            }
        }
    }
}