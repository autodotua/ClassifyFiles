using ClassifyFiles.UI.Dialog;
using FzLib.Extension;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : ContentDialogBase
    {
        public InputDialog()
        {
            InitializeComponent();
        }
        public async Task<string> ShowAsync(string title, bool multipleLines, string hint = "", string defaultContent = "")
        {
            Title = title;
            InputContent = defaultContent;
            this.Notify(nameof(InputContent));
            TextBox txt = multipleLines ? textArea : textLine;
            txt.Visibility = Visibility.Visible;

            Opened += (p1, p2) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                   (Action)(() =>
                   {
                       txt.Focus();
                       txt.SelectAll();
                       Keyboard.Focus(txt);
                   }));
                
            };
            PrimaryButtonClick += (p1, p2) => Result = true;
            await ShowAsync();
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
