using FzLib.Extension;
using System;
using System.Windows;
using System.Windows.Media.Animation;

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
        public void Show(bool overlay)
        {
            Message = "";
            grdOverlay.Opacity = overlay ? 0.75 : 0;
            ring.IsActive = true;
            Opacity = 0;
            Visibility = Visibility.Visible;
            DoubleAnimation ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
            BeginAnimation(OpacityProperty, ani);
        }

        public void Close()
        {
            ring.IsActive = false;
            DoubleAnimation ani = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            ani.Completed += (p1, p2) =>
            {
                Visibility = Visibility.Collapsed;
            };
            BeginAnimation(OpacityProperty, ani);

        }

    }


}
