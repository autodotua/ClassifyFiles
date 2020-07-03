using ClassifyFiles.UI;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClassifyFiles.WPFCore
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static new App Current { get; private set; }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Current = this;
            Window splash = new SplashWindow();
            splash.Show();
            MainWindow win = new MainWindow();
            win.Loaded += (p1, p2) => splash.Close();
#if (!DEBUG)
            UnhandledException.RegistAll();

            FzLib.Program.Runtime.SingleInstance singleInstance = new FzLib.Program.Runtime.SingleInstance(Assembly.GetExecutingAssembly().FullName);
            if (await singleInstance.CheckAndOpenWindow(this, this))
            {
                return;
            }
#endif

            //FzLib.Program.App.SetWorkingDirectoryToAppPath();

            //InitializeTheme();

            //SetTheme();

            //SetCulture();

            win.Show();

        }

        private void InitializeTheme()
        {
            var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
            if (v == null || v.ToString() == "1")
            {
                AppsUseLightTheme = true;
            }
            v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "0");
            if (v == null || v.ToString() == "1")
            {
                SystemUsesLightTheme = true;
            }
        }
        public static void SetTheme(FrameworkElement element = null)
        {
            ElementTheme theme = ElementTheme.Default;

            switch (GUIConfig.Instance.Theme)
            {
                case 0:
                    if (AppsUseLightTheme)
                    {
                        goto l;
                    }
                    goto d;
                case -1:
                d:
                    theme = ElementTheme.Dark;
                    break;
                case 1:
                l:
                    theme = ElementTheme.Light;
                    break;
            }
            if (element == null)
            {
                foreach (var win in App.Current.Windows)
                {
                    ThemeManager.SetRequestedTheme(win as Window, theme);

                }
            }
            else
            {
                ThemeManager.SetRequestedTheme(element, theme);
            }
        }

        public static bool SystemUsesLightTheme { get; private set; }
        public static bool AppsUseLightTheme { get; private set; }


        //public void SetCulture()
        //{
        //    string culture = Config.Instance.Language;

        //    //Copy all MergedDictionarys into a auxiliar list.
        //    var dictionary = Resources.MergedDictionaries;

        //    //Search for the specified culture.     
        //    string requestedCulture = string.Format("/Properties/StringResources.{0}.xaml", culture);
        //    var resourceDictionary = dictionary.
        //        FirstOrDefault(p => p.Source != null && p.Source.OriginalString == requestedCulture);


        //    //If we have the requested resource, remove it from the list and place at the end.     
        //    //Then this language will be our string table to use.      
        //    if (resourceDictionary != null)
        //    {
        //        dictionary.Remove(resourceDictionary);
        //        dictionary.Add(resourceDictionary);
        //    }


        //    //Inform the threads of the new culture.     
        //    var c = new CultureInfo(culture);
        //    Thread.CurrentThread.CurrentCulture = c;
        //    Thread.CurrentThread.CurrentUICulture = c;
        //}


    }
}