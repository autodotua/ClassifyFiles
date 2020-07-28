using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Globalization;
using ModernWpf.Controls.Primitives;
using ModernWpf;

namespace ClassifyFiles.UI
{
    public class WindowBase : Window, INotifyPropertyChanged
    {
        public WindowBase()
        {
            DataContext = this;
            WindowHelper.SetUseModernWindowStyle(this, true);
            ThemeManager.SetIsThemeAware(this, true);
            WPFCore.App.SetTheme(this);
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
        public void BringToFront()
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
            Topmost = false; // important
            Focus();
        }

    }

}
