using ClassifyFiles.Data;
using ClassifyFiles.UI.Util;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClassifyFiles.UI.Component
{
    public abstract class TitleBarBase : UserControlBase
    {
        protected void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            AddProjectButtonClick?.Invoke(sender, e);
        }

        protected void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedViewPanelChanged?.Invoke(sender, e);
        }

        public static readonly DependencyProperty ProjectsProperty =
          DependencyProperty.Register(nameof(Projects), typeof(ObservableCollection<Project>), typeof(TitleBarBase));

        public ObservableCollection<Project> Projects
        {
            get => GetValue(ProjectsProperty) as ObservableCollection<Project>; //file;
            set
            {
                SetValue(ProjectsProperty, value);
            }
        }
        public static readonly DependencyProperty SelectedProjectProperty =
         DependencyProperty.Register(nameof(SelectedProject), typeof(Project), typeof(TitleBarBase), new PropertyMetadata(OnSelectedProjectChanged));

        protected static void OnSelectedProjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TitleBarBase).SelectedProjectChanged?.Invoke(d, new EventArgs());
        }

        public event EventHandler SelectedProjectChanged;
        public event SelectionChangedEventHandler SelectedViewPanelChanged;
        public event RoutedEventHandler AddProjectButtonClick;

        public Project SelectedProject
        {
            get => GetValue(SelectedProjectProperty) as Project;
            set
            {
                SetValue(SelectedProjectProperty, value);
            }
        }
        protected void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //确保只有一个SettingWindow
            if (SettingWindow.Current == null)
            {
                SettingWindow win = new SettingWindow(Projects);
                win.Show();
            }
            else
            {
                SettingWindow.Current.BringToFront();
            }
        }
        protected void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scr)
            {
                e.Handled = true;
                SmoothScrollViewerHelper.HandleMouseWheel(scr, e.Delta, true);
            }
        }


    }
}
