﻿using FzLib.Basic;
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
using System.Windows.Media.Animation;
using ClassifyFiles.Enum;
using ClassifyFiles.UI.Util;
using ClassifyFiles.UI.Event;
using ClassifyFiles.UI.Dialog;

namespace ClassifyFiles.UI.Panel
{

    public partial class FileBrowserPanel : ProjectPanelBase
    {

        public FileBrowserPanel()
        {
            InitializeComponent();
            new ClickEventHelper(grdSplitter).Click += GridSplitter_Click;
            foreach (MenuItem item in menuSort.Items)
            {
                if ((int)item.Tag == Configs.SortType)
                {
                    item.IsChecked = true;
                    break;
                }
            }
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

        private bool ignoreClassChanged = false;
        private async void SelectedClassChanged(object sender, SelectedClassChangedEventArgs e)
        {
            if (ignoreClassChanged)
            {
                return;
            }
            if (GetItemsPanel().SelectedItem == null)
            {
                //await SetFilesAsync(() => GetFilesByProjectAsync(Project.ID));
            }
            else
            {
                await SetFilesAsync(() => GetFilesByClassAsync(GetItemsPanel().SelectedItem.ID));
            }
        }

        /// <summary>
        /// 设置FileViewer的文件
        /// </summary>
        /// <param name="func">用于获取文件的异步函数</param>
        /// <returns></returns>
        private async Task SetFilesAsync(Func<Task<List<File>>> func)
        {
            GetProgress().Show(true);
            Debug.WriteLine("Set Files, Project Hashcode is " + Project.GetHashCode()
            + ", Class is " + (GetItemsPanel().SelectedItem == null ? "null" : GetItemsPanel().SelectedItem.Name));
            List<File> files = await func();
            await filesViewer.SetFilesAsync(files);
            if (filesViewer.Files == null)
            {
                Dirs = null;
            }
            else
            {
                HashSet<string> dirs = null;
                await Task.Run(() =>
                {
                    dirs = new HashSet<string>(filesViewer.Files.Select(p => p.File.Dir));
                });
                Dirs = dirs;
            }
            GetProgress().Close();
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


        private async void JumpToDirComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = e.AddedItems.Count == 0 ? null : e.AddedItems.Cast<string>().First();
            if (dir != null)
            {
                filesViewer.SelectFileByDir(dir);
                await Task.Delay(250);
                flyoutJumpToDir.Hide();// = false;
                (sender as ListBox).SelectedItem = null;
            }
        }

        private void filesViewer_ViewTypeChanged(object sender, EventArgs e)
        {
            btnLocateByDir.IsEnabled = filesViewer.CurrentFileView != FileView.Tree;
            btnSort.IsEnabled = filesViewer.CurrentFileView != FileView.Tree;
        }

        private async void ClassifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Project.RootPath))
            {
                await new ErrorDialog().ShowAsync("请先设置根目录地址！", "错误");
                return;
            }
            GetProgress().Show(true);
            if (new UpdateFilesDialog(Project) { Owner = Window.GetWindow(this) }.ShowDialog() == true)
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
            if (e.Data.GetDataPresent(nameof(ClassifyFiles)))
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
            var dialog = new AddFilesDialog(classPanel.SelectedItem, files)
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

        private async void btnAllFiles_Click(object sender, RoutedEventArgs e)
        {
            await SetFilesAsync(() => GetFilesByProjectAsync(Project.ID));
        }
        public bool ShowClassTags
        {
            get => Configs.ShowClassTags;
            set
            {
                Configs.ShowClassTags = value;
                filesViewer.Refresh();
            }
        }
        public bool ShowThumbnail
        {
            get => Configs.ShowThumbnail;
            set
            {
                Configs.ShowThumbnail = value;
                filesViewer.Refresh();
            }
        }
        public bool ShowExplorerIcon
        {
            get => Configs.ShowExplorerIcon;
            set
            {
                Configs.ShowExplorerIcon = value;
                filesViewer.Refresh();
            }
        }
        public bool ShowIconViewNames
        {
            get => Configs.ShowIconViewNames;
            set
            {
                Configs.ShowIconViewNames = value;
                filesViewer.Refresh();
            }
        }
        public bool ShowTilePath
        {
            get => Configs.ShowTilePath;
            set
            {
                Configs.ShowTilePath = value;
                filesViewer.Refresh();
            }
        }
        public bool ShowFileExtension
        {
            get => Configs.ShowFileExtension;
            set
            {
                Configs.ShowFileExtension = value;
                filesViewer.Refresh();
            }
        }
        public bool ShowToolTip
        {
            get => Configs.ShowToolTip;
            set => Configs.ShowToolTip = value;
        }
        public bool ShowToolTipImage
        {
            get => Configs.ShowToolTipImage;
            set => Configs.ShowToolTipImage = value;
        }
        bool isSettingSortChecked = false;
        public bool GroupByDir
        {
            get => Configs.GroupByDir;
            set
            {
                Configs.GroupByDir = value;
                if (value)
                {
                    Configs.SortType = 0;
                    isSettingSortChecked = true;
                    menuSortDefault.IsChecked = true;
                    isSettingSortChecked = false;
                }
                this.Notify(nameof(GroupByDir));
                filesViewer.SetGroupEnable(value);
            }
        }

        public double IconSize
        {
            get => Configs.IconSize;
            set
            {
                tbkIconSize.Text = ((int)(value / 32 * 100)).ToString() + "%";
                if (!IsLoaded)
                {
                    return;
                }
                Configs.IconSize = value;
                this.Notify(nameof(IconSize), nameof(IconSizeString));
            }
        }
        public string IconSizeString => ((int)(IconSize / 32 * 100)).ToString() + "%";


        private void sldIconSize_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            sldIconSize.Value = 32;
        }

        private void filesViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                this.Notify(nameof(IconSize), nameof(IconSizeString));
            }
        }

        private async void RadioMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || isSettingSortChecked)
            {
                return;
            }
            GetProgress().Show(true);
            SortType type = (SortType)(sender as FrameworkElement).Tag;
            await filesViewer.SortAsync(type);
            Configs.SortType = (int)type;
            Configs.GroupByDir = false;

            this.Notify(nameof(GroupByDir));
            //GroupByDir = false;
            GetProgress().Close();
        }

        bool isAnimating = false;
        private void GridSplitter_Click(object sender, EventArgs e)
        {
            if (isAnimating)
            {
                return;
            }
            isAnimating = true;
            (sender as FrameworkElement).IsEnabled = false;

            double to = grdLeft.Width == 0 ? 200 : 0;
            StartAnimation(to);
        }

        private void StartAnimation(double to)
        {
            DoubleAnimation ani = new DoubleAnimation(to, TimeSpan.FromSeconds(0.5))
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop
            };
            ani.Completed += (p1, p2) =>
            {
                isAnimating = false;
                grdSplitter.IsEnabled = true;
                grdLeft.Width = to;
                grdMain.ColumnDefinitions[0].Width = GridLength.Auto;
            };
            grdLeft.BeginAnimation(WidthProperty, ani);
        }

        private void grdSplitter_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cd = grdMain.ColumnDefinitions[0];
            if (cd.Width.IsAbsolute)
            {
                StartAnimation(cd.Width.Value - 8);
            }
        }

        private async void btnNoClassesFiles_Click(object sender, RoutedEventArgs e)
        {
            await SetFilesAsync(() => GetNoClassesFilesByProjectAsync(Project.ID));

        }
    }
}
