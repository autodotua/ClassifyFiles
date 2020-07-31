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
    public partial class FileMetadataDialog : DialogWindowBase
    {
        public FileMetadataDialog(string path)
        {
            InitializeComponent();
            Path = path;
        }

        public string Path { get; }

        private void ListViewItem_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText("");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((sender as Button).Tag as MetadataExtractor.Tag).Type.ToString());
            (sender as Button).Content = "√";
        }

        private async void DialogWindowBase_ContentRendered(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(Path))
            {
                await new MessageDialog().ShowAsync("找不到文件", "文件元数据");
                Close();
                return;
            }
            try
            {
                IReadOnlyList<MetadataExtractor.Directory> metadatas = ImageMetadataReader.ReadMetadata(Path);
                items.ItemsSource = metadatas;
            }
            catch (Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "获取文件元数据错误");
                Close();
                return;
            }
        }
    }
}
