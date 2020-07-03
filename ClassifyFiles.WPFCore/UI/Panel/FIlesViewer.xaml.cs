using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using System;
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                this.Notify(nameof(Files), nameof(PagingFiles), nameof(FileTree), nameof(Dirs));
            }
        }
        /// <summary>
        /// 供树状图使用的文件树
        /// </summary>
        public List<UIFile> FileTree => Files == null ? null : new List<UIFile>(FileUtility.GetFileTree(Files).SubFiles.Cast<UIFile>());
        public HashSet<string> Dirs
        {
            get
            {
                if (Files == null)
                {
                    return null;
                }
                HashSet<string> set = new HashSet<string>();
                foreach (var file in Files)
                {
                    set.Add(file.Dir);
                }
                return set;
            }
        }
        /// <summary>
        /// 供
        /// </summary>
        public IEnumerable<UIFile> PagingFiles
        {
            get
            {
                var files = Files == null ? null : Files.Skip(pagingItemsCount * page).Take(pagingItemsCount);
                if (files != null)
                {
                }
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

        public void SetFiles(IEnumerable<File> files, bool tags = true)
        {
            IEnumerable<UIFile> filesWithIcon = files.Select(p => new UIFile(p, tags, Project));
            var orderedFiles = filesWithIcon.OrderBy(p => p.Dir).ThenBy(p => p.Name);
            Files = new ObservableCollection<UIFile>(orderedFiles);

        }
        public void AddFiles(IEnumerable<File> files, bool tags = true)
        {
            IEnumerable<UIFile> filesWithIcon = files.Select(p => new UIFile(p, tags, Project));
            foreach (var file in filesWithIcon)
            {
                Files.Add(file);
            }

        }



        public void GeneratePaggingButtons()
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

        private async void lvwFiles_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                File file = GetSelectedFile();
                if (file != null)
                {
                    if (file.Dir == null)//是目录
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
                    if (file != null)
                    {
                        lvwFiles.SelectedItem = file;
                        lvwFiles.ScrollIntoView(file);
                    }
                    break;
                case 2:
                    file = Files.FirstOrDefault(p => p.Dir == dir);
                    if (file != null)
                    {
                        int index = Files.IndexOf(file);
                        int page = (int)Math.Ceiling((double)index / pagingItemsCount);
                        stkPagging.Children.Cast<Button>().FirstOrDefault(p => (int)p.Content == page)?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        lbxGrdFiles.SelectedItem = file;
                        lbxGrdFiles.ScrollIntoView(file);  //无效
                    }
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

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Class c = (e.Source as ContentPresenter).Content as Class;
            ClickTag?.Invoke(this, new ClickTagEventArgs(c));
        }
        public event EventHandler<ClickTagEventArgs> ClickTag;

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = FindResource("menu") as ContextMenu;
            UIFile file = GetSelectedFile();
            while (menu.Items.Count > 1)
            {
                menu.Items.RemoveAt(menu.Items.Count - 1);
            }
            foreach (var tag in Project.Classes)
            {
                CheckBox chk = new CheckBox()
                {
                    Content = tag.Name,
                    IsChecked = file.Classes.Contains(tag)
                };
                chk.Click += (p1, p2) =>
                {

                };
                menu.Items.Add(chk);
            }

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
}
