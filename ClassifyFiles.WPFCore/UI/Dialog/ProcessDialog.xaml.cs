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
    public partial class ProgressDialog : UserControl
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }
        public void Show()
        {
            if(dialog.IsOpen)
            {
                return;
            }
            dialog.ShowDialog(dialog.DialogContent);
        }

        public void Close()
        {
            dialog.CurrentSession?.Close();
        }

    }


}
