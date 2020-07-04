using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClassifyFiles.UI.Panel
{
    public class ClassesPanel : ListPanelBase
    {
        public override async Task LoadAsync(Project project)
        {
            Project = project;
            var treeClasses = await DbUtility.GetClassesAsync(Project);
            Items = new ObservableCollection<Class>(treeClasses);
        }
        public async override Task RenameAsync(string newName)
        {
            SelectedItem.Name = newName;
            await DbUtility.SaveClassAsync(SelectedItem as Class);
            await LoadAsync(Project);
        }
        public async override Task DeleteAsync()
        {
            await DbUtility.DeleteClassAsync(SelectedItem  as Class);
            await LoadAsync(Project);
        }

        protected async override Task AddItemAsync()
        {
            await DbUtility.AddClassAsync(Project);
        }
    }


    public abstract class ListPanelBase : UserControlBase, INotifyPropertyChanged
    {
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
                var oldValue = selectedItem;
                selectedItem = value;
                this.Notify(nameof(SelectedItem));
                //if (value != null)
                //{
                    SelectedItemChanged?.Invoke(this, new SelectedItemChanged(oldValue, value));
                }    
        //}
        }
        public Project Project { get; protected set; }
        public abstract Task LoadAsync(Project project);

        public ListPanelBase()
        {
            DataContext = this;
            ListBox list = new ListBox();
            list.SetBinding(ListBox.ItemsSourceProperty, new Binding("Items"));
            list.SetBinding(ListBox.SelectedItemProperty, new Binding("SelectedItem"));
            list.DisplayMemberPath = "Name";
            list.SetResourceReference(BackgroundProperty, "SystemControlBackgroundAltHighBrush");
            Content = list;
        }

        public async Task AddAsync()
        {
            await AddItemAsync();
            await LoadAsync(Project);
        }
        protected abstract Task AddItemAsync();

        public async Task DeleteSelectedAsync()
        {
            if (SelectedItem == null)
            {
                await new MessageDialog().ShowAsync("请先选择一项", "错误");
            }
            else
            {
                await DeleteAsync();
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
                string value = await new InputDialog().ShowAsync("重命名", false, "请输入新的标题", "");
                if (!string.IsNullOrWhiteSpace(value))
                {
                    await RenameAsync(value);
                }
            }
        }

        public abstract Task RenameAsync(string newName);
        public abstract Task DeleteAsync();
        public event EventHandler<SelectedItemChanged> SelectedItemChanged;
    }

    public class SelectedItemChanged : EventArgs
    {
        public SelectedItemChanged(Class oldValue, Class newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public Class OldValue { get; set; }
        public Class NewValue { get; set; }
    }
}
