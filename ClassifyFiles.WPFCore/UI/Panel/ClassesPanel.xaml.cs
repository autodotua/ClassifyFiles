using ClassifyFiles.Data;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// ClassesPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ClassesPanel : UserControlBase
    {
        private List<Class> classes;
        public List<Class> Classes
        {
            get => classes;
            set
            {
                classes = value;
                this.Notify(nameof(Classes));
            }
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
