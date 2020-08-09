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
using ClassifyFiles.UI.Model;
using ClassifyFiles.Util;
using ClassifyFiles.UI.Panel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using ClassifyFiles.UI.Util;
using FzLib.Basic;
using Windows.Storage;

namespace ClassifyFiles.UI.Page
{

    public partial class FileBrowserPanel : ProjectPageBase
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
            if (project != Project)
            {
                await base.LoadAsync(project);
                filesViewer.Project = project;
                await filesViewer.SetFilesAsync(null, null, FileCollectionType.None);
            }
            await classPanel.LoadAsync(project);
            UpdateAppBarButtonsEnable();
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
            if (classPanel.SelectedUIClass == null)
            {
                await filesViewer.SetFilesAsync(null, null, FileCollectionType.None);
            }
            else
            {
                await SetFilesAsync(() =>
                {
                    var fileClasses = GetFilesWithClassesByClass(classPanel.SelectedUIClass.Class);
                    var uiFiles = fileClasses.Select(p => new UIFile(p.Key)
                    {
                        Classes = new ObservableCollection<Class>(p.Value),
                    }).ToList();
                    return uiFiles;
                }, FileCollectionType.Class);
            }
        }

        /// <summary>
        /// 分类面板接收到文件拖放信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ClassPanel_ClassFilesDrop(object sender, ClassFilesDropEventArgs e)
        {
            await MainWindow.Current.DoProcessAsync(Do());
            async Task Do()
            {

                if (e.Files != null)
                {
                    await AddFilesAsync(e.Class, e.Files);
                }
                else if (e.UIFiles != null)//说明是由软件内部拖放过来的
                {
                    IReadOnlyList<File> files = null;
                    if (currentFileCollectionType == FileCollectionType.NoClass)
                    {
                        //如果当前显示的是没有被分类的文件，那么当拖放之后，自然就不再是没有分类的文件了，所以要从列表中删去
                        await filesViewer.RemoveFilesAsync(e.UIFiles);
                    }
                    await Task.Run(() => files = AddFilesToClass(e.UIFiles.Select(p => p.File), e.Class));

                    foreach (var file in e.UIFiles)
                    {
                        await file.LoadClassesAsync(null);
                    }
                    await classPanel.UpdateUIClassesAsync();
                }
            }
        }

        #endregion

        #region 文件视图
        /// <summary>
        /// 设置FileViewer的文件
        /// </summary>
        /// <param name="filesWithClassesFunc">用于获取文件的异步函数</param>
        /// <returns></returns>
        private async Task<bool> SetFilesAsync(
              Func<List<UIFile>> getUIFiles,
              FileCollectionType type
          )
        {
            currentFileCollectionType = type;
            bool result = false;
            await MainWindow.Current.DoProcessAsync(Do());
            async Task Do()
            {
                Debug.WriteLine("Set Files, Project Hashcode is " + Project.GetHashCode()
            + ", Class is " + (classPanel.SelectedUIClass == null ? "null" : classPanel.SelectedUIClass.Class.Name));
                IEnumerable<UIFile> uiFiles = null;
                await Task.Delay(1);
                Class c = classPanel.SelectedUIClass?.Class;
                await Task.Run(() =>
                {
                    try
                    {
                        uiFiles = getUIFiles();
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                       {
                           new ErrorDialog().ShowAsync(ex, "查询文件失败");
                       });
                        return;
                    }
                    rawFiles = uiFiles;
                    if (FilterPattern.Length > 0)
                    {
                        Regex r = new Regex(FilterPattern);
                        uiFiles = uiFiles.Where(p => r.IsMatch(p.File.Name));
                    }
                    uiFiles.ForEach(p => p.Class = c);
                    result = true;
                });
                await filesViewer.SetFilesAsync(uiFiles, c, type);

                await classPanel.UpdateUIClassesAsync();
                await ApplyDirs();
            }
            return result;
        }
        private IEnumerable<UIFile> rawFiles = null;
        private async Task ApplyFilterAsync()
        {
            await MainWindow.Current.DoProcessAsync(Do());
            async Task Do()
            {
                IEnumerable<UIFile> files = rawFiles;
                if (rawFiles != null)
                {
                    if (FilterPattern.Length > 0)
                    {
                        await Task.Run(() =>
                        {
                            Regex r = new Regex(FilterPattern);
                            files = files.Where(p => r.IsMatch(p.File.Name));
                        });
                    }
                    else
                    {
                        files = rawFiles;
                    }
                    await filesViewer.SetFilesAsync(files, classPanel.SelectedUIClass?.Class, filesViewer.FileCollectionType);
                    await ApplyDirs();
                }
            }
        }

        private async Task ApplyDirs()
        {
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
        }

        /// <summary>
        /// 文件视图中的标签被单击了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesViewer_ClickTag(object sender, ClickTagEventArgs e)
        {
            if (e.Class != classPanel.SelectedUIClass.Class)
            {
                classPanel.SelectedUIClass = classPanel.UIClasses.First(p => p.Class == e.Class);
            }
        }

        /// <summary>
        /// 有拖放落在了文件视图上方
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FileViewer_Drop(object sender, DragEventArgs e)
        {
            if (classPanel.SelectedUIClass == null)
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
                await AddFilesAsync(classPanel.SelectedUIClass.Class, files);
            }
        }

        /// <summary>
        /// 文件视图的类型发生了改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesViewer_ViewTypeChanged(object sender, EventArgs e)
        {
            UpdateAppBarButtonsEnable();
        }

        private void UpdateAppBarButtonsEnable()
        {
            btnLocateByDir.IsEnabled = filesViewer.CurrentFileView != FileView.Tree
              && Configs.SortType == (int)SortType.Default;
            btnSort.IsEnabled = filesViewer.CurrentFileView != FileView.Tree;
        }



        private async Task SetSpecialFiles(Func<Project, IEnumerable<KeyValuePair<File, IEnumerable<Class>>>> func, FileCollectionType type)
        {
            ignoreClassChanged = true;
            classPanel.SelectedUIClass = null;
            ignoreClassChanged = false;
            await SetFilesAsync(() =>
            {
                var files = func(Project);
                return files.Select(p => new UIFile(p.Key)
                {
                    Classes = new ObservableCollection<Class>(),
                }).ToList();
            }, type);
        }
        /// <summary>
        /// 单击全部文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnAllFiles_Click(object sender, RoutedEventArgs e)
        {
            await SetSpecialFiles(GetFilesWithClassesByProject, FileCollectionType.All);
        }

        private async void BtnManualClassFiles_Click(object sender, RoutedEventArgs e)
        {
            await SetSpecialFiles(GetManualFilesWithClassesByProject, FileCollectionType.Manual);
        }

        private async void BtnDisabledClassFiles_Click(object sender, RoutedEventArgs e)
        {
            await SetSpecialFiles(GetDisabledFilesWithClassesByProject, FileCollectionType.Disabled);
        }

        /// <summary>
        /// 单击未分类文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnNoClassesFiles_Click(object sender, RoutedEventArgs e)
        {
            ignoreClassChanged = true;
            classPanel.SelectedUIClass = null;
            ignoreClassChanged = false;
            await SetFilesAsync(() =>
            {
                var fileClasses = GetNoClassesFilesByProject(Project);
                var uiFiles = fileClasses.Select(p => new UIFile(p)
                {
                    Classes = new ObservableCollection<Class>(),
                }).ToList();
                return uiFiles;
            }, FileCollectionType.NoClass);
        }

        public FileCollectionType currentFileCollectionType = FileCollectionType.None;

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
        public bool ShowFileTime
        {
            get => Configs.ShowFileTime;
            set
            {
                Configs.ShowFileTime = value;
                //filesViewer.Refresh();
            }
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

        public ThumbnailStrategy ThumbnailStrategy
        {
            get => Configs.ThumbnailStrategy;
            set
            {
                Configs.ThumbnailStrategy = value;
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
        public bool FileIconUniformToFill
        {
            get => Configs.FileIconUniformToFill;
            set
            {
                Configs.FileIconUniformToFill = value;
                filesViewer.Refresh();
            }
        }
        public bool TreeSimpleTemplate
        {
            get => Configs.TreeSimpleTemplate;
            set
            {
                Configs.TreeSimpleTemplate = value;
                if (filesViewer.CurrentFileView == FileView.Tree)
                {
                    filesViewer.Refresh();
                }
            }
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
                UpdateAppBarButtonsEnable();
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

        private void flyoutDisplay_Opening(object sender, object e)
        {
            this.Notify(nameof(IconSize), nameof(IconSizeString));
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
                await filesViewer.SelectFileByDirAsync(dir);
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
            await MainWindow.Current.DoProcessAsync(Do());
            async Task Do()
            {
                var dialog = new UpdateFilesDialog(Project) { Owner = Window.GetWindow(this) };
                dialog.ShowDialog();
                if (dialog.Updated)
                {
                    classPanel.SelectedUIClass = null;
                    await classPanel.UpdateUIClassesAsync();
                }
            }
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
                await AddFilesAsync(classPanel.SelectedUIClass.Class, dialog.FileNames.ToList());
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
            if (c == classPanel.SelectedUIClass.Class && dialog.AddedFiles != null)
            {
                await filesViewer.AddFilesAsync(dialog.AddedFiles);
            }
            await classPanel.UpdateUIClassesAsync();
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

        /// <summary>
        /// 排序类型的单选框被选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RadioMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || isSettingSortChecked)
            {
                return;
            }
            await MainWindow.Current.DoProcessAsync(Do());
            async Task Do()
            {
                SortType type = (SortType)(sender as FrameworkElement).Tag;
                await filesViewer.SortAsync(type);
                Configs.SortType = (int)type;
                Configs.GroupByDir = false;

                this.Notify(nameof(GroupByDir));
                UpdateAppBarButtonsEnable();
            }
        }

        private string filterContent = "";
        public string FilterPattern
        {
            get => filterContent;
            set
            {
                filterContent = value;
                this.Notify(nameof(FilterPattern));
            }
        }

        /// <summary>
        /// 单击独立显示文件查看窗口的按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFileViewerWindow_Click(object sender, RoutedEventArgs e)
        {
            filesViewer.ShowAsWindow();
        }

        /// <summary>
        /// 单击筛选按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            flyoutFilter.Hide();
            await ApplyFilterAsync();
        }

        /// <summary>
        /// 单击清除筛选按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPattern = "";
            flyoutFilter.Hide();
            await ApplyFilterAsync();
        }
        /// <summary>
        /// 筛选弹窗打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void FlyoutFilter_Opened(object sender, object e)
        {
            //无效
            txtFilter.Focus();
            Keyboard.Focus(txtFilter);
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
            //暂时固定宽度，不然会卡顿
            filesViewer.Width = filesViewer.ActualWidth;
            DoubleAnimation ani = new DoubleAnimation(to, Configs.AnimationDuration * 2)
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut },
            };
            ani.Completed += (p1, p2) =>
            {
                filesViewer.Width = double.NaN;
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


    }


}
