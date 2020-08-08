using System;
using System.Windows;
using ClassifyFiles.Data;
using FzLib.Extension;
using static ClassifyFiles.Util.ClassUtility;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using ClassifyFiles.UI.Event;
using System.Windows.Controls;
using ClassifyFiles.UI.Model;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using ClassifyFiles.UI.Util;

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// ClassesPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ClassesPanel : UserControlBase
    {
        public ClassesPanel()
        {
            InitializeComponent();
        }
        private bool isLoading = false;
        public async Task LoadAsync(Project project)
        {
            isLoading = true;
               Project = project;
            List<Class> classes = null;
            await Task.Run(() => classes = GetClasses(Project));
            UIClasses = new ObservableCollection<UIClass>(classes.Select(p => new UIClass(p)));
            foreach (var c in UIClasses)
            {
                await c.UpdatePropertiesAsync();
            }
            isLoading = false;
            if (UIClasses.Any(p => p.Class.ID == Configs.LastClassID))
            {
                SelectedUIClass = UIClasses.First(p => p.Class.ID == Configs.LastClassID);
            }
        }

        private ObservableCollection<UIClass> uIClasses;
        public ObservableCollection<UIClass> UIClasses
        {
            get => uIClasses;
            protected set
            {
                uIClasses = value;
                this.Notify(nameof(UIClasses));
            }
        }
        private UIClass selectedUIClass;
        public UIClass SelectedUIClass
        {
            get => selectedUIClass;
            set
            {
                if(isLoading)
                {
                    return;
                }
                if (selectedUIClass == value)
                {
                    return;
                }
                var oldValue = selectedUIClass;
                selectedUIClass = value;
                this.Notify(nameof(SelectedUIClass));
                if (value != null && IsLoaded)
                {
                    Task.Run(() =>
                    Configs.LastClassID = value.Class.ID);
                }

                SelectedClassChanged?.Invoke(this, new SelectedClassChangedEventArgs(oldValue?.Class, value?.Class));
            }
        }

        public bool CanReorder { get; set; } = false;

        public async Task UpdateUIClassesAsync()
        {
            foreach (var c in UIClasses)
            {
                await c.UpdatePropertiesAsync();
            }
        }
        public Project Project { get; protected set; }

        public async Task<Class> AddAsync()
        {
            var c = await Task.Run(() => AddClass(Project));
            UIClass uiC = new UIClass(c);
            UIClasses.Add(uiC);
            SelectedUIClass = uiC;
            return c;
        }

        public async Task DeleteSelectedAsync()
        {
            if (SelectedUIClass == null)
            {
                await new MessageDialog().ShowAsync("请先选择一项", "错误");
            }
            else
            {
                int index = UIClasses.IndexOf(SelectedUIClass);
                await Task.Run(() => DeleteClass(SelectedUIClass.Class));
                UIClasses.Remove(SelectedUIClass);
                if (UIClasses.Count > 0)
                {
                    SelectedUIClass = index == 0 ? UIClasses[0] : UIClasses[index - 1];
                }
            }
        }

        public event EventHandler<SelectedClassChangedEventArgs> SelectedClassChanged;

        private void btnAllFiles_Click(object sender, RoutedEventArgs e)
        {
            SelectedUIClass = null;
        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            UIClass c = item.DataContext as UIClass;
            if (e.Data.GetDataPresent(nameof(ClassifyFiles)))
            {
                var files = (UIFile[])e.Data.GetData(nameof(ClassifyFiles));
                ClassFilesDrop?.Invoke(sender, new ClassFilesDropEventArgs(c.Class, files));
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ClassFilesDrop?.Invoke(sender, new ClassFilesDropEventArgs(c.Class, files));
            }
            else
            {
                return;
            }

            //开始设置背景渐变动画
            //需要使用非冻结的颜色，因此需要Clone。而且很奇怪，不能加null判断
            item.Background = (FindResource("SystemControlBackgroundBaseLowBrush") as Brush).Clone();
            DoubleAnimation ani = new DoubleAnimation()
            {
                Duration = Configs.AnimationDuration,
                FillBehavior = FillBehavior.HoldEnd,
                To = 1,
                From = 0,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };

            item.Background.BeginAnimation(Brush.OpacityProperty, ani);
        }

        public event EventHandler<ClassFilesDropEventArgs> ClassFilesDrop;

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            SmoothScrollViewerHelper.Regist(lbx);
            if (CanReorder)
            {
                var lbxHelper = new ListBoxHelper<UIClass>(lbx);
                lbxHelper.EnableDragAndDropItem();
                lbxHelper.SingleItemDragDroped += (p1, p2) =>
                {
                    ClassDragDroped?.Invoke(p1, p2);
                };
            }
        }

        public event EventHandler ClassDragDroped;
    }


}
