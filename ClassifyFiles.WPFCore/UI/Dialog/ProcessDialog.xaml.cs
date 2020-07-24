using FzLib.Extension;
using System;
using System.ComponentModel;
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        bool canClose = true;

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
        public bool Showing { get; private set; }
        public void Show(bool overlay)
        {
            if(Showing)
            {
                return;
            }
            Showing = true;
            canClose = false;
            Message = "";
            grdOverlay.Opacity = overlay ? 0.75 : 0;
            ring.IsActive = true;
            Opacity = 0;
            Visibility = Visibility.Visible;
            DoubleAnimation ani = new DoubleAnimation(1, Configs.AnimationDuration);
            BeginAnimation(OpacityProperty, ani);
        }

        public void Close()
        {
            canClose = true;
            ring.IsActive = false;
            DoubleAnimation ani = new DoubleAnimation(0, Configs.AnimationDuration);
            ani.Completed += (p1, p2) =>
            {
                Visibility = Visibility.Collapsed;
                Showing = false;
            };
            BeginAnimation(OpacityProperty, ani);

        }

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Closing += Window_Closing;
        }
    }


}
