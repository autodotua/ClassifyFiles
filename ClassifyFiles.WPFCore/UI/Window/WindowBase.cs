using ModernWpf;
using ModernWpf.Controls.Primitives;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ClassifyFiles.UI
{
    public class WindowBase : Window, INotifyPropertyChanged
    {
        public WindowBase()
        {
            DataContext = this;
            WindowHelper.SetUseModernWindowStyle(this, true);
            ThemeManager.SetIsThemeAware(this, true);
            App.SetTheme(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        /// <summary>
        /// 把窗体带到最前面
        /// </summary>
        public async void BringToFront()
        {
            if (!IsVisible)
            {
                Show();
            }

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
            Topmost = true;  // important
            Dispatcher.InvokeAsync(() =>
            {
                Topmost = false; // important
                Focus();
            }, DispatcherPriority.Loaded);
        }
    }
}