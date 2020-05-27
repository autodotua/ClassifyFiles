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
    public partial class FileBrowserPanel : ProjectPanelBase
    {
        private ObservableCollection<FileWithIcon> files;
        public ObservableCollection<FileWithIcon> Files
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
        public List<File> FileTree => Files == null ? null : new List<File>(new FileWithIcon(FileUtility.GetFileTree(Files)).SubFiles);
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
        public IEnumerable<FileWithIcon> PagingFiles
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
        public FileBrowserPanel()
        {
            InitializeComponent();
        }

        public override ClassesPanel GetClassesPanel()
        {
            return classes;
        }

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
        }
        private ProgressDialog GetProgress()
        {
            return (Window.GetWindow(this) as MainWindow).Progress;
        }
        private async void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            GetProgress().Show();
            var classFiles = await FileUtility.GetFilesAsync(new System.IO.DirectoryInfo(Project.RootPath), GetClassesPanel().Classes, true, p =>
            {

            });
            await DbUtility.UpdateFilesAsync(classFiles);
            if (classes.SelectedClass != null && classFiles.ContainsKey(classes.SelectedClass))
            {
                SetFiles(classFiles[classes.SelectedClass]);
            }
            GetProgress().Close();
            GeneratePaggingButtons();
        }

        private void SetFiles(IEnumerable<File> files)
        {
            IEnumerable<FileWithIcon> filesWithIcon = files.Select(p => new FileWithIcon(p));
            var orderedFiles = filesWithIcon.OrderBy(p => p.Dir).ThenBy(p => p.Name);
            Files = new ObservableCollection<FileWithIcon>(orderedFiles);

        }

        private async void classes_SelectedClassChanged(object sender, SelectedItemChanged<Class> e)
        {
            if (e.NewValue != null)
            {
                var files = await DbUtility.GetFilesAsync(e.NewValue);
                SetFiles(files);

                GeneratePaggingButtons();
            }
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
                        .ForEach(p => p.Background=Brushes.Transparent);
                        (p1 as Button).SetResourceReference(Button.BackgroundProperty, "SystemAccentColorLight3Brush");
                        Page = (int)btn.Content - 1;
                    };
                    stkPagging.Children.Add(btn);
                }
               (stkPagging.Children[0] as Button).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvwFiles == null)
            {
                return;
            }
        }

        private File GetSelectedFile()
        {
            return CurrentViewType switch
            {
                1 => lvwFiles.SelectedItem as File,
                2 => lbxGrdFiles.SelectedItem as File,
                3 => treeFiles.SelectedItem as File,
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
                    if (file.Dir == "")//是目录
                    {
                        return;
                    }
                    string path = file.GetAbsolutePath(Project.RootPath);

                    if (!System.IO.File.Exists(path))
                    {
                        await new MessageDialog().ShowAsync("文件不存在","打开");
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
                FileWithIcon.DefualtIconSize += e.Delta / 30;
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
                    Arguments = $"/select, \"{GetSelectedFile().GetAbsolutePath(Project.RootPath, false)}\"",
                    UseShellExecute = true
                };
                p.Start();
            }
        }

        private void JumpToDirComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = e.AddedItems.Count == 0 ? null : e.AddedItems.Cast<string>().First();
            if (dir != null)
            {
                FileWithIcon file;
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
                            lbxGrdFiles.ScrollIntoView( file);  //无效
                        }
                            break;
                    case 3:
                        break;
                    default:
                        break;
                }
             
                (sender as ListBox).SelectedItem = null;
                flyoutJumpToDir.Hide();// = false;
            }
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

        private void ViewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            int i = int.Parse((sender as FrameworkElement).Tag as string);
            CurrentViewType = i;
            grdAppBar.Children.OfType<AppBarToggleButton>().ForEach(p => p.IsChecked = false);
            (sender as AppBarToggleButton).IsChecked = true;
            lvwFilesArea.Visibility = i == 1 ? Visibility.Visible : Visibility.Collapsed;
            grdFilesArea.Visibility = i == 2 ? Visibility.Visible : Visibility.Collapsed;
            treeFiles.Visibility = i == 3 ? Visibility.Visible : Visibility.Collapsed;
            btnLocateByDir.IsEnabled = i != 3;
        }
        private int CurrentViewType { get; set; } = 1;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsOpen = true;
        }
    }


}
