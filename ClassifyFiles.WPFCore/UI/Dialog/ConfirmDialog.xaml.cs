using ClassifyFiles.UI.Dialog;
using FzLib.Extension;

using ModernWpf.Controls;
using System.Threading.Tasks;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmDialog : ContentDialogBase
    {
        public ConfirmDialog()
        {
            InitializeComponent();
            base.PrimaryButtonText = "是";
            base.SecondaryButtonText = "否";
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

        public async new Task<bool> ShowAsync()
        {
            return await base.ShowAsync() == ContentDialogResult.Primary;
        }

        public async new Task<bool> ShowAsync(string message, string title)
        {
            Message = message;
            Title = title;
            return await base.ShowAsync() == ContentDialogResult.Primary;
        }
    }
}