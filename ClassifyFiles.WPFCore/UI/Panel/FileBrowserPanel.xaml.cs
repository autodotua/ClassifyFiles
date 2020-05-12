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
using MaterialDesignThemes.Wpf;
using System.Diagnostics;

namespace ClassifyFiles.UI.Panel
{
    public partial class FileBrowserPanel : ProjectPanelBase
    {
        private ObservableCollection<FileWithIcon> files;
        public ObservableCollection<FileWithIcon> Files
        {
            get => files;
            set
            {
                files = value;
                this.Notify(nameof(Files));
            }
        }
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
            Class c = new Class()
            {
                MatchConditions = new List<MatchCondition>()
                {
                    new MatchCondition(){Type=MatchType.InFileName,Value="航拍"},
                    new MatchCondition(){ConnectionLogic=Logic.Or, Type=MatchType.InDirName,Value="航拍"}
                }
            };
            var classes = new List<Class>() { c };


            //Files = new ObservableCollection<FileWithIcon>(result.First().Value.Select(p => new FileWithIcon(p)));
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
        }

        private async void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            loading.Show();
         var   classFiles = await FileUtility.GetFilesAsync(new System.IO.DirectoryInfo(Project.RootPath), GetClassesPanel().Classes);
            await DbUtility.UpdateFilesAsync(classFiles);
            if(classes.SelectedClass!=null &&classFiles.ContainsKey(classes.SelectedClass))
            {
                Files = new ObservableCollection<FileWithIcon>(classFiles[classes.SelectedClass].Select(p => new FileWithIcon(p)));
            }
            loading.Close();
        }

        private async void classes_SelectedClassChanged(object sender, SelectedItemChanged<Class> e)
        {
            if (e.NewValue != null)
            {
                var files = await DbUtility.GetFilesAsync(e.NewValue);

                Files = new ObservableCollection<FileWithIcon>(files.Select(p=>new FileWithIcon(p)));
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lvwFiles==null)
            {
                return;
            }
            if(lbxDisplayMode.SelectedIndex==0)
            {
                lvwFiles.Visibility = Visibility.Visible;
                lbxGrdFiles.Visibility = Visibility.Collapsed;
            }
            else {
                lvwFiles.Visibility = Visibility.Collapsed;
                lbxGrdFiles.Visibility = Visibility.Visible;
            }
        }

        private void lvwFiles_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(lvwFiles.SelectedItem!=null)
            {
                var file = lvwFiles.SelectedItem as File; var p = new Process();
                p.StartInfo = new ProcessStartInfo(System.IO.Path.Combine(Project.RootPath, file.Dir, file.Name))
                {
                    UseShellExecute = true
                };
                p.Start();
            }
        }
    }


    public class FileWithIcon : File
    {
        public PackIconKind Kind { get; set; } = PackIconKind.File;

        public FileWithIcon() { }

        public FileWithIcon(File file)
        {
            Name = file.Name;
            Dir = file.Dir;
        }

    }
}
