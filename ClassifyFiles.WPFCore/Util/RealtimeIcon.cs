using ClassifyFiles.UI.Model;
using ClassifyFiles.Util.Win32.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    /// <summary>
    /// 实时图标/缩略图生成
    /// </summary>
    public static class RealtimeIcon
    {
        private static ConcurrentDictionary<int, UIFile> generatedThumbnails = new ConcurrentDictionary<int, UIFile>();
        private static ConcurrentDictionary<int, UIFile> generatedIcons = new ConcurrentDictionary<int, UIFile>();

        public static void ClearCahces()
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
            });
            return result;
        }
    }
}
