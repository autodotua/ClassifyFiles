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
using System.Windows.Shapes;
using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using ClassifyFiles.UI.Panel;
using System.Diagnostics;
using DImg = System.Drawing.Image;
using ModernWpf.Controls;
using ClassifyFiles.UI.Model;
using Microsoft.WindowsAPICodePack.Dialogs;
using IO = System.IO;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;

namespace ClassifyFiles.UI.Panel
{

    public partial class FileBrowserPanel : ProjectPanelBase
    {

        public FileBrowserPanel()
        {
            InitializeComponent();
        }

        public override ClassesPanel GetItemsPanel()
        {
            return classPanel;
        }

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override async Task LoadAsync(Project project)
        {
            filesViewer.Project = project;
            await base.LoadAsync(project);

        }
        private ProgressDialog GetProgress()
        {
            return (Window.GetWindow(this) as MainWindow).Progress;
        }


        private async void RenameProjectButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = await new InputDialog().ShowAsync("请输入新的项目名", false, "项目名", Project.Name);
            Project.Name = newName;
            await UpdateProjectAsync(Project);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWin = (Window.GetWindow(this) as MainWindow);
            await mainWin.DeleteSelectedProjectAsync();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsOpen = true;
        }


        private async void SelectedClassChanged(object sender, SelectedClassChangedEventArgs e)
        {
            Debug.WriteLine("Selected Class Changed, Project Hashcode is " + Project.GetHashCode()
            + ", Class is " + (GetItemsPanel().SelectedItem == null ? "null" : GetItemsPanel().SelectedItem.Name));
            List<File> files = GetItemsPanel().SelectedItem == null ?
                await GetFilesByProjectAsync(Project.ID)
                : await GetFilesByClassAsync(GetItemsPanel().SelectedItem.ID);
            await filesViewer.SetFilesAsync(files);
            Dirs = filesViewer.Files == null ? null : new HashSet<string>(filesViewer.Files.Select(p => p.Dir));
        }
        public HashSet<string> dirs;
        public HashSet<string> Dirs
        {
            get => dirs;
            set
            {
                dirs = value;
                this.Notify(nameof(Dirs));
            }
        }


        private void JumpToDirComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = e.AddedItems.Count == 0 ? null : e.AddedItems.Cast<string>().First();
            if (dir != null)
            {
                filesViewer.SelectFileByDir(dir);
                (sender as ListBox).SelectedItem = null;
                flyoutJumpToDir.Hide();// = false;
            }
        }

        private void filesViewer_ViewTypeChanged(object sender, EventArgs e)
        {
            btnLocateByDir.IsEnabled = filesViewer.CurrentViewType != 3;
        }

        private async void ClassifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Project.RootPath))
            {
                await new ErrorDialog().ShowAsync("请先设置根目录地址！", "错误");
                return;
            }
            GetProgress().Show(true);
            if (new UpdateFilesWindow(Project) { Owner = Window.GetWindow(this) }.ShowDialog() == true)
            {
                if (classPanel.SelectedItem != null)
                {
                    await filesViewer.SetFilesAsync(await GetFilesByClassAsync(classPanel.SelectedItem.ID));
                }
            }
            GetProgress().Close();
        }

        private async void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = true,
                DefaultDirectory = Project.RootPath
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                await AddFilesAsync(dialog.FileNames.ToList());
            }
        }

        private void filesViewer_ClickTag(object sender, ClickTagEventArgs e)
        {
            if (e.Class != GetItemsPanel().SelectedItem)
            {
                GetItemsPanel().SelectedItem = e.Class;
            }
        }

        private async void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (classPanel.SelectedItem == null)
            {
                return;
            }
            if(e.Data.GetDataPresent(nameof(ClassifyFiles)))
            {
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await AddFilesAsync(files);
            }
        }

        private async Task AddFilesAsync(IList<string> files)
        {
            var dialog = new AddFilesWindow(classPanel.SelectedItem, files)
            { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            if (dialog.AddedFiles != null)
            {
                await filesViewer.AddFilesAsync(dialog.AddedFiles);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedClassChanged(null, null);
        }

        private void btnAllFiles_Click(object sender, RoutedEventArgs e)
        {
            if (GetItemsPanel().SelectedItem == null)
            {
                SelectedClassChanged(null, null);
            }
            else
            {
                GetItemsPanel().SelectedItem = null;
            }
        }
    }
}
