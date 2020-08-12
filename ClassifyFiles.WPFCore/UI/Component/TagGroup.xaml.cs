using ClassifyFiles.UI.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClassifyFiles.UI.Component
{
    /// <summary>
    /// TagGroup.xaml 的交互逻辑
    /// </summary>
    public partial class TagGroup : UserControl
    {
        public TagGroup()
        {
            if (!Configs.ShowClassTags)
            {
                return;
            }
            InitializeComponent();
            grd.DataContext = this;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TagMouseDown?.Invoke(this, e);
        }

        public static readonly DependencyProperty FileProperty =
DependencyProperty.Register("File", typeof(UIFile), typeof(TagGroup), new PropertyMetadata(OnFileChanged));

        private static void OnFileChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
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

        public static readonly DependencyProperty OrientationProperty =
DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TagGroup));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty); //file;
            set
            {
                SetValue(OrientationProperty, value);
            }
        }

        public event MouseButtonEventHandler TagMouseDown;

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //防止将ScrollChanged事件传递到FileViewer的List****中
            e.Handled = true;
        }
    }
}