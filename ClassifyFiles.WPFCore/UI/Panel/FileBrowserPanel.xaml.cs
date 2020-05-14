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
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using DImg = System.Drawing.Image;

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
                this.Notify(nameof(Files), nameof(PagingFiles), nameof(FileTree));
            }
        }
        /// <summary>
        /// 供树状图使用的文件树
        /// </summary>
        public List<File> FileTree => Files == null ? null : new List<File>(new FileWithIcon(FileUtility.GetFileTree(Files)).SubFiles);
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
                    //Task.Run(() =>
                    //{
                    //    string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ClassifyFiles");
                    //    System.IO.Directory.CreateDirectory(tempDir);
                    //    foreach (var file in files)
                    //    {
                    //        string path = (file as File).GetAbsolutePath(Project.RootPath);
                    //        try
                    //        {
                    //            DImg image = DImg.FromFile(path);
                    //            DImg thumb = image.GetThumbnailImage(240, 240, () => false, IntPtr.Zero);
                    //            string thumbPath = System.IO.Path.Combine(tempDir, Guid.NewGuid().ToString("N")+".jpg");
                    //            thumb.Save(thumbPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    //            //BitmapImage需要使用UI线程进行操纵
                    //            Dispatcher.InvokeAsync(() =>
                    //            {
                    //                BitmapImage bmImg = new BitmapImage();
                    //                bmImg.BeginInit();
                    //                bmImg.UriSource = new Uri(thumbPath);
                    //                bmImg.EndInit();
                    //                file.Image = bmImg;
                    //            });
                    //        }
                    //        catch (Exception ex)
                    //        {

                    //        }
                    //    }
                    //});
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
            Class c = new Class()
            {
                MatchConditions = new List<MatchCondition>()
                {
                    new MatchCondition(){Type=MatchType.InFileName,Value="航拍"},
                    new MatchCondition(){ConnectionLogic=Logic.Or, Type=MatchType.InDirName,Value="航拍"}
                }
            };
            var classes = new List<Class>() { c };


            //Files = new ObservableCollection<FileWithIcon>(result.First().Value.Select(p => new FileWithIcon(p)));
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
        }

        private async void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            loading.Show();
            loading.SetMessage("正在枚举文件");
            var classFiles = await FileUtility.GetFilesAsync(new System.IO.DirectoryInfo(Project.RootPath), GetClassesPanel().Classes, true);

            //loading.SetMessage("正在生成缩略图");
            //HashSet<File> files = new HashSet<File>();
            //classFiles.ForEach(p => (p.Value as List<File>).ForEach(q => files.Add(q)));
            //await FileUtility.GenerateThumbsAsync(files, Project.RootPath, "thumbs");
            loading.SetMessage("正在保存");
            await DbUtility.UpdateFilesAsync(classFiles);
            if (classes.SelectedClass != null && classFiles.ContainsKey(classes.SelectedClass))
            {
                Files = new ObservableCollection<FileWithIcon>(classFiles[classes.SelectedClass].Select(p => new FileWithIcon(p)));

            }
            loading.Close();
        }

        private async void classes_SelectedClassChanged(object sender, SelectedItemChanged<Class> e)
        {
            if (e.NewValue != null)
            {
                var files = await DbUtility.GetFilesAsync(e.NewValue);

                Files = new ObservableCollection<FileWithIcon>(files.Select(p => new FileWithIcon(p)));

                stkPagging.Children.Clear();
                GeneratePaggingButtons();

                //var fileTree = FileUtility.GetFileTree(Files);
            }
        }

        private void GeneratePaggingButtons()
        {
            if (Files.Count > 0)
            {
                for (int i = 1; i <= Math.Ceiling((double)Files.Count / pagingItemsCount); i++)
                {
                    Button btn = new Button()
                    {
                        Style = FindResource("MaterialDesignFlatButton") as Style,
                        Content = i,
                    };
                    btn.Click += (p1, p2) =>
                    {
                        stkPagging.Children.Cast<Button>()
                        .ForEach(p => p.Style = FindResource("MaterialDesignFlatButton") as Style);
                        (p1 as Button).Style = FindResource("MaterialDesignOutlinedButton") as Style;
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
            int i = lbxDisplayMode.SelectedIndex;
            lvwFiles.Visibility = i == 0 ? Visibility.Visible : Visibility.Collapsed;
            lbxGrdFiles.Visibility = i == 1 ? Visibility.Visible : Visibility.Collapsed;
            treeFiles.Visibility = i == 2 ? Visibility.Visible : Visibility.Collapsed;

        }

        private async void lvwFiles_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                File file = null;
                if (sender is ListBox lbx)
                {
                    file = lbx.SelectedItem as File;
                }
                else if (sender is TreeView tree)
                {
                    file = tree.SelectedItem as File;
                }

                if (file != null)
                {
                    if(file.Dir=="")//是目录
                    {
                        return;
                    }
                    string path = file.GetAbsolutePath(Project.RootPath);

                    if (!System.IO.File.Exists(path))
                    {
                        (Window.GetWindow(this) as MainWindow).snack.Message.Content = "文件不存在";
                        (Window.GetWindow(this) as MainWindow).snack.IsActive = true;
                        await Task.Delay(3000);
                        (Window.GetWindow(this) as MainWindow).snack.IsActive = false;
                        e.Handled = true;
                        return;
                    }
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = true
                    };
                    p.Start();

                    e.Handled = true;
                }
            }
            catch
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
    }


    public class FileWithIcon : File
    {
        public PackIconKind Kind { get; set; } = PackIconKind.File;
        public BitmapImage Image
        {
            get
            {
                if (Thumbnail == null)
                {
                    return null;
                }
                return ToImage(Thumbnail);
                //BitmapImage bmImg = new BitmapImage();
                //bmImg.BeginInit();
                //bmImg.UriSource = new Uri("thumbs/" + ImageID + ".jpg",UriKind.Relative);
                //bmImg.EndInit();
                //return bmImg;
            }
        }
        private BitmapImage ToImage(byte[] array)
        {
            using var ms = new System.IO.MemoryStream(array);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // here
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
        //private BitmapImage image;
        //public BitmapImage Image
        //{
        //    get => image;
        //    set
        //    {
        //        image = value;
        //        this.Notify(nameof(Image), nameof(IconVisibility), nameof(ImageVisibility));
        //    }
        //}
        public Visibility IconVisibility => Image == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ImageVisibility => Image == null ? Visibility.Collapsed : Visibility.Visible;
        //public Control IconControl
        //{
        //    get
        //    {
        //        if(Image==null)
        //        {
        //            return new PackIcon() { Kind = PackIconKind.File ,Width=32,Height=32};
        //        }
        //        else
        //        {
        //            return new Grid() { Children = { new Image() { Source = Image, Width = 32, Height = 32 } } };
        //        }
        //    }
        //}
        private static double defualtIconSize = 32;
        public static double DefualtIconSize
        {
            get => defualtIconSize;
            set
            {
                if (value < 16 || value > 144)
                {
                    return;
                }
                defualtIconSize = value;
            }
        }

        public double LargeIconSize { get; private set; } = DefualtIconSize;
        public double SmallIconSize { get; private set; } = DefualtIconSize / 2;

        public void UpdateIconSize()
        {
            LargeIconSize = DefualtIconSize;
            SmallIconSize = DefualtIconSize / 2;
            this.Notify(nameof(LargeIconSize), nameof(SmallIconSize));
        }
        //public double IconHight => 48;
        //public double IconWidth => 48;
        public FileWithIcon() { }

        public FileWithIcon(File file)
        {
            Name = file.Name;
            Dir = file.Dir;
            SubFiles = file.SubFiles.Select(p => new FileWithIcon(p)).Cast<File>().ToList();
            Thumbnail = file.Thumbnail;
        }

    }
}
