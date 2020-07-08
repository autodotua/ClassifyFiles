using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace ClassifyFiles.UI
{
    public class UserControlBase : UserControl, INotifyPropertyChanged
    {
        public UserControlBase()
        {
            Initialized += (p1, p2) =>
            {
                (Content as System.Windows.FrameworkElement).DataContext = this;
            };
            //DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
