using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace ClassifyFiles.UI.Dialog
{
   public abstract class DialogBase:UserControl, INotifyPropertyChanged
    {
        public DialogBase()
        {
            DataContext = this;
        }

        protected void Notify(params string[] names)
        {
            foreach (var name in names)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetValueAndNotify<T>(ref T field, T value, params string[] names)
        {
            field = value;
            Notify(names);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
