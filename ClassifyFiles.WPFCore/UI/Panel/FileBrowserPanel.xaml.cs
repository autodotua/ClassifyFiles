using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassifyFiles.Data;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;
using System.Windows.Media.Animation;
using ClassifyFiles.Enum;
using ClassifyFiles.UI.Event;
using ClassifyFiles.UI.Dialog;

namespace ClassifyFiles.UI.Panel
{

    public partial class FileBrowserPanel : ProjectPanelBase
    {

        public FileBrowserPanel()
        {
            InitializeComponent();
            new ClickEventHelper(grdSplitter).Click += Splitter_Click;
            foreach (MenuItem item in menuSort.Items)
            {
                if ((int)item.Tag == Configs.SortType)
                {
                    item.IsChecked = true;
                    break;
                }
            }
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
            filesViewer.Project = project;
            await filesViewer.SetFilesAsync(null);
            await classPanel.LoadAsync(project);

        }

        #region 分类面板



        /// <summary>
        /// 是否忽略类选择事件。当显示全部文件或未被分类的文件时，
        /// 类会被设置为null，但是又不需要进行事件响应，故有此字段。
        /// </summary>
        private bool ignoreClassChanged = false;

        /// <summary>
        /// 被选中的类发生了改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SelectedClassChanged(object sender, SelectedClassChangedEventArgs e)
        {
            if (ignoreClassChanged)
            {
                return;
            }
            if (classPanel.SelectedItem == null)
            {
                //这里应该是不需要有任何操作了
                //await SetFilesAsync(() => GetFilesByProjectAsync(Project.ID));
            }
            else
            {
                await SetFilesAsync(() => GetFilesByClassAsync(classPanel.SelectedItem.ID));
            }
        }

        #endregion

        #region 文件视图
        /// <summary>
        /// 设置FileViewer的文件
        /// </summary>
        /// <param name="func">用于获取文件的异步函数</param>
        /// <returns></returns>
        private async Task SetFilesAsync(Func<Task<List<File>>> func)
        {
            GetProgress().Show(true);
            Debug.WriteLine("Set Files, Project Hashcode is " + Project.GetHashCode()
            + ", Class is " + (classPanel.SelectedItem == null ? "null" : classPanel.SelectedItem.Name));
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

        /// <summary>
        /// 文件视图中的标签被单机了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesViewer_ClickTag(object sender, ClickTagEventArgs e)
        {
            if (e.Class != classPanel.SelectedItem)
            {
                classPanel.SelectedItem = e.Class;
            }
        }

        /// <summary>
        /// 有拖放落在了文件视图上方
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FileViewer_Drop(object sender, DragEventArgs e)
        {
            if (classPanel.SelectedItem == null)
            {
                return;
            }
            if (e.Data.GetDataPresent(nameof(ClassifyFiles)))
            {
                //这里是因为当文件拖出去的时候，会有一个名为ClassifyFiles的拖放格式
                //需要把这个格式排除掉，因为这是自己拖出去的
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await AddFilesAsync(classPanel.SelectedItem, files);
            }
        }

        /// <summary>
        /// 文件视图的类型发生了改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesViewer_ViewTypeChanged(object sender, EventArgs e)
        {
            btnLocateByDir.IsEnabled = filesViewer.CurrentFileView != FileView.Tree;
            btnSort.IsEnabled = filesViewer.CurrentFileView != FileView.Tree;
        }

        /// <summary>
        /// 单击全部文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnAllFiles_Click(object sender, RoutedEventArgs e)
        {
            ignoreClassChanged = true;
            classPanel.SelectedItem = null;
            ignoreClassChanged = false;
            await SetFilesAsync(() => GetFilesByProjectAsync(Project.ID));
        }

        /// <summary>
        /// 单击未分类文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnNoClassesFiles_Click(object sender, RoutedEventArgs e)
        {
            ignoreClassChanged = true;
            classPanel.SelectedItem = null;
            ignoreClassChanged = false;
            await SetFilesAsync(() => GetNoClassesFilesByProjectAsync(Project.ID));

        }

        /// <summary>
        /// 文件视图滚轮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                //需要通知显示设置面板里的滑动条
                this.Notify(nameof(IconSize), nameof(IconSizeString));
            }
        }

        #endregion

        #region 显示设置面板
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

        /// <summary>
        /// 双击缩放条，将缩放设置为默认值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconSize_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            sldIconSize.Value = 32;
        }


        #endregion

        #region 底部操作栏

        public HashSet<string> dirs;
        /// <summary>
        /// 所有的目录
        /// </summary>
        public HashSet<string> Dirs
        {
            get => dirs;
            set
            {
                dirs = value;
                this.Notify(nameof(Dirs));
            }
        }

        /// <summary>
        /// 跳转到某一个目录被选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void JumpToDirComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = e.AddedItems.Count == 0 ? null : e.AddedItems.Cast<string>().First();
            if (dir != null)
            {
                filesViewer.SelectFileByDir(dir);
                //延时1/4秒，让用户能够看到被选中的状态
                await Task.Delay(250);
                flyoutJumpToDir.Hide();// = false;
                (sender as ListBox).SelectedItem = null;
            }
        }

        /// <summary>
        /// 单击分类按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 单击添加文件/文件夹按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnAddFile_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = true,
                DefaultDirectory = Project.RootPath
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                await AddFilesAsync(classPanel.SelectedItem, dialog.FileNames.ToList());
            }
        }

        /// <summary>
        /// 向文件视图添加文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private async Task AddFilesAsync(Class c, IList<string> files)
        {
            var dialog = new AddFilesDialog(c, files)
            { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            if (c==classPanel.SelectedItem && dialog.AddedFiles != null)
            {
                await filesViewer.AddFilesAsync(dialog.AddedFiles);
            }
        }

        /// <summary>
        /// 单击刷新按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedClassChanged(null, null);
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
        #endregion

        #region 左侧分类列表面板

        /// <summary>
        /// 是否正在执行左侧面板伸展和收缩的动画
        /// </summary>
        bool isAnimating = false;

        /// <summary>
        /// 单击分界处
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Splitter_Click(object sender, EventArgs e)
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

        /// <summary>
        /// 开始执行宽度动画
        /// </summary>
        /// <param name="to"></param>
        private void StartAnimation(double to)
        {
            DoubleAnimation ani = new DoubleAnimation(to, TimeSpan.FromSeconds(0.5))
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut },
                //FillBehavior = FillBehavior.Stop
            };
            ani.Completed += (p1, p2) =>
            {
                isAnimating = false;
                grdSplitter.IsEnabled = true;
                //grdLeft.Width = to;
                //本来是写了上面注释掉的部分的，后来发现反正改变宽度都是用动画，那么就无所谓了
                grdMain.ColumnDefinitions[0].Width = GridLength.Auto;
            };
            grdLeft.BeginAnimation(WidthProperty, ani);
        }

        /// <summary>
        /// 在分界处鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Splitter_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //此处不是MouseMove，而是在移动结束后，通过动画的形式改变左侧面板的宽度，这样节省性能
            var cd = grdMain.ColumnDefinitions[0];
            if (cd.Width.IsAbsolute)
            {
                StartAnimation(cd.Width.Value - 8);
                //这边可能是有Margin什么的，要-8才能没有顿挫
            }
        }
        #endregion

        private async void ClassPanel_ClassFilesDrop(object sender, ClassFilesDropEventArgs e)
        {
            await AddFilesAsync(e.Class, e.Files);
        }
    }
}
