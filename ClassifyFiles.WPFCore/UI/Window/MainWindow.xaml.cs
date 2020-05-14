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
                LoadProject();
            }
        }
        public MainWindow()
        {
            InitializeComponent();

        }




        private async Task LoadProject()
        {
            MenuToggleButton.IsChecked = false;
            if (MainPanel != null)
            {
                await MainPanel.LoadAsync(SelectedProject);
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

        public ProjectPanelBase mainPanel = new FileBrowserPanel();
        public ProjectPanelBase MainPanel
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
            RadioButton rbtn = sender as RadioButton;
            switch (rbtn.Tag)
            {
                case "1":
                    if (MainPanel is ClassSettingPanel)
                    {
                        await (MainPanel as ClassSettingPanel).SaveClassAsync();
                    }
                    MainPanel = new FileBrowserPanel();
                    break;
                case "2":
                    MainPanel = new ClassSettingPanel();
                    break;
                case null:
                    return;
            }
            await MainPanel.LoadAsync(SelectedProject);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow() { Owner = this }.ShowDialog();
        }

        private async void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Project project = await DbUtility.AddProjectAsync();
            Projects.Add(project);
            SelectedProject = project;
        }
    }
}
