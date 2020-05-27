using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public void Show(bool overlay)
        {
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
