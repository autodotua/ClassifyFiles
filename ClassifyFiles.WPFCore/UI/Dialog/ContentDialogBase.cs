using ClassifyFiles.WPFCore;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClassifyFiles.UI.Dialog
{
    public abstract class ContentDialogBase : ContentDialog, INotifyPropertyChanged
    {
        protected ContentDialogBase()
        {
            DataContext = this;
            Owner = App.CurrentWindow;
            Closed += ContentDialogBase_Closed;
        }

        private void ContentDialogBase_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            currentDialog = null;
        }

        private static ContentDialogBase currentDialog = null;
        protected new Task<ContentDialogResult> ShowAsync()
        {
            if (currentDialog != null)
            {
                currentDialog.Hide();
            }
            currentDialog = this;
            return base.ShowAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
