using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace ClassifyFiles.UI.Dialog
{
   public abstract class DialogBase: ContentDialog, INotifyPropertyChanged
    {
        public DialogBase()
        {
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
