using ClassifyFiles.UI.Dialog;
using FzLib.Extension;

using ModernWpf.Controls;
using System.Threading.Tasks;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class DeleteFilesDialog : ContentDialogBase
    {
        public DeleteFilesDialog()
        {
            InitializeComponent();
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

        public async Task<int> ShowAsync(int fileCount)
        {
            Message = $"是否删除{fileCount}个文件？";
            int result = (int)await ShowAsync();
            if (chkRemember.IsChecked.Value)
            {
                Configs.AutoDeleteFiles = result;
            }
            return result;
        }
    }
}