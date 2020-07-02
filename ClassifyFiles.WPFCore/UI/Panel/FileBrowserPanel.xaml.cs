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

namespace ClassifyFiles.UI.Panel
{
    public abstract class FilePropsBrowserPanelBase : ProjectPanelBase<Class>
    {

    }
    public partial class FilePropsBrowserPanel : FilePropsBrowserPanelBase
    {

        public FilePropsBrowserPanel()
        {
            InitializeComponent();
        }

        public override ListPanelBase<Class> GetItemsPanel()
        {
            return classes;
        }

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
            filesViewer.Project = project;
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


        private async void classes_SelectedItemChanged_1(object sender, SelectedItemChanged<Class> e)
        {
            var files = await DbUtility.GetFilesAsync(SelectedItem);
            filesViewer.SetFiles(files);

            filesViewer.GeneratePaggingButtons();
        }

        private async void FilesViewer_RefreshButtonClick(object sender, RefreshButtonClickEventArgs e)
        {
            if (e.RefreshAll)
            {
                GetProgress().Show(true);
                try
                {
                    var classFiles = await FileUtility.GetFilesAsync(new System.IO.DirectoryInfo(Project.RootPath), GetItemsPanel().Items, true, (p, f) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            GetProgress().Message = p.ToString("P") + (f == null ? "" : $"（{f.Name}）");
                        });
                    });
                    await DbUtility.UpdateFilesAsync(classFiles);
                    if (classes.SelectedItem != null && classFiles.ContainsKey(classes.SelectedItem))
                    {
                        filesViewer.SetFiles(classFiles[classes.SelectedItem]);
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
            else
            {
                GetProgress().Show(true);
                try
                {
                    var files = await FileUtility.GetFilesAsync(new System.IO.DirectoryInfo(Project.RootPath), GetItemsPanel().SelectedItem, true, (p, f) =>
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
    }


}
