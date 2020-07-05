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
using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using ClassifyFiles.UI.Panel;
using System.Diagnostics;
using DImg = System.Drawing.Image;
using ModernWpf.Controls;
using ClassifyFiles.UI.Model;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using FzLib.Program;
using Microsoft.WindowsAPICodePack.Dialogs;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;

namespace ClassifyFiles.UI.Panel
{
    public partial class ProjectSettingsPanel : ProjectPanelBase
    {
        public string Splitter { get; set; } = "-";
        public ProjectSettingsPanel()
        {
            InitializeComponent();
        }


        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
            tbkFilesCount.Text = (await GetFilesCountAsync(project)).ToString();
            tbkClassesCount.Text =( await GetClassesCountAsync(project)).ToString();
            tbkFileClassesCount.Text = (await GetFileClassesCountAsync(project)).ToString();
        }

        public ExportFormat ExportFormat { get; set; }
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            flyoutDelete.Hide();
            MainWindow mainWin = (Window.GetWindow(this) as MainWindow);
            await mainWin.DeleteSelectedProjectAsync();
        }
        void ShowProgressMessage(Data.File file)
        {
            Dispatcher.Invoke(() => GetProgress().Message = "正在导出" + file.Name);
        }
        private async void ExportLinkButton_Click(object sender, RoutedEventArgs e)
        {
            CommonFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "请选择用于存放快捷方式的文件夹"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                GetProgress().Show(true);
                await FileUtility.Export(path, Project, ExportFormat, CreateLink, Splitter, ShowProgressMessage);


                GetProgress().Close();
            }
        }

        private void CreateLink(string filePath, string distPath)
        {
            if (!distPath.EndsWith(".lnk"))
            {
                distPath += ".lnk";
            }
            IShellLink link = (IShellLink)new ShellLink();

            // setup shortcut information
            //link.SetDescription("My Description");
            link.SetPath(filePath);

            // save it
            IPersistFile file = (IPersistFile)link;
            file.Save(distPath, false);
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        private async void ExportFileButton_Click(object sender, RoutedEventArgs e)
        {
            CommonFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "请选择导出的目标文件夹"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                GetProgress().Show(true);
                await FileUtility.Export(path, Project, ExportFormat, (from, to) => System.IO.File.Copy(from, to, true),
                                    Splitter, ShowProgressMessage);

                GetProgress().Close();
            }
        }

        private async void ExportProjectButton_Click(object sender, RoutedEventArgs e)
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog()
            {
                Title = "请选择导出的位置",
                DefaultFileName = Project.Name
            };
            dialog.Filters.Add(new CommonFileDialogFilter("SQLite数据库", "db"));
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                GetProgress().Show(true);
                await ExportProjectAsync(path, Project.ID);
                GetProgress().Close();
                await new MessageDialog().ShowAsync("导出成功", "导出");
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "请选择文件夹"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == CommonFileDialogResult.Ok)
            {
                Project.RootPath = dialog.FileName;

            }
        }
        public override ClassesPanel GetItemsPanel()
        {
            return null;
        }

        private async void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            flyoutDeleteFiles.Hide();
            GetProgress().Show(false);
            await DeleteFilesOfProjectAsync(Project);
            GetProgress().Close();
        }
    }
}


