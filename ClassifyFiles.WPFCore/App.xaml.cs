using ClassifyFiles.UI;
using ClassifyFiles.Util;
using ClassifyFiles.Util.Win32;
using ClassifyFiles.Util.Win32.Shell;
using ModernWpf;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ClassifyFiles.WPFCore
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static new App Current { get; private set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            FileUtility.ThumbnailFolderPath = "thumb";
            FileUtility.GetFileIcon = path => ExplorerIcon.GetBitmapFromFilePath(path, ExplorerIcon.IconSizeEnum.ExtraLargeIcon);
            FileUtility.GetFolderIcon = path => ExplorerIcon.GetBitmapFromFolderPath(path, ExplorerIcon.IconSizeEnum.ExtraLargeIcon);

            FzLib.Program.Runtime.UnhandledException.RegistAll();
            FzLib.Program.Runtime.UnhandledException.UnhandledExceptionCatched += UnhandledException_UnhandledExceptionCatched;

            if (!Directory.Exists("thumb"))
            {
                Directory.CreateDirectory("thumb");
            }

            FileUtility.FFMpegPath = "exe/ffmpeg.exe";
            Current = this;
            SplashWindow.TryShow();
            MainWindow win = new MainWindow();
            MainWindow = win;
            win.Show();
            SplashWindow.TryClose();

        }

        private async void UnhandledException_UnhandledExceptionCatched(object sender, FzLib.Program.Runtime.UnhandledExceptionEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    LogUtility.AddLog(e.Exception.Message, e.Exception.ToString());
                }
                catch (Exception ex)
                {

                }
            });
            if (!e.Exception.Source.StartsWith("Microsoft.EntityFrameworkCore"))
            {

                await Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        await new ErrorDialog().ShowAsync(e.Exception, "程序发生错误");
                    }
                    catch
                    {

                    }
                });
            }
        }

        private static void InitializeTheme()
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
            InitializeTheme();
            ElementTheme theme = ElementTheme.Default;

            switch (Configs.Theme)
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
                foreach (var win in Current.Windows)
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