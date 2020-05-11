using ClassifyFiles.WPFCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ClassifyFiles.UI
{
   public class SelectorHelper<T> where T : class
    {
        public SelectorHelper(Selector view, ObservableCollection<T> source)
        {
            View = view;
            Source = source;
        }

        public void SetContextMenu() 
        {
            MenuItem menuDelete = new MenuItem() { Header = App.Current.FindResource("win_delete") as string };
            menuDelete.Click += (p1, p2) =>
            {
                T item = View.SelectedItem as T;
                Source.Remove(item);
                Delete?.Invoke(p1, item);
            };
            
            MenuItem menuClone = new MenuItem() { Header = App.Current.FindResource("win_clone") as string };
            menuClone.Click += (p1, p2) =>
            {
                T item = View.SelectedItem as T;
                Clone?.Invoke(p1, item);
            };

            ContextMenu menu = new ContextMenu();
            menu.Items.Add(menuDelete);
            menu.Items.Add(menuClone);
            if (View != null)
            {
                View.ContextMenu = menu;
            }
        }
        public event EventHandler<T> Delete;
        public event EventHandler<T> Clone;

        public Selector View { get; }
        public ObservableCollection<T> Source { get; }
    }
}
