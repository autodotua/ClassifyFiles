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
            if (GetItemsPanel().SelectedItem == null)
            {
                await filesViewer.SetFilesAsync(null);
            }
            else
            {
                GetProgress().Show(false);
                var files = await DbUtility.GetFilesByClassAsync(GetItemsPanel().SelectedItem.ID);
                await filesViewer.SetFilesAsync(files);
                GetProgress().Close();
            }
        }


        private void JumpToDirComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = e.AddedItems.Count == 0 ? null : e.AddedItems.Cast<string>().First();
            if (dir != null)
            {
                UIFile file;
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
            if (string.IsNullOrEmpty(Project.RootPath))
            {
                await new ErrorDialog().ShowAsync("请先设置根目录地址！", "错误");
                return;
            }
            GetProgress().Show(true);
            try
            {
                await DbUtility.UpdateFilesOfClassesAsync(new UpdateFilesArgs()
                {
                    Classes = GetItemsPanel().Items,
                    IncludeThumbnails = true,
                    Project = Project,
                    RefreshClasses = true,
                    Callback = (p, f) => Dispatcher.Invoke(() =>
                      {
                          GetProgress().Message = p.ToString("P") + (f == null ? "" : $"（{f.Name}）");
                      })
                });
                if (classPanel.SelectedItem != null)
                {
                    await filesViewer.SetFilesAsync(await DbUtility.GetFilesByClassAsync(classPanel.SelectedItem.ID));
                }
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
                Class tag = GetItemsPanel().SelectedItem as Class;
                var files = await DbUtility.AddFilesToClass(dialog.FileNames, tag, true);
                filesViewer.AddFiles(files);
                GetProgress().Close();
            }
        }

        private void filesViewer_ClickTag(object sender, ClickTagEventArgs e)
        {
            if (e.Class != GetItemsPanel().SelectedItem)
            {
                GetItemsPanel().SelectedItem = e.Class;
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            }
        }
    }
}



