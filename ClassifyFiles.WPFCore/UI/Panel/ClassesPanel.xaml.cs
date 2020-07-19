using System;
using System.Windows;
using ClassifyFiles.Data;
using FzLib.Extension;
using static ClassifyFiles.Util.ClassUtility;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using ClassifyFiles.UI.Event;

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
            if(Items.Any(p=>p.ID== Configs.LastClassID))
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
                if(value!=null)
                {
                    Configs.LastClassID = value.ID;
                }
                SelectedClassChanged?.Invoke(this, new SelectedClassChangedEventArgs(oldValue, value));
            }
        }
        public Project Project { get; protected set; }

        public async Task<Class> AddAsync()
        {
            var c= await AddClassAsync(Project);
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
                if(Items.Count>0)
                {
                        SelectedItem = index == 0? Items[0]:Items[index-1];
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
    }
}
