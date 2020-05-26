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
            //cbbLanguage.SelectedItem = cbbLanguage.Items.Cast<ComboBoxItem>().First(p => p.Tag.Equals(Config.Language));
            cbbTheme.SelectedItem = cbbTheme.Items.Cast<ComboBoxItem>().First(p => p.Tag.Equals(GUIConfig.Theme.ToString()));
            cbbTheme.SelectionChanged += cbbTheme_SelectionChanged;

           
            //this.Notify(nameof(Config));
        }


        private void cbbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Config.Language = (cbbLanguage.SelectedItem as ComboBoxItem).Tag as string;
            //App.Current.SetCulture();
            //Config.Save();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
    
        }

        private void cbbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GUIConfig.Theme = int.Parse((cbbTheme.SelectedItem as ComboBoxItem).Tag as string);
            App.SetTheme();
            GUIConfig.Save();
        }

        //public Config Config => Config.Instance;
        public GUIConfig GUIConfig => GUIConfig.Instance;

     
        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void RegardOneSideParseErrorAsNotSameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //Config.Save();
        }
    }
}
