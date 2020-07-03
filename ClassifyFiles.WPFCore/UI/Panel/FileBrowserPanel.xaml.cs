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

namespace ClassifyFiles.UI.Panel
{

    public partial class FileBrowserPanel : ProjectPanelBase
    {

        public FileBrowserPanel()
        {
            InitializeComponent();
        }

        public override ListPanelBase GetItemsPanel()
        {
            return leftPanel;
        }
        ListPanelBase leftPanel;

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override async Task LoadAsync(Project project)
        {
            switch (project.Type)
            {
                case Project.ClassifyType.FileProps:
                    classPanel.Visibility = Visibility.Visible;
                    btnRefreshAll.Visibility = Visibility.Visible;
                    btnRefreshCurrent.Visibility = Visibility.Visible;
                    leftPanel = classPanel;
                    break;
                case Project.ClassifyType.Tag:
                    tagPanel.Visibility = Visibility.Visible;
                    btnAddFile.Visibility = Visibility.Visible;
                    leftPanel = tagPanel;
                    break;
            }
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
            await DbUtility.UpdateProjectAsync(Project);
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


        private async void classes_SelectedItemChanged_1(object sender, SelectedItemChanged e)
        {
            var files = await DbUtility.GetFilesAsync(GetItemsPanel().SelectedItem);
            filesViewer.SetFiles(files);

            filesViewer.GeneratePaggingButtons();
        }


        private void JumpToDirComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = e.AddedItems.Count == 0 ? null : e.AddedItems.Cast<string>().First();
            if (dir != null)
            {
                FileWithIcon file;
                filesViewer.SelectFileByDir(dir);
                (sender as ListBox).SelectedItem = null;
                flyoutJumpToDir.Hide();// = false;
            }
        }

        private void filesViewer_ViewTypeChanged(object sender, EventArgs e)
        {
            btnLocateByDir.IsEnabled = filesViewer.CurrentViewType != 3;
        }

        private async void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            GetProgress().Show(true);
            try
            {
                var classFiles = await FileUtility.GetFilesOfClassesAsync(new System.IO.DirectoryInfo(Project.RootPath), GetItemsPanel().Items.Cast<Class>(), true, (p, f) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        GetProgress().Message = p.ToString("P") + (f == null ? "" : $"（{f.Name}）");
                    });
                });
                await DbUtility.UpdateFilesAsync(classFiles);
                if (classPanel.SelectedItem != null && classFiles.ContainsKey(classPanel.SelectedItem as Class))
                {
                    filesViewer.SetFiles(classFiles[classPanel.SelectedItem as Class]);
                }
                filesViewer.GeneratePaggingButtons();
            }
            catch (Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "刷新错误");
            }
            finally
            {

                GetProgress().Close();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetProgress().Show(true);
            try
            {
                var files = await FileUtility.GetFilesOfClassesAsync(new System.IO.DirectoryInfo(Project.RootPath), GetItemsPanel().SelectedItem as Class, true, (p, f) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        GetProgress().Message = p.ToString("P") + (f == null ? "" : $"（{f.Name}）");
                    });
                });
                await DbUtility.UpdateFilesAsync(GetItemsPanel().SelectedItem, files);

                filesViewer.SetFiles(files);
                filesViewer.GeneratePaggingButtons();
            }
            catch (Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "刷新错误");
            }
            finally
            {
                GetProgress().Close();
            }

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
                GetProgress().Show(true);
                Tag tag = tagPanel.SelectedItem as Tag;
                var files = await FileUtility.AddFilesToTag(dialog.FileNames, tag, new System.IO.DirectoryInfo(base.Project.RootPath), (p, f) =>
                  {
                      Dispatcher.Invoke(() =>
                      {
                          GetProgress().Message = p.ToString("P") + (f == null ? "" : $"（{f.Name}）");
                      });
                  });
                await DbUtility.SaveTagAsync(tag);
                filesViewer.AddFiles(files);
                GetProgress().Close();
            }
        }

        private void filesViewer_ClickTag(object sender, ClickTagEventArgs e)
        {
            if(e.Tag!= tagPanel.SelectedItem)
            {
                tagPanel.SelectedItem= e.Tag;
            }
        }
    }


}
