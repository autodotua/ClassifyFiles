using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static ClassifyFiles.Util.FileClassUtility;

namespace ClassifyFiles.UI.Dialog
{
    /// <summary>
    /// LogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageExifDialog : DialogWindowBase
    {
        public ImageExifDialog(string path)
        {
            InitializeComponent();
            Path = path;
        }

        public string Path { get; }

        private async void DialogWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            var metadatas = ImageMetadataReader.ReadMetadata(Path);
            ExifSubIfdDirectory dir = metadatas.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (dir == null)
            {
                await new MessageDialog().ShowAsync("找不到Exif信息", "文件Exif信息");
                return;
            }
            lvw.ItemsSource = dir.Tags;
        }

        private void ListViewItem_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText("");
        }

        private  void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((sender as Button).Tag as MetadataExtractor.Tag).Type.ToString());
            (sender as Button).Content = "√";
        }
    }
}
