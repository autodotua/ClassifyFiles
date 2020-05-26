using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// ClassesPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ClassesPanel : UserControlBase, INotifyPropertyChanged
    {
        private ObservableCollection<Class> classes;
        public ObservableCollection<Class> Classes
        {
            get => classes;
            private set
            {
                classes = value;
                this.Notify(nameof(Classes));
            }
        }
        //public bool editable;
        //public bool Editable
        //{
        //    get => editable;
        //    set
        //    {
        //        editable = value;
        //        this.Notify(nameof(Editable));
        //    }
        //}
        public Project Project { get; private set; }
        public async Task LoadAsync(Project project)
        {
            Project = project;
            //var (treeClasses, tile) = await DbUtility.GetTreeAndTileClassesAsync(Project);
            var treeClasses = await DbUtility.GetClassesAsync(Project);
            Classes = new ObservableCollection<Class>(treeClasses);

            Class first = Classes.FirstOrDefault();
            if (first != null)
            {
                while (first.Children?.Count > 0)
                {
                    first = first.Children[0];
                }
                await Task.Delay(200);
                var tvi = tree.ItemContainerGenerator.ContainerFromItem(Classes.First()) as TreeViewItem;
                if (tvi != null) tvi.IsSelected = true;
            }
        }

        public ClassesPanel()
        {
            DataContext = this;
            InitializeComponent();
        }
        public Class SelectedClass { get; private set; }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var oldValue = SelectedClass;
            SelectedClass = e.NewValue as Class;
            this.Notify(nameof(SelectedClass));
            SelectedClassChanged?.Invoke(this, new SelectedItemChanged<Class>(oldValue, SelectedClass));
        }



        public event EventHandler<SelectedItemChanged<Class>> SelectedClassChanged;


        public async Task AddClassAfter()
        {
            if (SelectedClass == null)
            {
                await DbUtility.AddClassAsync(Project, null, false);
                await LoadAsync(Project);
            }
            else
            {
                await DbUtility.AddClassAsync(Project, SelectedClass, false);
                await LoadAsync(Project);
            }

        }
        public async Task AddClassIn()
        {
            if (SelectedClass == null)
            {
                await DbUtility.AddClassAsync(Project, null, false);
                await LoadAsync(Project);
            }
            else
            {
                await DbUtility.AddClassAsync(Project, SelectedClass, true);

                await LoadAsync(Project);

            }
        }

        public async Task DeleteClass()
        {
            if (SelectedClass == null)
            {
                await new MessageDialog().ShowAsync("请先选择一项","错误");
            }
            else
            {
                await DbUtility.DeleteClassAsync(SelectedClass);
                await LoadAsync(Project);
            }
        }

        public async Task RenameButton()
        {
            if (SelectedClass == null)
            {
                await new MessageDialog().ShowAsync("请先选择一项", "错误");
            }
            else
            {
                string value = await new InputDialog().ShowAsync("重命名", false, "请输入新的分类名", SelectedClass.Name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    SelectedClass.Name = value;
                    await DbUtility.SaveClassAsync(SelectedClass);
                    await LoadAsync(Project);
                }
            }
        }
    }
    public class SelectedItemChanged<T> : EventArgs
    {
        public SelectedItemChanged(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; }
        public T NewValue { get; }
    }
}
