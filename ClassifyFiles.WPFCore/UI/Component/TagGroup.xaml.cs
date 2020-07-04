using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
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

namespace ClassifyFiles.UI.Component
{
    /// <summary>
    /// TagGroup.xaml 的交互逻辑
    /// </summary>
    public partial class TagGroup : UserControl
    {
        public TagGroup()
        {
            InitializeComponent();
            grd.DataContext = this;
        }
 
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TagMouseDown?.Invoke(sender, e);
        }
        public static readonly DependencyProperty FileProperty =
DependencyProperty.Register("File", typeof(UIFile), typeof(TagGroup), new PropertyMetadata(OnFileChanged));
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
        public event MouseButtonEventHandler TagMouseDown;
    }
}
