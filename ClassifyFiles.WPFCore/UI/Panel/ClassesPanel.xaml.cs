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
        public bool editable;
        public bool Editable
        {
            get => editable;
            set
            {
                editable = value;
                this.Notify(nameof(Editable));
            }
        }
        public Project Project { get; private set; }
        public async Task LoadAsync(Project project)
        {
            Project = project;
            //var (treeClasses, tile) = await DbUtility.GetTreeAndTileClassesAsync(Project);
            var treeClasses= await DbUtility.GetClassesAsync(Project);
            Classes = new ObservableCollection<Class>(treeClasses);
        }

        public ClassesPanel()
        {
            InitializeComponent();
        }
        public Class SelectedClass { get; private set; }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var oldValue = SelectedClass;
            SelectedClass = e.NewValue as Class;
            this.Notify(nameof(SelectedClass));
            SelectedClassChanged?.Invoke(this, new SelectedItemChanged<Class>(oldValue, SelectedClass));
        }
        public event EventHandler<SelectedItemChanged<Class>> SelectedClassChanged;
        public event PropertyChangedEventHandler PropertyChanged;


        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClass == null)
            {
                await DbUtility.AddClassAsync(Project, null, false);
                await LoadAsync(Project);
            }
            else
            {
                MenuItem menuInside = new MenuItem() { Header = "在内部插入" };
                menuInside.Click += async (p1, p2) =>
                {

                    await DbUtility.AddClassAsync(Project, SelectedClass, true);

                    await LoadAsync(Project);
                };
                MenuItem menuAfter = new MenuItem() { Header = "在同级插入" };
                menuAfter.Click += async (p1, p2) =>
                {

                    await DbUtility.AddClassAsync(Project, SelectedClass, false);
                    await LoadAsync(Project);
                };

                ContextMenu menu = new ContextMenu()
                {
                    Items = { menuInside, menuAfter },
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Top,
                    PlacementTarget = sender as Button,
                    IsOpen = true,
                };
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClass == null)
            {
                await msgDialog.ShowAsync("请先选择一项");
            }
            else
            {
                await DbUtility.DeleteClassAsync(SelectedClass);
                await LoadAsync(Project);
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClass == null)
            {
                await msgDialog.ShowAsync("请先选择一项");
            }
            else
            {
                string value = await inputDialog.ShowAsync("重命名", false, "请输入新的分类名", SelectedClass.Name);
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
