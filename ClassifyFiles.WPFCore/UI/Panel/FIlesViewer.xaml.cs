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
using System.Windows.Controls.Primitives;
using System.Collections.Concurrent;
using ListView = System.Windows.Controls.ListView;

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// FIlesViewer.xaml 的交互逻辑
    /// </summary>
    public partial class FilesViewer : UserControl, INotifyPropertyChanged
    {

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
        protected ProgressDialog GetProgress()
        {
            return (Window.GetWindow(this) as MainWindow).Progress;
        }
        private Project project;
        public virtual Project Project
        {
            get => project;
            set
            {
                project = value;
                this.Notify(nameof(Project));
            }
        }
        public FilesViewer()
        {
            DataContext = this;
            InitializeComponent();
            FilesContent = FindResource("lvwFiles") as ListBox;
            new DragDropFilesHelper(FindResource("lvwFiles") as ListBox).Regist();
            new DragDropFilesHelper(FindResource("grdFiles") as ListBox).Regist();
            var btn = grdAppBar.Children.OfType<AppBarToggleButton>()
                .FirstOrDefault(p => int.Parse(p.Tag as string) == Configs.LastViewType);
            if(btn!=null)
            {
                ViewTypeButton_Click(btn, new RoutedEventArgs());
            }

        }
        private ObservableCollection<UIFile> files;
        public ObservableCollection<UIFile> Files
        {
            get => files;
            set
            {
                files = value;
                this.Notify(nameof(Files), nameof(FileTree));
            }
        }
        /// <summary>
        /// 供树状图使用的文件树
        /// </summary>
        public List<UIFile> FileTree => Files == null ? null : new List<UIFile>(
            FileUtility.GetFileTree<UIFile>(Project, Files,p=>new UIFile(p),p=>p.File.Dir,p=>p.SubUIFiles)
            .SubUIFiles);

        private double iconSize = 64;
        public double IconSize
        {
            get => iconSize;
            set
            {
                iconSize = value;
                this.Notify(nameof(IconSize));
            }
        }
        public const int pagingItemsCount = 120;

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task SetFilesAsync(IEnumerable<File> files)
        {
            if (files == null || !files.Any())
            {
                Files = new ObservableCollection<UIFile>();
            }
            else
            {
                List<UIFile> filesWithIcon = new List<UIFile>();
                await Task.Run(() =>
               {
                   foreach (var file in files)
                   {
                       UIFile uiFile = new UIFile(file);
                       filesWithIcon.Add(uiFile);
                   }
               });
                Files = new ObservableCollection<UIFile>(filesWithIcon);
                await Task.Delay(100);//不延迟大概率会一直转圈
                //await RealtimeRefresh(Files.Take(100));
            }
        }
        public async Task SetFilesAsync(IEnumerable<UIFile> files)
        {
            if (files == null || !files.Any())
            {
                Files = new ObservableCollection<UIFile>();
            }
            else
            {
                Files = new ObservableCollection<UIFile>(files);
                await Task.Delay(100);//不延迟大概率会一直转圈
                //await RealtimeRefresh(Files.Take(100));
            }
        }
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
                await file.LoadAsync();
            }

            this.Notify(nameof(Files));
        }

        private UIFile GetSelectedFile()
        {
            return FilesContent switch
            {
                ListBox lvw => lvw.SelectedItem as UIFile,
                TreeView t => t.SelectedItem as UIFile,
                _ => null,
            };
        }
        private IReadOnlyList<UIFile> GetSelectedFiles()
        {
            return FilesContent switch
            {
                ListBox lvw => lvw.SelectedItems.Cast<UIFile>().ToList().AsReadOnly(),
                TreeView t => new List<UIFile>() { t.SelectedItem as UIFile }.AsReadOnly(),
                _ => new List<UIFile>().AsReadOnly(),
            };
        }

        private async void lvwFiles_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                File file = GetSelectedFile()?.File;
                if (file != null)
                {
                    if (file.IsFolder)//是目录
                    {
                        return;
                    }
                    string path = file.GetAbsolutePath();

                    if (!System.IO.File.Exists(path))
                    {
                        await new ErrorDialog().ShowAsync("文件不存在", "打开失败");
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

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                UIFileSize.DefualtIconSize += e.Delta / 30;
                Files.ForEach(p => p.Size.UpdateIconSize());
                e.Handled = true;
            }
        }

        private void treeFiles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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

        private void OpenDirMernuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedFile() != null)
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select, \"{GetSelectedFile().File.GetAbsolutePath(false)}\"",
                    UseShellExecute = true
                };
                p.Start();
            }
        }

        public async Task RefreshAsync()
        {
            FileIcon.ClearCaches();
            var files = Files;
            Files = null;
           await SetFilesAsync(files);
        }

        public event EventHandler ViewTypeChanged;
        private void ViewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            int type = int.Parse((sender as FrameworkElement).Tag as string);
            grdAppBar.Children.OfType<AppBarToggleButton>().ForEach(p => p.IsChecked = false);
            (sender as AppBarToggleButton).IsChecked = true;
            CurrentViewType = type;
                ChangeViewType(type);
        }

        private void ChangeViewType(int type)
        {
            var selectedFile = GetSelectedFile();
            if (type == 1)
            {
                FilesContent = FindResource("lvwFiles") as ListBox;
                if (selectedFile != null)
                {
                    (FilesContent as ListBox).SelectedItem = selectedFile;
                    (FilesContent as ListBox).ScrollIntoView(selectedFile);
                }
            }
            else if (type == 2 || type == 3)
            {
                FilesContent = FindResource("grdFiles") as ListBox;
                FilesContent.ItemTemplate = FindResource(type == 2 ? "grdIconView" : "grdTileView") as DataTemplate;
                if (selectedFile != null)
                {
                    (FilesContent as ListBox).SelectedItem = selectedFile;
                    (FilesContent as ListBox).ScrollIntoView(selectedFile);
                }
            }
            else if (type == 4)
            {
                FilesContent = FindResource("treeFiles") as TreeView;
            }
            if (Files != null)
            {
                //RealtimeRefresh(Files.Take(100));
            }
            Configs.LastViewType = CurrentViewType;
            ViewTypeChanged?.Invoke(this, new EventArgs());
        }

        public void SelectFileByDir(string dir)
        {
            UIFile file = null;
            switch (CurrentViewType)
            {
                case 1:
                    file = Files.FirstOrDefault(p => p.File.Dir == dir);
                    break;
                case 2:
                case 3:
                    file = Files.FirstOrDefault(p => p.File.Dir == dir);
                    break;
                default:
                    break;
            }
            SelectFile(file);
        }
        public void SelectFile(UIFile file)
        {
            if (FilesContent is ListView lvw)
            {
                lvw.SelectedItem = file;
                lvw.ScrollIntoView(file);
            }
        }
        public int CurrentViewType { get; private set; } = 1;
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void Tags_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Class c = (e.Source as ContentPresenter).Content as Class;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ClickTag?.Invoke(this, new ClickTagEventArgs(c));
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                TagGroup tg = sender as TagGroup;

                await RemoveFilesFromClass(new File[] { tg.File.File }, c);
                tg.File.Classes.Remove(c);
            }
            e.Handled = true;
        }
        public event EventHandler<ClickTagEventArgs> ClickTag;

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

            ContextMenu menu = FindResource("menu") as ContextMenu;
            menu.Items.Clear();
            var files = GetSelectedFiles();
            if (files.Count == 1)
            {
                MenuItem menuOpenFolder = new MenuItem() { Header = "打开目录" };
                menuOpenFolder.Click += OpenDirMernuItem_Click;
                menu.Items.Add(menuOpenFolder);
            }
            if ((!files.Any(p => p.File.IsFolder) || CurrentViewType<4) && Project.Classes!=null)
            {
                menu.Items.Add(new Separator());
                foreach (var tag in Project.Classes)
                {
                    CheckBox chk = new CheckBox()
                    {
                        Content = tag.Name,
                        IsChecked = (!files.Any(p => p.Classes.Any(q=>q.ID==tag.ID))) ? false :
                        files.All(p => p.Classes.Any(q => q.ID == tag.ID)) ? true : (bool?)null
                    };
                    chk.Click += async (p1, p2) =>
                     {
                         GetProgress().Show(false);
                         if (chk.IsChecked == true)
                         {
                             await AddFilesToClassAsync(files.Select(p => p.File), tag);
                             foreach (var file in files)
                             {
                                 var newC = file.Classes.FirstOrDefault(p => p.ID == tag.ID);
                                 if (newC==null)
                                 {
                                     file.Classes.Add(tag);
                                 }
                             }
                         }
                         else
                         {
                             await RemoveFilesFromClass(files.Select(p => p.File), tag);
                             foreach (var file in files)
                             {
                                 var c = file.Classes.FirstOrDefault(p => p.ID == tag.ID);
                                 if (c!=null)
                                 {
                                     file.Classes.Remove(c);
                                 }
                             }
                         }

                         GetProgress().Close();
                     };
                    menu.Items.Add(chk);
                }
            }

        }

        private void SearchTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string txt = sender.Text.ToLower();
                var suggestions = Files == null ? new List<UIFile>() : Files.Where(p => p.File.Name.ToLower().Contains(txt)).ToList();

                sender.ItemsSource = suggestions.Count > 0 ?
                    suggestions : new string[] { "结果为空" } as IEnumerable;
            }
        }

        private void SearchTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            UIFile file = args.ChosenSuggestion as UIFile;
            SelectFile(file);
        }

        private void SearchTextBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            //(sender as ContextMenu).Items.Clear();
        }


    }

    public class ClickTagEventArgs : EventArgs
    {
        public ClickTagEventArgs(Class c)
        {
            Class = c;
        }

        public Class Class { get; }
    }

    public class DragDropFilesHelper
    {
        private ListBox list;
        private bool mouseDown = false;
        private bool set = false;
        private Point beginPosition = default;

        public DragDropFilesHelper(ListBox list)
        {
            this.list = list;
        }
        public void Regist()
        {
            list.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            list.PreviewMouseLeftButtonUp += List_PreviewMouseLeftButtonUp;
            list.PreviewMouseMove += List_PreviewMouseMove;
        }

        private void List_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(!(e.OriginalSource is Image))
            {
                return;
            }
            Point position = e.GetPosition(null);
            double distance = Math.Sqrt(Math.Pow(position.X - beginPosition.X, 2) + Math.Pow(position.Y - beginPosition.Y, 2));
            //如果还没有放置项，并且鼠标已经按下，并且移动距离超过了10单位
            if (!set && mouseDown && distance > 10)
            {
                set = true;
                var files = list.SelectedItems.Cast<UIFile>().Select(p => p.File.GetAbsolutePath()).ToArray();
                if (files.Length == 0)
                {
                    return;
                }
                var data = new DataObject(DataFormats.FileDrop, files);
                //放置一个特殊类型，这样好让自己的程序识别，防止自己拖放到自己身上
                data.SetData(nameof(ClassifyFiles), "");
                //实测支持复制和移动，不知道为什么不支持快捷方式
                DragDrop.DoDragDrop(sender as DependencyObject, data, DragDropEffects.All);
            }
        }

        private void List_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            set = false;
            if (ignoredSelect)
            {
                //当我们发现用户并不是真的要拖放，而是真的想选中某一个项时，
                //就把该项单独选中
                ignoredSelect = false;
                var mouseOverItem = list.SelectedItems.Cast<object>().FirstOrDefault(p =>
           (list.ItemContainerGenerator.ContainerFromItem(p) as ListBoxItem).IsMouseOver);
                if (mouseOverItem != null)
                {
                    list.SelectedItem = mouseOverItem;
                }
            }
        }

        bool ignoredSelect = false;
        private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            beginPosition = e.GetPosition(null);
            //当鼠标点击列表项时，如果鼠标位置在已经被选中的项的上方，那么取消响应
            //这是由于ListView总是在拖放之前就把多选变成了单选，与拖放需求不符
            if (e.ClickCount > 1)
            {
                return;
            }
            //判断鼠标是否在已经选中的项的上方
            var hasMouseOver = list.SelectedItems.Cast<object>().Any(p =>
            {
                if (list.ItemContainerGenerator.ContainerFromItem(p) is ListBoxItem item)
                {
                    return item.IsMouseOver;
                }
                return false;
            });
            if (hasMouseOver)
            {
                ignoredSelect = true;
                e.Handled = true;
            }
        }
    }
}
