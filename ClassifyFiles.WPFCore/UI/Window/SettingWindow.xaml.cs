using ClassifyFiles.Util;
using ClassifyFiles.WPFCore;
using FzLib.Extension;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClassifyFiles.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : WindowBase
    {
        public SettingWindow()
        {
            InitializeComponent();

            int theme = Configs.Theme;
            switch (theme)
            {
                case 0: rbtnThemeAuto.IsChecked = true; break;
                case -1: rbtnThemeDark.IsChecked = true; break;
                case 1: rbtnThemeLight.IsChecked = true; break;
                default:
                    break;
            }
            chkAutoThumbnails.IsChecked = Configs.AutoThumbnails;
            //if(ConfigUtility.GetBool(ConfigKeys.IncludeThumbnailsWhenAddingFilesKey,true))
            //{
            //    chkIncludeThumbnailsWhenAddingFiles.IsChecked = true;
            //}
        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Configs.AutoThumbnails = chkAutoThumbnails.IsChecked.Value;
            //ConfigUtility.Set(ConfigKeys.IncludeThumbnailsWhenAddingFilesKey, chkIncludeThumbnailsWhenAddingFiles.IsChecked.Value);
        }


        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void ThemeRButton_Click(object sender, RoutedEventArgs e)
        {
            int theme = rbtnThemeAuto.IsChecked.Value ?
                0 : (rbtnThemeLight.IsChecked.Value ? 1 : -1);
            Configs.Theme = theme;
            App.SetTheme();
        }

        private async void ZipDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            long rawLength = new FileInfo(DbUtility.DbPath).Length;
            await DbUtility.ZipAsync();
            long nowLength = new FileInfo(DbUtility.DbPath).Length;
            await new MessageDialog().ShowAsync("压缩成功，当前数据库大小为" + FzLib.Basic.Number.ByteToFitString(nowLength)
                + "，" + System.Environment.NewLine + "共释放" + FzLib.Basic.Number.ByteToFitString(rawLength - nowLength), "压缩成功");
        }
    }
}
