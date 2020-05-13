using FzLib.Extension;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressDialog : UserControlBase
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }
        public void Show()
        {
            if (dialog.IsOpen)
            {
                return;
            }
            dialog.ShowDialog(dialog.DialogContent);
        }
        //private string message;
        //public string Message
        //{
        //    get => message;
        //    private set
        //    {
        //        message = value;
        //        this.Notify(nameof(Message));
        //    }
        //}

        public void SetMessage(string message)
        {
            Dispatcher?.Invoke(() =>
            {
                if (message == null && tbk.Visibility == Visibility.Visible)
                {
                    tbk.Visibility = Visibility.Collapsed;
                }
                else if (message != null)
                {
                    if (tbk.Visibility == Visibility.Collapsed)
                    {
                        tbk.Visibility = Visibility.Visible;
                    }
                    tbk.Text = message;
                }
            });
        }

        public void Close()
        {
            dialog.CurrentSession?.Close();
        }

    }


}
