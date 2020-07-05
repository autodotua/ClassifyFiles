using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using static ClassifyFiles.Data.Project;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;
using static ClassifyFiles.Util.DbUtility;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
        public  async Task LoadAsync(Project project)
        {
            Project = project;
            var treeClasses = await GetClassesAsync(Project);
            Items = new ObservableCollection<Class>(treeClasses);
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
                if(selectedItem==value)
                {
                    return;
                }
                var oldValue = selectedItem;
                selectedItem = value;
                this.Notify(nameof(SelectedItem));
                SelectedItemChanged?.Invoke(this, new SelectedItemChanged(oldValue, value));
            }
        }
        public Project Project { get; protected set; }

        private bool allFilesButtonVisiable;
        public bool AllFilesButtonVisiable
        {
            get => allFilesButtonVisiable;
            set
            {
                allFilesButtonVisiable = value;
                this.Notify(nameof(AllFilesButtonVisiable));
            }
        }
        public async Task AddAsync()
        {
            await AddClassAsync(Project);
            await LoadAsync(Project);
        }



        public async Task DeleteSelectedAsync()
        {
            if (SelectedItem == null)
            {
                await new MessageDialog().ShowAsync("请先选择一项", "错误");
            }
            else
            {
                await DeleteClassAsync(SelectedItem as Class);
                await LoadAsync(Project);
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
                    SelectedItem.Name = value;
                    await SaveClassAsync(SelectedItem as Class);
                    await LoadAsync(Project);
                }
            }
        }

        public event EventHandler<SelectedItemChanged> SelectedItemChanged;

        private void btnAllFiles_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
        }
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
