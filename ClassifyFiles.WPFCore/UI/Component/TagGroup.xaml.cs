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
            //Loaded += (p1, p2) =>
            //{
            //    var a = Tags;

            //};
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(p =>
            {
                Dispatcher.Invoke(() =>
                {
                    //var a = Tags;
                    //var b = File;
                    System.Diagnostics.Debug.WriteLine(File);
                });
            });
        }
        //public ObservableCollection<Tag> Tags=> new ObservableCollection<Tag>(File.Tags);
        //        public static readonly DependencyProperty TagsProperty =
        //DependencyProperty.Register(nameof(Tags),
        //    typeof(ObservableCollection<Tag>), typeof(TagGroup),
        //    new PropertyMetadata(OnTagsChanged));
        //        static void OnTagsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        //        {
        //        }
        //        public ObservableCollection<Tag> Tags
        //        {
        //            get => GetValue(TagProperty) as ObservableCollection<Tag>; //file;
        //            set
        //            {
        //                SetValue(TagProperty, value);
        //                //file = value;
        //                //this.Notify(nameof(File));
        //            }
        //        }

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
