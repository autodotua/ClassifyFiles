using ClassifyFiles.WPFCore;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace ClassifyFiles.UI.Dialog
{
    public class ContentDialogBase : ContentDialog, INotifyPropertyChanged
    {
        public ContentDialogBase()
        {
            DataContext = this;
            Owner = App.CurrentWindow;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
