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
using System.Windows.Markup;
using System.Diagnostics;

namespace ClassifyFiles.UI.Panel
{
    public partial class TagSettingPanel : ProjectPanelBase
    {
        public TagSettingPanel()
        {
            InitializeComponent();
        }

        public override ListPanelBase GetItemsPanel()
        {
            return tags;
        }

        public async override Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
            fileViewer.Project = Project;
        }

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void AddClassInButton_Click(object sender, RoutedEventArgs e)
        {
            await tags.AddAsync();
        }

        private async void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            await tags.DeleteSelectedAsync();
        }

        private async void RenameClassButton_Click(object sender, RoutedEventArgs e)
        {
            await tags.RenameButton();
        }





        private async void classes_SelectedItemChanged_1(object sender, SelectedItemChanged e)
        {
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var files = await FileUtility.GetAllFiles(new System.IO.DirectoryInfo(Project.RootPath), false, null);
            fileViewer.SetFiles(files);
        }
        private void filesViewer_ClickTag(object sender, ClickTagEventArgs e)
        {
            if (e.Tag != tags.SelectedItem)
            {
                tags.SelectedItem = e.Tag;
            }
        }
    }


}
