using ClassifyFiles.UI.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassifyFiles.Util;
using System.Windows.Controls.Primitives;

namespace ClassifyFiles.UI.Util
{
    public class DragDropFilesHelper
    {
        private ListBox list;
        private bool mouseDown = false;
        private bool set = false;
        private Point beginPosition = default;

        public DragDropFilesHelper(ListBox list)
        {
            this.list = list;
        }
        public void Regist()
        {
            list.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            list.PreviewMouseLeftButtonUp += List_PreviewMouseLeftButtonUp;
            list.PreviewMouseMove += List_PreviewMouseMove;
        }

        private void List_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            Point position = e.GetPosition(null);
            double distance = Math.Sqrt(Math.Pow(position.X - beginPosition.X, 2) + Math.Pow(position.Y - beginPosition.Y, 2));
            //如果还没有放置项，并且鼠标已经按下，并且移动距离超过了10单位
            if (!set && mouseDown && distance > 10)
            {
                if (e.OriginalSource is Thumb || e.OriginalSource is RepeatButton)
                {
                    return;
                }
                set = true;
                var files = list.SelectedItems.Cast<UIFile>().Select(p => p.File.GetAbsolutePath()).ToArray();
                if (files.Length == 0)
                {
                    return;
                }
                var data = new DataObject(DataFormats.FileDrop, files);
                //放置一个特殊类型，这样好让自己的程序识别，防止自己拖放到自己身上
                data.SetData(nameof(ClassifyFiles), "");
                //实测支持复制和移动，不知道为什么不支持快捷方式
                DragDrop.DoDragDrop(sender as DependencyObject, data, DragDropEffects.All);
            }
        }

        private void List_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            set = false;
            if (ignoredSelect)
            {
                //当我们发现用户并不是真的要拖放，而是真的想选中某一个项时，
                //就把该项单独选中
                ignoredSelect = false;

                if (!list.SelectedItems.Cast<object>().Any(p => list.ItemContainerGenerator.ContainerFromItem(p) == null))
                {
                    var mouseOverItem = list.SelectedItems.Cast<object>().FirstOrDefault(p =>
               (list.ItemContainerGenerator.ContainerFromItem(p) as ListBoxItem).IsMouseOver);
                    if (mouseOverItem != null)
                    {
                        list.SelectedItem = mouseOverItem;
                    }
                }
            }
        }

        bool ignoredSelect = false;
        private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            beginPosition = e.GetPosition(null);
            //当鼠标点击列表项时，如果鼠标位置在已经被选中的项的上方，那么取消响应
            //这是由于ListView总是在拖放之前就把多选变成了单选，与拖放需求不符
            if (e.ClickCount > 1)
            {
                return;
            }
            //判断鼠标是否在已经选中的项的上方
            var hasMouseOver = list.SelectedItems.Cast<object>().Any(p =>
            {
                if (list.ItemContainerGenerator.ContainerFromItem(p) is ListBoxItem item)
                {
                    return item.IsMouseOver;
                }
                return false;
            });
            if (hasMouseOver)
            {
                ignoredSelect = true;
                e.Handled = true;
            }
        }

    }

}
