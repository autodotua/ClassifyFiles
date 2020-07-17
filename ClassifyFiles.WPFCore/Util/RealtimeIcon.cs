using ClassifyFiles.UI.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    public static class RealtimeIcon
    {
        private static ConcurrentDictionary<int, UIFile> generatedThumbnails = new ConcurrentDictionary<int, UIFile>();
        private static ConcurrentDictionary<int, UIFile> generatedIcons = new ConcurrentDictionary<int, UIFile>();

        public static  void ClearCahces()
        {
            generatedIcons.Clear();
            generatedIcons.Clear();
        }
        public static async Task<bool> RefreshIcon(UIFile file)
        {
            bool result = false;
            await Task.Run(() =>
            {
                if (Configs.ShowThumbnail)
                {
                    if (!generatedThumbnails.ContainsKey(file.File.ID) && string.IsNullOrEmpty(file.File.ThumbnailGUID) /* file.File.ThumbnailGUID == null && file.File.ThumbnailGUID != ""*/)
                    {
                        generatedThumbnails.TryAdd(file.File.ID, file);
                        if (FileUtility.TryGenerateThumbnail(file.File))
                        {
                            result = true;
                            DbUtility.SetObjectModified(file.File);
                        }
                    }
                }
                if (Configs.ShowExplorerIcon && (!Configs.ShowThumbnail || Configs.ShowThumbnail && string.IsNullOrEmpty(file.File.ThumbnailGUID)))
                {
                    if (!generatedIcons.ContainsKey(file.File.ID) && file.File.IconGUID == null && file.File.IconGUID != "")
                    {
                        generatedIcons.TryAdd(file.File.ID, file);
                        if (FileUtility.TryGenerateExplorerIcon(file.File))
                        {
                            result = true;
                            DbUtility.SetObjectModified(file.File);
                        }
                    }
                }
            });
            return result;

        }

    }
}
