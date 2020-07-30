using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using System;
using FzLib.Basic;
using FzLib.Extension;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassifyFiles.Util;
using System.Diagnostics;
using ModernWpf.Controls;
using ClassifyFiles.UI.Component;
using static ClassifyFiles.Util.FileClassUtility;
using System.Collections;
using System.Collections.Concurrent;
using ListView = System.Windows.Controls.ListView;
using System.Collections.Specialized;
using System.Windows.Data;
using FI = System.IO.FileInfo;
using ClassifyFiles.UI.Util;
using ClassifyFiles.Enum;
using ClassifyFiles.UI.Event;
using System.Runtime.InteropServices;
using ClassifyFiles.Util.Win32;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using ClassifyFiles.UI.Dialog;

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// FIlesViewer.xaml 的交互逻辑
    /// </summary>
    public partial class FilesViewer : UserControl, INotifyPropertyChanged
    {
        public FilesViewer()
        {
            DataContext = this;
            InitializeComponent();
            SetGroupEnable(Configs.GroupByDir);
            treeViewHelper = new TreeViewSelectorHelper<UIFile>(
                FindResource("treeFiles") as TreeView,
                p => p.Parent,
                p => p.SubUIFiles,
                (c, p) => c.Parent = p
                );
            new DragDropFilesHelper(FindResource("lvwFiles") as ListBox).Regist();
            new DragDropFilesHelper(FindResource("grdFiles") as ListBox).Regist();
            new DragDropFilesHelper(FindResource("lvwDetailFiles") as ListBox).Regist();
            new DragDropFilesHelper(FindResource("treeFiles") as TreeView).Regist();
            var btn = grdAppBar.Children.OfType<AppBarToggleButton>()
                .FirstOrDefault(p => int.Parse(p.Tag as string) == Configs.LastViewType);
            if (btn != null)
            {
                ViewTypeButton_Click(btn, new RoutedEventArgs());
            }

            FileIcon.Tasks.ProcessStatusChanged += TaskQueue_ProcessStatusChanged;

        }
        private bool IsSingleWindow { get; set; } = false;
        public Window ShowAsWindow()
        {
            //由于本来一个文件只会同时显示一个图标，所以可以用缓存
            //但是开启独立窗口后，一个文件可能同时会有多个显示，因此需要禁用缓存
            FileIcon.StaticEnableCache = false;
            WindowBase win = new WindowBase()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "文件查看",
            };
            FilesViewer fv = new FilesViewer()
            {
                Project = Project,
                Files = Files,
                FileTree = FileTree,
                CurrentClass = CurrentClass,
                IsSingleWindow = true
            };
            win.Content = fv;
            win.Show();
            return win;
        }

        #region 属性和字段
        private TreeViewSelectorHelper<UIFile> treeViewHelper;

        private ItemsControl filesContent;
        public ItemsControl FilesContent
        {
            get => filesContent;
            set
            {
                filesContent = value;
                this.Notify(nameof(FilesContent));
            }
        }

        public Class CurrentClass { get; private set; }

        private Project project;
        /// <summary>
        /// 当前项目
        /// </summary>
        public virtual Project Project
        {
            get => project;
            set
            {
                project = value;
                this.Notify(nameof(Project));
            }
        }

        private ObservableCollection<UIFile> files;
        public ObservableCollection<UIFile> Files
        {
            get => files;
            set
            {
                files = value;
                this.Notify(nameof(Files));
            }
        }

        private ObservableCollection<UIFile> fileTree;
        /// <summary>
        /// 供树状图使用的文件树
        /// </summary>
        public ObservableCollection<UIFile> FileTree
        {
            get => fileTree;
            set
            {
                fileTree = value;
                this.Notify(nameof(FileTree));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region 文件相关

        /// <summary>
        /// 设置文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task SetFilesAsync(IEnumerable<UIFile> files, Class currentClass)
        {
            CurrentClass = currentClass;
            this.Notify(nameof(CurrentClass));
            if (files == null || !files.Any())
            {
                //如果为空
                Files = new ObservableCollection<UIFile>();
            }
            else
            {
                if (Configs.SortType == 0)
                {
                    //如果使用默认排序（已经在数据库那边排了）
                    Files = new ObservableCollection<UIFile>(files);
                }
                else
                {
                    await SortAsync((SortType)Configs.SortType, files);
                }
            }
            FileTree = await GetFileTreeAsync();
        }
        //public async Task SetFilesAsync(IEnumerable<File> files)
        //{
        //    if (files == null || !files.Any())
        //    {
        //        //如果为空
        //        Files = new ObservableCollection<UIFile>();
        //    }
        //    else
        //    {
        //        List<UIFile> filesWithIcon = new List<UIFile>();
        //        await Task.Run(() =>
        //        {
        //            //把每个File要转换为UIFile
        //            foreach (var file in files)
        //            {
        //                UIFile uiFile = new UIFile(file);
        //                filesWithIcon.Add(uiFile);
        //            }
        //        });

        //        if (Configs.SortType == 0)
        //        {
        //            //如果使用默认排序（已经在数据库那边排了）
        //            Files = new ObservableCollection<UIFile>(filesWithIcon);
        //        }
        //        else
        //        {
        //            await SortAsync((SortType)Configs.SortType, filesWithIcon);
        //        }
        //    }
        //    ResetFileTree();
        //}
        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public async Task AddFilesAsync(IEnumerable<File> files, bool tags = true)
        {
            List<UIFile> filesWithIcon = new List<UIFile>();
            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    UIFile uiFile = new UIFile(file);
                    //await uiFile.LoadTagsAsync(Project);
                    filesWithIcon.Add(uiFile);
                }
            });
            foreach (var file in filesWithIcon)
            {
                Files.Add(file);
                await file.LoadClassesAsync();
            }

            this.Notify(nameof(Files));
            FileTree = await GetFileTreeAsync();
            if (filesWithIcon.Count > 0)
            {
                await SelectFileAsync(filesWithIcon.First());
            }
        }
        /// <summary>
        /// 移除文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task RemoveFilesAsync(IEnumerable<UIFile> files)
        {
            foreach (var file in files)
            {
                Files.Remove(file);

                if (CurrentFileView == FileView.Tree && GetSelectedFile() != null)
                {
                    //树状图需要单独移除
                    await treeViewHelper.RemoveItemAsync(file, FileTree);
                }
            }
        }
        /// <summary>
        /// 重新设置树状图
        /// </summary>
        private async Task<ObservableCollection<UIFile>> GetFileTreeAsync()
        {
            ObservableCollection<UIFile> fileTree = new ObservableCollection<UIFile>();
            await Task.Run(() =>
            {
                if (Files == null || Files.Count == 0)
                {
                    fileTree = null;
                }
                else
                {
                    fileTree = new ObservableCollection<UIFile>(
                   FileUtility.GetFileTree(Project, Files,
                       p => new UIFile(p),
                       p => p.File,
                       p => p.SubUIFiles,
                       (c, p) => c.Parent = p)
                   .SubUIFiles);
                }
            });
            return fileTree;

        }
        /// <summary>
        /// 获取被选中的文件（1个）
        /// </summary>
        /// <returns></returns>
        private UIFile GetSelectedFile()
        {
            return FilesContent switch
            {
                ListBox lvw => lvw.SelectedItem as UIFile,
                TreeView t => t.SelectedItem as UIFile,
                _ => null,
            };
        }

        /// <summary>
        /// 获取所有被选中的文件
        /// </summary>
        /// <returns></returns>
        private IReadOnlyList<UIFile> GetSelectedFiles()
        {
            return FilesContent switch
            {
                ListBox lvw => lvw.SelectedItems.Cast<UIFile>().ToList().AsReadOnly(),
                TreeView t => new List<UIFile>() { t.SelectedItem as UIFile }.AsReadOnly(),
                _ => new List<UIFile>().AsReadOnly(),
            };
        }

        /// <summary>
        /// 选择指定文件夹的第一个文件
        /// </summary>
        /// <param name="dir"></param>
        public async Task SelectFileByDirAsync(string dir)
        {
            UIFile file = Files.FirstOrDefault(p => p.File.Dir == dir);
            if (file != null)
            {
                await SelectFileAsync(file);
            }
        }

        /// <summary>
        /// 选中指定文件
        /// </summary>
        /// <param name="file"></param>
        public async Task SelectFileAsync(UIFile file)
        {
            if (FilesContent is ListBox lbx)
            {
                lbx.SelectedItem = file;
                if (file != null)
                {
                    lbx.ScrollIntoView(file);
                }
            }
            else if (FilesContent is TreeView tree)
            {
                if (file == null)
                {

                    await treeViewHelper.ClearSelectionAsync(FileTree);
                }
                else
                {
                    await treeViewHelper.SelectItemWhileLoadedAsync(file, FileTree);
                }
            }
        }

        /// <summary>
        /// 刷新，实质就是重新设置一遍Files
        /// </summary>
        public void Refresh()
        {
            FileIcon.ClearCaches();
            var files = Files;
            Files = null;
            if (files == null || !files.Any())
            {
                Files = new ObservableCollection<UIFile>();
            }
            else
            {
                Files = new ObservableCollection<UIFile>(files);
            }
            if (CurrentFileView == FileView.Tree)
            {
                DataTemplate dataTemplate = FindResource(Configs.TreeSimpleTemplate ? "treeSimpleDataTemplate" : "listDataTemplate") as DataTemplate;
                if (FilesContent.ItemTemplate != dataTemplate)
                {
                    FilesContent.ItemTemplate = dataTemplate;
                }

            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rawFiles"></param>
        /// <returns></returns>
        public async Task SortAsync(SortType type, IEnumerable<UIFile> rawFiles = null)
        {
            IEnumerable<UIFile> files = null;
            if (rawFiles == null)
            {
                rawFiles = Files;
            }
            await Task.Run(() =>
            {
                switch (type)
                {
                    case SortType.Default:
                        files = rawFiles
                            .OrderBy(p => p.File.Dir)
                            .ThenBy(p => p.File.Name);
                        break;
                    case SortType.NameUp:
                        files = rawFiles
                            .OrderBy(p => p.File.Name)
                            .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.NameDown:
                        files = rawFiles
                           .OrderByDescending(p => p.File.Name)
                           .ThenByDescending(p => p.File.Dir);
                        break;
                    case SortType.LengthUp:
                        files = rawFiles
                          .OrderBy(p => GetFileInfoValue(p, nameof(FI.Length)))
                          .ThenBy(p => p.File.Name)
                          .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.LengthDown:
                        files = rawFiles
                          .OrderByDescending(p => GetFileInfoValue(p, nameof(FI.Length)))
                          .ThenByDescending(p => p.File.Name)
                          .ThenByDescending(p => p.File.Dir);
                        break;
                    case SortType.LastWriteTimeUp:
                        files = rawFiles
                          .OrderBy(p => GetFileInfoValue(p, nameof(FI.LastWriteTime)))
                          .ThenBy(p => p.File.Name)
                          .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.LastWriteTimeDown:
                        files = rawFiles
                          .OrderByDescending(p => GetFileInfoValue(p, nameof(FI.LastWriteTime)))
                          .ThenByDescending(p => p.File.Name)
                          .ThenByDescending(p => p.File.Dir);
                        break;
                    case SortType.CreationTimeUp:
                        files = rawFiles
                          .OrderBy(p => GetFileInfoValue(p, nameof(FI.CreationTime)))
                          .ThenBy(p => p.File.Name)
                          .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.CreationTimeDown:
                        files = rawFiles
                          .OrderByDescending(p => GetFileInfoValue(p, nameof(FI.CreationTime)))
                          .ThenByDescending(p => p.File.Name)
                          .ThenByDescending(p => p.File.Dir);
                        break;
                }
                //在非UI线程里就得把Lazy的全都计算好
                files = files.ToArray();
            });
            Files = new ObservableCollection<UIFile>(files);
            //当排序开启时，一律不支持分组
            SetGroupEnable(false);

            //若文件不存在，直接访问FileInfo属性会报错，因此需要加一层try
            static long GetFileInfoValue(UIFile file, string name)
            {
                try
                {
                    return name switch
                    {
                        nameof(FI.Length) => file.FileInfo.Length,
                        nameof(FI.LastWriteTime) => file.FileInfo.LastWriteTime.Ticks,
                        nameof(FI.CreationTime) => file.FileInfo.CreationTime.Ticks,
                        _ => throw new NotImplementedException(),
                    };
                }
                catch
                {
                    return 0;
                }
            }
        }

        #endregion

        #region 视图相关

        /// <summary>
        /// 获取“处理中”圈圈
        /// </summary>
        /// <returns></returns>
        protected ProgressDialog GetProgress()
        {
            return (Window.GetWindow(this) as MainWindow).Progress;
        }

        /// <summary>
        /// 视图类型改变事件
        /// </summary>
        public event EventHandler ViewTypeChanged;

        /// <summary>
        /// 视图类型的5个按钮的单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ViewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            int type = int.Parse((sender as FrameworkElement).Tag as string);
            //由于并不是RadioButton，因此需要手动设置IsChecked值
            //为了防止递归，这边采用的是Click事件而不是Checked事件
            grdAppBar.Children.OfType<AppBarToggleButton>().ForEach(p => p.IsChecked = false);
            (sender as AppBarToggleButton).IsChecked = true;
            CurrentFileView = (FileView)type;
            await Task.Delay(1);//让UI先响应
            await RefreshFileViewAsync();
        }

        /// <summary>
        /// 设置是否分组
        /// </summary>
        /// <param name="enable"></param>
        public void SetGroupEnable(bool enable)
        {
            foreach (var list in Resources.Values.OfType<ListBox>())
            {
                //如果启用，那么ItemsSource绑定为xaml中的分组数据源
                if (enable)
                {
                    list.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = FindResource("listDetailItemsSource") as CollectionViewSource });
                }
                else
                {
                    list.SetBinding(ItemsControl.ItemsSourceProperty, nameof(Files));
                }
            }
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        private async Task RefreshFileViewAsync()
        {
            var selectedFile = GetSelectedFile();
            if (CurrentFileView == FileView.List)
            {
                FilesContent = FindResource("lvwFiles") as ListBox;

            }
            else if (CurrentFileView == FileView.Icon || CurrentFileView == FileView.Tile)
            {
                FilesContent = FindResource("grdFiles") as ListBox;
                FilesContent.ItemTemplate = FindResource(CurrentFileView == FileView.Icon ? "grdIconView" : "grdTileView") as DataTemplate;
            }
            else if (CurrentFileView == FileView.Tree)
            {
                FilesContent = FindResource("treeFiles") as TreeView;
                FilesContent.ItemTemplate = FindResource(Configs.TreeSimpleTemplate ? "treeSimpleDataTemplate" : "listDataTemplate") as DataTemplate;
            }
            else if (CurrentFileView == FileView.Detail)
            {
                FilesContent = FindResource("lvwDetailFiles") as ListView;
            }

            ViewTypeChanged?.Invoke(this, new EventArgs());

            if (selectedFile != null)
            {
                await SelectFileAsync(selectedFile);
            }
            await Task.Run(() =>
            Configs.LastViewType = (int)CurrentFileView);
        }


        /// <summary>
        /// 当前的视图
        /// </summary>
        public FileView CurrentFileView { get; private set; } = FileView.List;

        #endregion

        #region 事件处理

        /// <summary>
        /// 任务队列状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskQueue_ProcessStatusChanged(object sender, ProcessStatusChangedEventArgs e)
        {
            progress.IsActive = e.IsRunning;
        }

        /// <summary>
        /// 双击事件，用于打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Viewer_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(e.OriginalSource as DependencyObject);
            if (parent is RepeatButton
                || parent is Thumb)
            {
                //由于双击滚动条也会触发该事件，因此需要判断该事件是不是从滚动条发出来的。
                //没有很好的方法来判断是不是滚动条
                return;
            }
            try
            {
                UIFile uiFile = GetSelectedFile();
                var files = GetSelectedFiles();
                if (files.Count != 1)
                {
                    return;
                }
                var file = files[0].File;
                if (file != null)
                {
                    if (file.IsFolder && CurrentFileView == FileView.Tree
                        && uiFile.SubUIFiles != null && uiFile.SubUIFiles.Count > 0)//是目录
                    {
                        return;
                    }
                    string path = file.GetAbsolutePath();

                    if (!file.IsFolder && !System.IO.File.Exists(path))
                    {
                        await new ErrorDialog().ShowAsync("文件不存在", "打开失败");
                        e.Handled = true;
                        return;
                    }
                    else if (file.IsFolder && !System.IO.Directory.Exists(path))
                    {
                        await new ErrorDialog().ShowAsync("文件夹不存在", "打开失败");
                        e.Handled = true;
                        return;
                    }
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo()
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    p.Start();

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "打开失败");
            }
        }

        /// <summary>
        /// 鼠标滚轮事件。当按下Ctrl时滚动滚轮，将能够缩放。全面接管滚动，自己写了一个动画滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && CurrentFileView != FileView.Detail)
            {
                Configs.IconSize += e.Delta / 30;
                e.Handled = true;
            }
            else
            {
                ScrollViewer scr = FilesContent.GetVisualChild<ScrollViewer>();
                if (scr != null)
                {
                    e.Handled = true;

                    await SmoothScrollViewerHelper.HandleMouseWheel(scr, e.Delta);
                }
            }
        }

        /// <summary>
        /// 树状图鼠标右键按下事件，目的是让鼠标按下时就能选中鼠标下面的项，不然右键菜单就会不对应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeFiles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }

            static TreeViewItem VisualUpwardSearch(DependencyObject source)
            {
                while (source != null && !(source is TreeViewItem))
                    source = VisualTreeHelper.GetParent(source);

                return source as TreeViewItem;
            }
        }

        /// <summary>
        /// 右键菜单打开目录单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenDirMernuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedFile() != null)
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select, \"{GetSelectedFile().File.GetAbsolutePath(false)}\"",
                    UseShellExecute = true//在.Net Core中，需要加这一条才能够正常运行
                };
                p.Start();
            }
        }

        /// <summary>
        /// 标签被按下事件，用于跳转到指定类和删除标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Tags_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Class c = (e.Source as ContentPresenter).Content as Class;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ClickTag?.Invoke(this, new ClickTagEventArgs(c));
                //向FileBrowsePanel传递
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                TagGroup tg = sender as TagGroup;
                var file = tg.File.File;
                tg.File.Classes.Remove(c);
                await Task.Run(() => RemoveFilesFromClass(new File[] { file }, c));
            }
            e.Handled = true;
        }

        /// <summary>
        /// 标签被单击事件
        /// </summary>
        public event EventHandler<ClickTagEventArgs> ClickTag;

        /// <summary>
        /// 搜索框的文本改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SearchTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string txt = sender.Text.ToLower();
                var suggestions = Files == null ? new List<UIFile>() :
                    Files.Where(p => (p.File.IsFolder ? p.File.Dir : p.File.Name).ToLower().Contains(txt)).ToList();

                sender.ItemsSource = suggestions.Count > 0 ?
                    suggestions : new string[] { "结果为空" } as IEnumerable;
            }
        }

        /// <summary>
        /// 搜索框比选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void SearchTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            UIFile file = args.ChosenSuggestion as UIFile;
            await SelectFileAsync(file);
        }

        /// <summary>
        /// 键盘按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.D) && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                await SelectFileAsync(null);
            }
        }

        /// <summary>
        /// 提示框打开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ToolTip_Opened(object sender, RoutedEventArgs e)
        {
            //光标在Item之间移动的时候ToolTip有几率会显示同一张图，目前还不知道原因
            ToolTip tt = sender as ToolTip;
            FileIcon icon = (tt.Content as System.Windows.Controls.Panel).Children.OfType<FileIcon>().First();
            if ((sender as ToolTip).Visibility == Visibility.Visible && icon.Visibility == Visibility.Visible)
            {
                await icon.LoadImageAsync();
            }
        }
        #endregion

        #region 菜单相关



        /// <summary>
        /// 右键菜单打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = FindResource("menu") as ContextMenu;
            menu.Items.Clear();
            var files = GetSelectedFiles();
            if (files == null || files.Count == 0)
            {
                //没有被选中的文件的话，就只能一闪而过了
                menu.IsOpen = false;
                return;
            }
            if (files.Count == 1)
            {
                MenuItem menuOpenFolder = new MenuItem() { Header = "打开目录" };
                menuOpenFolder.Click += OpenDirMernuItem_Click;
                menu.Items.Add(menuOpenFolder);
            }

            MenuItem menuCopy = new MenuItem() { Header = "复制" };
            menuCopy.Click += MenuCopy_Click;
            menu.Items.Add(menuCopy);

            if (!IsSingleWindow)
            {
                MenuItem menuDelete = new MenuItem() { Header = "删除记录", ToolTip = "这不会删除磁盘上的文件，仅仅删除记录" };
                menuDelete.Click += MenuDelete_Click;
                menu.Items.Add(menuDelete);
            }

            if (files.Count == 1)
            {
                if (files[0].File.IsImage())
                {
                    MenuItem menuShowExifs = new MenuItem() { Header = "查看Exif信息属性" };
                    menuShowExifs.Click += (p1, p2) =>
                        new ImageExifDialog(files[0].File.GetAbsolutePath()) { Owner = Window.GetWindow(this) }.ShowDialog();
                    menu.Items.Add(menuShowExifs);

                }

                MenuItem menuShowProperties = new MenuItem() { Header = "属性" };
                menuShowProperties.Click += (p1, p2) =>
                    FileProperty.ShowFileProperties(files[0].File.GetAbsolutePath());
                menu.Items.Add(menuShowProperties);
            }

            if (!IsSingleWindow)
            {
                menu.Items.Add(new Separator());

                foreach (var tag in Project.Classes)
                {
                    bool? isChecked = null;
                    if (!files.Any(p => p.Classes == null))
                    {
                        //首先确保被选中的文件都知道它们的类。
                        //因为时虚拟列表，所以没有显示的部分可能是还没有得到他们的类的。
                        if (files.Any(p => p.Classes.Any(q => q.ID == tag.ID)))
                        {
                            //如果有一部分或全部都属于该类
                            if (files.All(p => p.Classes.Any(q => q.ID == tag.ID)))
                            {
                                //如果全部属于该类
                                isChecked = true;
                            }
                            //这里else  isChecked = null;
                        }
                        else
                        {
                            isChecked = false;
                        }
                    }
                    CheckBox chk = new CheckBox()
                    {
                        Content = tag.Name,
                        IsChecked = isChecked,
                        Tag = tag
                    };
                    chk.Click += ChkTag_Click;
                    menu.Items.Add(chk);
                }
            }

        }

        private async void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            var files = GetSelectedFiles();
            GetProgress().Show();
            await Task.Run(() =>
            FileUtility.DeleteFilesRecord(files.Select(p => p.File)));
            foreach (var file in files)
            {
                Files.Remove(file);
            }
            if (CurrentFileView == FileView.Tree && GetSelectedFile() != null)
            {
                await treeViewHelper.RemoveItemAsync(files[0], FileTree);
            }
            GetProgress().Close();
        }

        private async void ChkTag_Click(object sender, RoutedEventArgs e)
        {
            GetProgress().Show();
            await Task.Delay(100);
            var files = GetSelectedFiles();
            CheckBox chk = sender as CheckBox;
            Class tag = chk.Tag as Class;

            if (chk.IsChecked == true)
            {
                //添加到类
                await Task.Run(() => AddFilesToClass(files.Select(p => p.File), tag));
                foreach (var file in files)
                {
                    var newC = file.Classes.FirstOrDefault(p => p.ID == tag.ID);
                    if (newC == null)
                    {
                        file.Classes.Add(tag);
                    }
                }
            }
            else
            {
                //从类中删除
                await Task.Run(() => RemoveFilesFromClass(files.Select(p => p.File), tag));
                foreach (var file in files)
                {
                    var c = file.Classes.FirstOrDefault(p => p.ID == tag.ID);
                    if (c != null)
                    {
                        file.Classes.Remove(c);
                    }
                }
            }

            GetProgress().Close();
        }

        /// <summary>
        /// 复制菜单单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            var files = new StringCollection();
            if (CurrentFileView != FileView.Tree)
            {
                files.AddRange(GetSelectedFiles().Select(p => p.File.GetAbsolutePath()).ToArray());
            }
            else
            {
                files.Add(GetSelectedFile().File.GetAbsolutePath());

            }
            Clipboard.SetFileDropList(files);
        }


        #endregion


    }

}
