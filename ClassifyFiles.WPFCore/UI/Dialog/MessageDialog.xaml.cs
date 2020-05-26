using ClassifyFiles.UI.Dialog;
using FzLib.Extension;
using MaterialDesignThemes.Wpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MessageDialog : DialogBase
    {
        public MessageDialog()
        {
            InitializeComponent();
            base.PrimaryButtonText = "确定";
        }
        private string message;

        public string Message
        {
            get => message;
            set
            {
                message = value;
                this.Notify(nameof(Message));
            }
        }
        public async new Task ShowAsync()
        {
            await base.ShowAsync();
        }
        public async new Task ShowAsync(string message, string title)
        {
            Message = message;
            Title = title;
            await base.ShowAsync();
        }

    }
}
