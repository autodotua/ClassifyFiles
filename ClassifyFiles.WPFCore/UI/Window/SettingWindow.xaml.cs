﻿using ClassifyFiles.Util;
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

            int theme = ConfigUtility.GetInt(ConfigKeys.ThemeKey, 0);
            switch (theme)
            {
                case 0: rbtnThemeAuto.IsChecked = true; break;
                case -1: rbtnThemeDark.IsChecked = true; break;
                case 1: rbtnThemeLight.IsChecked = true; break;
                default:
                    break;
            }
            if(ConfigUtility.GetBool(ConfigKeys.IncludeThumbnailsWhenAddingFilesKey,true))
            {
                chkIncludeThumbnailsWhenAddingFiles.IsChecked = true;
            }
        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ConfigUtility.Set(ConfigKeys.IncludeThumbnailsWhenAddingFilesKey, chkIncludeThumbnailsWhenAddingFiles.IsChecked.Value);
        }


        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void rbtnThemeAuto_Click(object sender, RoutedEventArgs e)
        {
            int theme = rbtnThemeAuto.IsChecked .Value?
                0 : (rbtnThemeLight.IsChecked.Value?1:-1);
            ConfigUtility.Set(ConfigKeys.ThemeKey, theme);

        }
    }
}
