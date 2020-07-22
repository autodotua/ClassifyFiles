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

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// ClassesPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ClassesPanel : UserControlBase
    {
        public ClassesPanel()
        {
            DataContext = this;
            InitializeComponent();
        }
        public async Task LoadAsync(Project project)
        {
            Project = project;
            var classes = await GetClassesAsync(Project);
            Items = new ObservableCollection<Class>(classes);
            if (Items.Any(p => p.ID == Configs.LastClassID))
            {
                SelectedItem = items.First(p => p.ID == Configs.LastClassID);
            }
        }

        private ObservableCollection<Class> items;
        public ObservableCollection<Class> Items
        {
            get => items;
            protected set
            {
                items = value;
                this.Notify(nameof(Items));
            }
        }
        private Class selectedItem;
        public Class SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value)
                {
                    return;
                }
                var oldValue = selectedItem;
                selectedItem = value;
                this.Notify(nameof(SelectedItem));
                if (value != null)
                {
                    Configs.LastClassID = value.ID;
                }
                SelectedClassChanged?.Invoke(this, new SelectedClassChangedEventArgs(oldValue, value));
            }
        }
        public Project Project { get; protected set; }

        public async Task<Class> AddAsync()
        {
            var c = await AddClassAsync(Project);
            Items.Add(c);
            SelectedItem = c;
            return c;
        }

        public async Task DeleteSelectedAsync()
        {
            if (SelectedItem == null)
            {
                await new MessageDialog().ShowAsync("请先选择一项", "错误");
            }
            else
            {
                int index = items.IndexOf(SelectedItem);
                await DeleteClassAsync(SelectedItem);
                Items.Remove(SelectedItem);
                if (Items.Count > 0)
                {
                    SelectedItem = index == 0 ? Items[0] : Items[index - 1];
                }
            }
        }

        public async Task RenameButton()
        {
            if (SelectedItem == null)
            {
                await new MessageDialog().ShowAsync("请先选择一项", "错误");
            }
            else
            {
                string value = await new InputDialog().ShowAsync("重命名", false, "请输入新的标题", SelectedItem.Name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    SelectedItem.Name = value;
                    await SaveClassAsync(SelectedItem);
                }
            }
        }

        public event EventHandler<SelectedClassChangedEventArgs> SelectedClassChanged;

        private void btnAllFiles_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            Class c = item.DataContext as Class;
            //后来想了下，不需要这一段，因为程序可以在类之间进行拖放
            if (e.Data.GetDataPresent(nameof(ClassifyFiles)))
            {
                var files = (UIFile[])e.Data.GetData(nameof(ClassifyFiles));
                ClassFilesDrop?.Invoke(sender, new ClassFilesDropEventArgs(c, files));
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ClassFilesDrop?.Invoke(sender, new ClassFilesDropEventArgs(c, files));
            }
            
            //开始设置背景渐变动画
            //需要使用非冻结的颜色，因此需要Clone。而且很奇怪，不能加null判断
            item.Background = (FindResource("SystemControlBackgroundBaseLowBrush") as Brush).Clone();
            DoubleAnimation ani = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.5),
                FillBehavior = FillBehavior.HoldEnd,
                To = 1,
                From=0,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(1)
            };
       
            item.Background.BeginAnimation(Brush.OpacityProperty, ani);
        }

        public event EventHandler<ClassFilesDropEventArgs> ClassFilesDrop;
    }


}
