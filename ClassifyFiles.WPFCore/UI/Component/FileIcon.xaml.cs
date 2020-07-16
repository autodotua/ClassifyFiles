using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using ClassifyFiles.Util;
using FzLib.Extension;
using ModernWpf.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClassifyFiles.UI.Component
{
    /// <summary>
    /// FileIcon.xaml 的交互逻辑
    /// </summary>
    public partial class FileIcon : UserControlBase
    {
        public FileIcon()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(UIFile), typeof(FileIcon), new PropertyMetadata(OnFileChanged));
        static void OnFileChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public UIFile File
        {
            get => GetValue(FileProperty) as UIFile; //file;
            set
            {
                SetValue(FileProperty, value);
            }
        }
        public static readonly DependencyProperty UseLargeIconProperty =
            DependencyProperty.Register("UseLargeIcon", typeof(bool), typeof(FileIcon));

        public bool UseLargeIcon
        {
            get => (bool)GetValue(UseLargeIconProperty); //UseLargeIcon;
            set => SetValue(UseLargeIconProperty, value);
        }

        public static ConcurrentDictionary<int, FrameworkElement> Caches { get; } = new ConcurrentDictionary<int, FrameworkElement>();
        private bool Load()
        {

            FrameworkElement item;
            if (File.File.IsFolder == false
            && Caches.ContainsKey(File.File.ID)
            && !(File.Display.Image != null && Caches[File.File.ID] is FontIcon))
            {
                item = Caches[File.File.ID];
            }
            else
            {
                if (File.Display.Image == null)
                {
                    if (main.Content is Image)
                    {
                        return true;
                    }
                    item = new FontIcon() { Glyph = File.Display.Glyph };
                }
                else
                {
                    item = new Image()
                    {
                        Source = File.Display.Image,
                        Stretch = Stretch.UniformToFill
                    };
                }
                item.HorizontalAlignment = HorizontalAlignment.Center;
                item.VerticalAlignment = VerticalAlignment.Center;

                if (File.File.IsFolder == false)
                {
                    Caches.TryAdd(File.File.ID, item);
                }
            }

            main.Content = item;
            return item is Image;
        }
        private async void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (UseLargeIcon)
            {
                main.SetBinding(WidthProperty, "File.Size.LargeIconSize");
                main.SetBinding(HeightProperty, "File.Size.LargeIconSize");
                main.SetBinding(FontIcon.FontSizeProperty, "File.Size.LargeFontIconSize");
            }
            else
            {
                main.SetBinding(WidthProperty, "File.Size.SmallIconSize");
                main.SetBinding(HeightProperty, "File.Size.SmallIconSize");
                main.SetBinding(FontIcon.FontSizeProperty, "File.Size.SmallFontIconSize");
            }
            await File.LoadAsync();
            if (!Load())
            {
                if (Configs.AutoThumbnails)
                {
                    tasks.Enqueue(RefreshIcon);
                }
            }
        }
        private static TaskQueue tasks = new TaskQueue();

        private static ConcurrentDictionary<int, UIFile> generatedThumbnails = new ConcurrentDictionary<int, UIFile>();
        private static ConcurrentDictionary<int, UIFile> generatedIcons = new ConcurrentDictionary<int, UIFile>();


        private async Task<bool> RefreshIcon()
        {
            UIFile file = null;
            Dispatcher.Invoke(() => file = File);
            bool result = false;
            await Task.Run(() =>
           {
               if (Configs.ShowThumbnail)
               {
                   if (!generatedThumbnails.ContainsKey(file.File.ID) && string.IsNullOrEmpty(file.File.ThumbnailGUID) /* file.File.ThumbnailGUID == null && file.File.ThumbnailGUID != ""*/)
                   {
                       generatedThumbnails.TryAdd(file.File.ID, file);
                       if (FileUtility.TryGenerateThumbnail(file.File))
                       {
                           result = true;
                           DbUtility.SetObjectModified(file.File);
                       }
                   }
               }
               if (Configs.ShowExplorerIcon && (!Configs.ShowThumbnail || Configs.ShowThumbnail && string.IsNullOrEmpty(file.File.ThumbnailGUID)))
               {
                   if (!generatedIcons.ContainsKey(file.File.ID) && file.File.IconGUID == null && file.File.IconGUID != "")
                   {
                       generatedIcons.TryAdd(file.File.ID, file);
                       if (FileUtility.TryGenerateIcon(file.File))
                       {
                           result = true;
                           DbUtility.SetObjectModified(file.File);
                       }
                   }
               }
           });
            Dispatcher.Invoke(() =>
            {
                Load();
            });
            return result;

        }
    }
    public class TaskQueue
    {
        ConcurrentQueue<Func<Task>> tasks = new ConcurrentQueue<Func<Task>>();
        bool isExcuting = false;

        public async void Enqueue(Func<Task> t)
        {
            tasks.Enqueue(t);
            if (!isExcuting)
            {
                isExcuting = true;
                await t();
                while (tasks.Count > 0)
                {
                    Debug.WriteLine("Task count is " + tasks.Count);
                    var currentTasks = tasks.ToArray();
                    tasks.Clear();
                    await Task.Run(() =>
                    {
                        ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = Configs.RefreshThreadCount };
                        Parallel.ForEach(currentTasks, opt, t2 =>
                        {
                            try
                            {
                                t2().Wait();
                            }
                            catch(Exception ex)
                            {

                            }
                        });
                    });
                    try
                    {
                        await DbUtility.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {

                    }
                    //Debug.WriteLine("Task count is " + tasks.Count);
                    //List<Task> tempTasks = new List<Task>(Configs.RefreshThreadCount);
                    //for (int i = 0; i < Configs.RefreshThreadCount && tasks.Count > 0; i++)
                    //{
                    //    if (tasks.TryDequeue(out Func<Task> t2))
                    //    {
                    //        tempTasks.Add(t2());
                    //        //await t2();
                    //    }
                    //}
                    //try
                    //{
                    //    await Task.WhenAll(tempTasks);
                    //}
                    //catch (Exception ex)
                    //{
                    //    try
                    //    {
                    //        foreach (var t2 in tempTasks)
                    //        {
                    //            await t2;
                    //        }
                    //    }
                    //    catch
                    //    {

                    //    }
                    //}
                }
                isExcuting = false;

            }
            else
            {
                tasks.Enqueue(t);
            }
        }
    }

}
