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
using System.Diagnostics;
using DImg = System.Drawing.Image;
using ModernWpf.Controls;
using ClassifyFiles.UI.Model;

namespace ClassifyFiles.UI.Panel
{
    public partial class ProjectSettingsPanel : ProjectPanelBase
    {

        public ProjectSettingsPanel()
        {
            InitializeComponent();
        }

        public override ClassesPanel GetClassesPanel()
        {
            return null;
        }

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
        }


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            flyoutDelete.Hide();
            MainWindow mainWin = (Window.GetWindow(this) as MainWindow);
            await mainWin.DeleteSelectedProjectAsync();
        }


    }


}
