using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using ClassifyFiles.Util;
using ClassifyFiles.Util.Win32.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.UI.Util
{
    /// <summary>
    /// 实时图标/缩略图生成
    /// </summary>
    public static class RealtimeUpdate
    {
        private static ConcurrentDictionary<int, UIFile> generatedThumbnails = new ConcurrentDictionary<int, UIFile>();
        private static ConcurrentDictionary<int, UIFile> generatedIcons = new ConcurrentDictionary<int, UIFile>();

        public static void ClearCahces()
        {
            generatedIcons.Clear();
            generatedIcons.Clear();
        }
        public static bool UpdateDisplay(UIFile file)
        {
            bool result = false;

            if (Configs.AutoThumbnails)
            {
                if (Configs.ShowThumbnail)
                {
                    //本来是null为没有生成，""为生成失败，后来感觉这样容易出问题，所以干脆生成失败的也再试一次好了
                    if (!generatedThumbnails.ContainsKey(file.File.ID)
                    && string.IsNullOrEmpty(file.File.ThumbnailGUID))
                    {
                        generatedThumbnails.TryAdd(file.File.ID, file);
                        if (FileUtility.TryGenerateThumbnail(file.File))
                        {
                            result = true;
                            DbUtility.SetObjectModified(file.File);
                        }
                    }
                }
                if (Configs.ShowExplorerIcon)
                {
                    if (!generatedIcons.ContainsKey(file.File.ID
                        ) && string.IsNullOrEmpty(file.File.IconGUID))
                    {
                        generatedIcons.TryAdd(file.File.ID, file);
                        if (FileUtility.TryGenerateExplorerIcon(file.File))
                        {
                            result = true;
                            DbUtility.SetObjectModified(file.File);
                        }
                    }
                }
            }
            if (file.Class != null)
            {
                if (!string.IsNullOrWhiteSpace(file.Class.DisplayNameFormat))
                {
                    file.Display.DisplayName = displayNameConverter.Convert(new object[] { file.Display, file.Class }, null, 0, null) as string ?? "";
                }
                if (FilesViewer.Main != null && FilesViewer.Main.CurrentFileView == ClassifyFiles.Enum.FileView.Detail)
                {
                    //只有详细视图会显示下面的内容
                    if (!string.IsNullOrWhiteSpace(file.Class.DisplayProperty1) && file.Display.DisplayProperty1 == null)
                    {
                        file.Display.DisplayProperty1 = displayNameConverter.Convert(new object[] { file.Display, file.Class }, null, 1, null) as string ?? "";
                    }
                    if (!string.IsNullOrWhiteSpace(file.Class.DisplayProperty2) && file.Display.DisplayProperty2 == null)
                    {
                        file.Display.DisplayProperty2 = displayNameConverter.Convert(new object[] { file.Display, file.Class }, null, 2, null) as string ?? "";
                    }
                    if (!string.IsNullOrWhiteSpace(file.Class.DisplayProperty3) && file.Display.DisplayProperty3 == null)
                    {
                        file.Display.DisplayProperty3 = displayNameConverter.Convert(new object[] { file.Display, file.Class }, null, 3, null) as string ?? "";
                    }
                }
            };
            return result;
        }

        private static ClassifyFiles.UI.Converter.DisplayFormatConverter displayNameConverter = new UI.Converter.DisplayFormatConverter();
    }
}
