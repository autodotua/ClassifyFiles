using FzLib.Extension;
using System.ComponentModel;
using System.Threading.Tasks;
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
            if (Showing)
            {
                e.Cancel = true;
            }
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

        public bool Showing => showCount > 0;

        private int showCount = 0;

        public void Show()
        {
            if (showCount++>0)
            {
                return;
            }
            System.Diagnostics.Debug.WriteLine(showCount);
            Message = "正在处理";
            Visibility = Visibility.Visible;
            if (Configs.ShowRing)
            {
                ring.IsActive = true;
                DoubleAnimation ani = new DoubleAnimation(1, Configs.AnimationDuration);
                Dispatcher.Invoke(() =>
                BeginAnimation(OpacityProperty, ani));
            }
            else
            {
                Cursor = System.Windows.Input.Cursors.Wait;
            }
        }

        public void Close()
        {
            if (--showCount > 0)
            {
                return;
            }
            if (Configs.ShowRing)
            {
                ring.IsActive = false;
                DoubleAnimation ani = new DoubleAnimation(0, Configs.AnimationDuration);
                ani.Completed += (p1, p2) =>
                    Visibility = Visibility.Collapsed;
                BeginAnimation(OpacityProperty, ani);
            }
            else
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
                Visibility = Visibility.Collapsed;
            }
        }

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Closing += Window_Closing;
        }
    }
}