using ClassifyFiles.UI.Dialog;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Windows;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : DialogBase
    {
        public InputDialog()
        {
            InitializeComponent();
        }
        public async Task<string> ShowAsync(string title, bool multipleLines,string hint="", string defaultContent = "")
        {
            tbkDialogTitle.Text = title;
            HintAssist.SetHint(textArea, hint);
            textLine.Visibility = multipleLines ? Visibility.Collapsed : Visibility.Visible;
            textArea.Visibility = multipleLines ? Visibility.Visible : Visibility.Collapsed;
            InputContent = defaultContent;
            Notify(nameof(InputContent));
            await dialog.ShowDialog(dialog.DialogContent);
            return Result ? InputContent : "";
        }
        public string InputContent { get; set; }
        public bool Result { get; set; }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
            {
                Result = true;
            }
            else if (sender == btnCancel)
            {
                Result = false;
            }

            dialog.CurrentSession.Close();
        }
    }
}
