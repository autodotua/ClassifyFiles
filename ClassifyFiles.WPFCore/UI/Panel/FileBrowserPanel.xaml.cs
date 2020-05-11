using FzLib.Basic;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using ClassifyFiles.UI.Panel;

namespace ClassifyFiles.UI.Panel
{
    public partial class FileBrowserPanel : ProjectPanelBase
    {
        public FileBrowserPanel()
        {
            InitializeComponent();
        }

        public override ClassesPanel GetClassesPanel()
        {
            return classes;
        }

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
    
}
