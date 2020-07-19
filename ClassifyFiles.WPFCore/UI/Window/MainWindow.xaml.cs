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
                if (selectedProject != null)
                {
                    selectedProject.PropertyChanged -= Project_PropertyChanged;
                }
                selectedProject = value;
                this.Notify(nameof(SelectedProject));
                if (value != null)
                {
                    value.PropertyChanged += Project_PropertyChanged;
                    if (value.ID != Configs.LastProjectID)
                    {
                        Configs.LastProjectID = value.ID;
                    }
                }
                LoadProjectAsync();
            }
        }

        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public MainWindow()
        {
            InitializeComponent();
            var width = SystemParameters.WorkArea.Width;
            var height = SystemParameters.WorkArea.Height;
            Width = width * 0.8;
            Height = height * 0.8;
            //Left = width * .1;
            //Top = height * .1;

        }


        public async Task DeleteSelectedProjectAsync()
        {
            await DeleteProjectAsync(SelectedProject);

            Projects.Remove(SelectedProject);
            Projects = new ObservableCollection<Project>(await GetProjectsAsync());
            if (Projects.Count == 0)
            {
                Projects.Add(await AddProjectAsync());
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
                    MainPanel = Activator.CreateInstance(MainPanel.GetType()) as ILoadable;
                    await MainPanel.LoadAsync(SelectedProject);
                }
                catch (Exception ex)
                { }
                Progress.Close();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Projects = new ObservableCollection<Project>(await GetProjectsAsync());
            if (Projects.Count == 0)
            {
                Projects.Add(await AddProjectAsync());
            }
            if (Projects.Any(p => p.ID == Configs.LastProjectID))
            {
                SelectedProject = Projects.First(p => p.ID == Configs.LastProjectID);
            }
            else
            {
                SelectedProject = Projects[0];
            }
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


        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow win = new SettingWindow(Projects) { Owner = this };
            win.ShowDialog();
        }



        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            ToggleButton btn = sender as ToggleButton;
            var a = cmdBarPanel.Content;
            foreach (var t in cmdBarPanel.PrimaryCommands.OfType<ToggleButton>())
            {
                (t as ToggleButton).IsChecked = btn == t;
            }
            if (MainPanel is ClassSettingPanel)
            {
                await (MainPanel as ClassSettingPanel).SaveClassAsync();
            }
            else if (MainPanel is ProjectSettingsPanel)
            {
                await SaveChangesAsync();
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

        private async void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Project project = await AddProjectAsync();
            Projects.Add(project);
            SelectedProject = project;
        }
    }
}
