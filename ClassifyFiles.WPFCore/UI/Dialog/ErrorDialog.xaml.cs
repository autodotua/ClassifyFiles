using ClassifyFiles.UI.Dialog;
using FzLib.Extension;

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
    public partial class ErrorDialog : DialogBase
    {
        public ErrorDialog()
        {
            InitializeComponent();
            base.PrimaryButtonText = "确定";
        }
        private string detail;

        public string Message
        {
            get => detail;
            set
            {
                detail = value;
                this.Notify(nameof(Message));
            }
        }
        private string message;

        public Visibility DetailVisibility => Detail == null ? Visibility.Collapsed : Visibility.Visible;
        public string Detail
        {
            get => message;
            set
            {
                message = value;
                this.Notify(nameof(Detail), nameof(DetailVisibility));
                System.Diagnostics.Debug.WriteLine("New Error：");
                System.Diagnostics.Debug.WriteLine(value);
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
        public async new Task ShowAsync(Exception ex, string title)
        {
            Message = ex.Message;
            Detail = ex.ToString();
            Title = title;
            await base.ShowAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Detail);
            (sender as Button).IsEnabled = false;
        }
    }
}
