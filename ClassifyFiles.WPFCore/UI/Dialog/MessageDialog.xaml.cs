using ClassifyFiles.UI.Dialog;
using FzLib.Extension;
using System.Threading.Tasks;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog : ContentDialogBase
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