using ModernWpf.Controls;
using System.ComponentModel;
using System.Threading.Tasks;

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