using ClassifyFiles.UI.Model;
using ClassifyFiles.UI.Panel;
using ClassifyFiles.Util;
using System.Collections.Concurrent;
using static ClassifyFiles.Util.FileIconUtility;

namespace ClassifyFiles.UI.Util
{
    /// <summary>
    /// 实时图标/缩略图生成
    /// </summary>
    public static class RealtimeUpdate
    {
        private static ConcurrentDictionary<int, UIFile> generatedThumbnails = new ConcurrentDictionary<int, UIFile>();
        private static ConcurrentDictionary<int, UIFile> generatedIcons = new ConcurrentDictionary<int, UIFile>();
        private static ConcurrentDictionary<int, UIFile> generatedWin10Icons = new ConcurrentDictionary<int, UIFile>();

        public static void ClearCahces()
        {
            generatedIcons.Clear();
            generatedIcons.Clear();
        }

        public static void UpdateFileIcon(UIFile file)
        {
            if (!Configs.AutoThumbnails)
            {
                return;
            }
            bool hasChanged = false;
            if (Configs.ThumbnailStrategy == ThumbnailStrategy.Win10Icon)
            {
                if (!generatedWin10Icons.ContainsKey(file.File.ID)
                && !file.File.HasWin10Icon())
                {
                    if (TryGenerateWin10Icon(file.File))
                    {
                        hasChanged = true;
                    }
                }
            }
            else
            {
                if (Configs.ThumbnailStrategy == ThumbnailStrategy.MediaThumbnailPrefer)
                {
                    //本来是null为没有生成，""为生成失败，后来感觉这样容易出问题，所以干脆生成失败的也再试一次好了
                    if (!generatedThumbnails.ContainsKey(file.File.ID)
                    && !file.File.HasThumbnail())
                    {
                        // generatedThumbnails.TryAdd(file.File.ID, file);
                        if (TryGenerateThumbnail(file.File))
                        {
                            hasChanged = true;
                        }
                    }
                }
                if (Configs.ThumbnailStrategy == ThumbnailStrategy.MediaThumbnailPrefer
                    || Configs.ThumbnailStrategy == ThumbnailStrategy.WindowsExplorerIcon)
                {
                    if (!generatedIcons.ContainsKey(file.File.ID
                        ) && !file.File.HasExplorerIcon())
                    {
                        if (TryGenerateExplorerIcon(file.File))
                        {
                            hasChanged = true;
                        }
                    }
                }
            }

            if (hasChanged)
            {
                file.Display.NotifyIconChanged();
            }
        }

        public static void UpdateDisplay(UIFile file)
        {
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
        }

        private static readonly Converter.DisplayFormatConverter displayNameConverter = new UI.Converter.DisplayFormatConverter();
    }
}