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
    public class ClassesPanel : ListPanelBase<Class>
    {
        public override async Task LoadAsync(Project project)
        {
            Project = project;
            //var (treeClasses, tile) = await DbUtility.GetTreeAndTileClassesAsync(Project);
            var treeClasses = await DbUtility.GetClassesAsync(Project);
            Items = new ObservableCollection<Class>(treeClasses);

            Class first = Items.FirstOrDefault();
            SelectedItem = first;
        }
        public async override Task RenameAsync(string newName)
        {
            SelectedItem.Name = newName;
            await DbUtility.SaveClassAsync(SelectedItem);
            await LoadAsync(Project);
        }
        public async override Task DeleteAsync()
        {
            await DbUtility.DeleteClassAsync(SelectedItem);
            await LoadAsync(Project);
        }
    }

    public class TagsPanel : ListPanelBase<Tag>
    {
        public override async Task LoadAsync(Project project)
        {
            Project = project;
            //var (treeClasses, tile) = await DbUtility.GetTreeAndTileClassesAsync(Project);
            var tags = await DbUtility.GetTagsAsync(Project);
            Items = new ObservableCollection<Tag>(tags);

            Tag first = Items.FirstOrDefault();
            SelectedItem = first;
        }
        public async override Task RenameAsync(string newName)
        {
            SelectedItem.Name = newName;
            await DbUtility.SaveTagAsync(SelectedItem);
            await LoadAsync(Project);
        }
        public async override Task DeleteAsync()
        {
            await DbUtility.DeleteTagAsync(SelectedItem);
            await LoadAsync(Project);
        }
    }

    public abstract class ListPanelBase<T> : UserControlBase, INotifyPropertyChanged where T : ClassifyItemModelBase
    {
        private ObservableCollection<T> items;
        public ObservableCollection<T> Items
        {
            get => items;
            protected set
            {
                items = value;
                this.Notify(nameof(Items));
            }
        }
        private T selectedItem;
        public T SelectedItem
        {
            get => selectedItem;
            set
            {
                var oldValue = selectedItem;
                selectedItem = value;
                this.Notify(nameof(SelectedItem));
                SelectedItemChanged?.Invoke(this, new SelectedItemChanged<T>(oldValue, value));
            }
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

        public async Task AddClass()
        {
            await DbUtility.AddClassAsync(Project);
            await LoadAsync(Project);
        }

        public async Task DeleteClass()
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
        public event EventHandler<SelectedItemChanged<T>> SelectedItemChanged;
    }

    public class SelectedItemChanged<T> : EventArgs
    {
        public SelectedItemChanged(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }
}
