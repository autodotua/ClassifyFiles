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

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// FIlesViewer.xaml 的交互逻辑
    /// </summary>
    public partial class FilesViewer : UserControl, INotifyPropertyChanged
    {
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
        }
        private ObservableCollection<UIFile> files;
        public ObservableCollection<UIFile> Files
        {
            get => files;
            set
            {
                files = value;
                page = 0;
                this.Notify(nameof(Files), nameof(PagingFiles), nameof(FileTree));
            }
        }
        /// <summary>
        /// 供树状图使用的文件树
        /// </summary>
        public List<UIFile> FileTree => Files == null ? null : new List<UIFile>(FileUtility.GetFileTree(Files).SubFiles.Cast<UIFile>());

        /// <summary>
        /// 供
        /// </summary>
        public IEnumerable<UIFile> PagingFiles
        {
            get
            {
                var files = Files == null ? null : Files.Skip(pagingItemsCount * page).Take(pagingItemsCount);
                RealtimeRefresh(files);
                return files;
            }
        }
        private int page;
        public int Page
        {
            get => page;
            set
            {
                page = value;
                this.Notify(nameof(Page), nameof(PagingFiles));
            }
        }
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

        public async Task SetFilesAsync(IEnumerable<File> files, bool tags = true)
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
                       //await uiFile.LoadTagsAsync(Project);
                       filesWithIcon.Add(uiFile);
                   }
               });
                Files = new ObservableCollection<UIFile>(filesWithIcon);
            }
            GeneratePaggingButtons();
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
            }
            this.Notify(nameof(Files));

            GeneratePaggingButtons();
        }



        private void GeneratePaggingButtons()
        {
            stkPagging.Children.Clear();
            if (Files != null && Files.Count > 0)
            {
                for (int i = 1; i <= Math.Ceiling((double)Files.Count / pagingItemsCount); i++)
                {
                    Button btn = new Button()
                    {
                        Content = i,
                    };
                    btn.Click += (p1, p2) =>
                    {
                        stkPagging.Children.Cast<Button>()
                        .ForEach(p => p.Background = Brushes.Transparent);
                        (p1 as Button).SetResourceReference(Button.BackgroundProperty, "SystemAccentColorLight3Brush");
                        Page = (int)btn.Content - 1;
                    };
                    stkPagging.Children.Add(btn);
                }
               (stkPagging.Children[0] as Button).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
        private UIFile GetSelectedFile()
        {
            return CurrentViewType switch
            {
                1 => lvwFiles.SelectedItem as UIFile,
                2 => lbxGrdFiles.SelectedItem as UIFile,
                3 => treeFiles.SelectedItem as UIFile,
                _ => throw new NotImplementedException()
            };
        }
        private IReadOnlyList<UIFile> GetSelectedFiles()
        {
            return CurrentViewType switch
            {
                1 => lvwFiles.SelectedItems.Cast<UIFile>().ToList().AsReadOnly(),
                2 => lbxGrdFiles.SelectedItems.Cast<UIFile>().ToList().AsReadOnly(),
                3 => new List<UIFile>().AsReadOnly(),
                _ => throw new NotImplementedException()
            };
        }

        private async void lvwFiles_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                File file = GetSelectedFile();
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

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            scrPagging.ScrollToHorizontalOffset(scrPagging.HorizontalOffset - e.Delta / 20);
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                UIFile.DefualtIconSize += e.Delta / 30;
                Files.ForEach(p => p.UpdateIconSize());
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
                    Arguments = $"/select, \"{GetSelectedFile().GetAbsolutePath(false)}\"",
                    UseShellExecute = true
                };
                p.Start();
            }
        }
        public event EventHandler ViewTypeChanged;
        private void ViewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            int i = int.Parse((sender as FrameworkElement).Tag as string);
            CurrentViewType = i;
            grdAppBar.Children.OfType<AppBarToggleButton>().ForEach(p => p.IsChecked = false);
            (sender as AppBarToggleButton).IsChecked = true;
            lvwFilesArea.Visibility = i == 1 ? Visibility.Visible : Visibility.Collapsed;
            grdFilesArea.Visibility = i == 2 ? Visibility.Visible : Visibility.Collapsed;
            treeFiles.Visibility = i == 3 ? Visibility.Visible : Visibility.Collapsed;
            ViewTypeChanged?.Invoke(this, new EventArgs());
        }

        public void SelectFileByDir(string dir)
        {
            UIFile file = null;
            switch (CurrentViewType)
            {
                case 1:
                    file = Files.FirstOrDefault(p => p.Dir == dir);
                    break;
                case 2:
                    file = Files.FirstOrDefault(p => p.Dir == dir);
                    break;
                case 3:
                    break;
                default:
                    break;
            }
            SelectFile(file);
        }
        public void SelectFile(UIFile file)
        {
            switch (CurrentViewType)
            {
                case 1:
                    lvwFiles.SelectedItem = file;
                    lvwFiles.ScrollIntoView(file);
                    break;
                case 2:
                    int index = Files.IndexOf(file);
                    int page = (int)Math.Ceiling((double)index / pagingItemsCount);
                    stkPagging.Children.Cast<Button>().FirstOrDefault(p => (int)p.Content == page)?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    lbxGrdFiles.SelectedItem = file;
                    lbxGrdFiles.ScrollIntoView(file);  //无效
                    break;
                case 3:
                    break;
                default:
                    break;
            }

        }
        public int CurrentViewType { get; private set; } = 1;
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvwFiles == null)
            {
                return;
            }
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

                await RemoveFilesFromClass(new File[] { tg.File.Raw }, c);
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
            foreach (var tag in Project.Classes)
            {
                CheckBox chk = new CheckBox()
                {
                    Content = tag.Name,
                    IsChecked = (!files.Any(p => p.Classes.Contains(tag))) ? false :
                    files.All(p => p.Classes.Contains(tag)) ? true : (bool?)null
                };
                chk.Click += async (p1, p2) =>
                 {
                     GetProgress().Show(false);
                     if (chk.IsChecked == true)
                     {
                         await AddFilesToClassAsync(files.Select(p => p.Raw), tag);
                         foreach (var file in files)
                         {
                             if (!file.Classes.Contains(tag))
                             {
                                 file.Classes.Add(tag);
                             }
                         }
                     }
                     else
                     {
                         await RemoveFilesFromClass(files.Select(p => p.Raw), tag);
                         foreach (var file in files)
                         {
                             if (file.Classes.Contains(tag))
                             {
                                 file.Classes.Remove(tag);
                             }
                         }
                     }

                     GetProgress().Close();
                 };
                menu.Items.Add(chk);
            }

        }

        private void SearchTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string txt = sender.Text.ToLower();
                var suggestions = Files == null ? new List<UIFile>() : Files.Where(p => p.Name.ToLower().Contains(txt)).ToList();

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

        #region 自动生成缩略图
        private  ConcurrentDictionary<int, UIFile> generated = new ConcurrentDictionary<int, UIFile>();
        private void lvwFiles_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = (sender as System.Windows.Controls.ListView).GetVisualChild<ScrollViewer>(); //Extension method

            if (scrollViewer != null)
            {
                if (scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) is ScrollBar scrollBar)
                {
                    scrollBar.ValueChanged += async delegate
                     {
                         await RealtimeRefresh(Files.Skip((int)scrollViewer.VerticalOffset).Take((int)scrollViewer.ViewportHeight + 5));
                     };
                }
            }

        }

        private Task RealtimeRefresh(IEnumerable<UIFile> files)
        {

            return Task.Run(async () =>
            {
                if (files == null || !files.Any())
                {
                    return;
                }
                AppDbContext db = new AppDbContext(DbUtility.DbPath);
                foreach (var file in files)
                {
                    await file.LoadTagsAsync(db);
                }
                if (Configs.AutoThumbnails)
                {
                    Parallel.ForEach(files, file =>
                    {
                        if (!generated.ContainsKey(file.ID) && file.Thumbnail == null)
                        {
                            generated.TryAdd(file.ID, file);
                            FileUtility.TryGenerateThumbnail(file);
                            file.Raw.Thumbnail = file.Thumbnail;
                        }
                    });

                    try
                    {
                        await FileUtility.SaveFilesAsync(files.Where(p => p.Thumbnail != null).Select(p => p.Raw));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

        private async void TreeItems_Expanded(object sender, RoutedEventArgs e)
        {
            var files = ((e.OriginalSource as TreeViewItem).DataContext as UIFile)
                .SubFiles.Where(p => !p.IsFolder).Cast<UIFile>();
            await RealtimeRefresh(files);
        }

        #endregion
    }

    public class ClickTagEventArgs : EventArgs
    {
        public ClickTagEventArgs(Class c)
        {
            Class = c;
        }

        public Class Class { get; }
    }
}
