using ClassifyFiles.UI.Util;
using FzLib.Extension;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClassifyFiles.UI.Component
{
    /// <summary>
    /// SingleLineTitleBar.xaml 的交互逻辑
    /// </summary>
    public partial class SingleLineTitleBar : TitleBarBase
    {
        public SingleLineTitleBar()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scr)
            {
                e.Handled = true;
                SmoothScrollViewerHelper.HandleMouseWheel(scr, e.Delta, true);
            }
        }

    }
}
