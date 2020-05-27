using ClassifyFiles.UI.Dialog;
using FzLib.Extension;

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
        public async Task<string> ShowAsync(string title, bool multipleLines, string hint = "", string defaultContent = "")
        {
            tbkDialogTitle.Text = title;
            InputContent = defaultContent;
            this.Notify(nameof(InputContent));
            if (multipleLines)
            {
                textLine.Visibility = Visibility.Collapsed;
                textArea.Visibility = Visibility.Visible;
                textArea.SelectAll();
            }
            else
            {
                textArea.Visibility = Visibility.Collapsed;
                textLine.Visibility = Visibility.Visible;
                textLine.SelectAll();
            }
            PrimaryButtonClick += (p1, p2) =>
            {
                Result = true;
            };
            await base.ShowAsync();
            return Result ? InputContent : "";
        }
        public string InputContent { get; set; }
        public bool Result { get; set; }
        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    if (sender == btnOk)
        //    {
        //        Result = true;
        //    }
        //    else if (sender == btnCancel)
        //    {
        //        Result = false;
        //    }

        //    dialog.CurrentSession.Close();
        //}
    }
}
