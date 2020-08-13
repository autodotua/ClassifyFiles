using ClassifyFiles.Util.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using D = System.IO.Directory;
using F = System.IO.File;
using File = ClassifyFiles.Data.File;
using P = System.IO.Path;

namespace ClassifyFiles.Util
{
    public static class FileIconUtility
    {
        static FileIconUtility()
        {
            encParams = new EncoderParameters(1);
            encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);
            encoder = ImageCodecInfo.GetImageEncoders()
                           .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        }

        public static BitmapImage GetFolderIcon()
        {
            string folderIconPath = Path.Combine(ThumbnailFolderPath, "folder.png");
            if (!F.Exists(folderIconPath))
            {
                ExplorerIcon.GetBitmapFromFolderPath("C:\\Windows", ExplorerIcon.IconSizeEnum.ExtraLargeIcon)
                    .Save(folderIconPath, ImageFormat.Png);
            }
            return new BitmapImage(new Uri(Path.GetFullPath(folderIconPath), UriKind.Absolute));
        }

        public static void UpdateSettings()
        {
            if (Configs.CacheInTempDir)
            {
                ThumbnailFolderPath = P.Combine(P.GetTempPath(), nameof(ClassifyFiles));
            }
            else
            {
                if (!FileUtility.CanWriteInCurrentDirectory() || F.Exists(DbUtility.DbInAppDataFolderMarkerFileName))
                {
                    string path = P.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(ClassifyFiles), "thumb");
                    ThumbnailFolderPath = path;
                }
                else
                {
                    ThumbnailFolderPath = "thumb";
                }
            }
            D.CreateDirectory(P.Combine(ThumbnailFolderPath, "exp"));
            D.CreateDirectory(P.Combine(ThumbnailFolderPath, "win10"));
            D.CreateDirectory(P.Combine(ThumbnailFolderPath, "media"));
        }

        public static string ThumbnailFolderPath { get; set; }
        public static string FFMpegPath => "exe/ffmpeg.exe";

        private static EncoderParameters encParams;
        private static ImageCodecInfo encoder;

        #region 路径

        public static bool HasThumbnail(this File file)
        {
            return F.Exists(file.GetThumbnailPath());
        }

        public static bool HasWin10Icon(this File file)
        {
            return F.Exists(file.GetWin10IconPath());
        }

        public static bool HasExplorerIcon(this File file)
        {
            return F.Exists(file.GetExplorerIconPath());
        }

        public static string GetThumbnailPath(this File file)
        {
            return P.GetFullPath(P.Combine(ThumbnailFolderPath, "media", file.ID + ".jpg"));
        }

        public static string GetWin10IconPath(this File file)
        {
            return P.GetFullPath(P.Combine(ThumbnailFolderPath, "win10", file.ID + ".jpg"));
        }

        public static string GetExplorerIconPath(this File file)
        {
            return P.GetFullPath(P.Combine(ThumbnailFolderPath, "exp", file.FileInfo.Extension.Trim('.').ToLower() + ".png"));
        }

        #endregion 路径

        #region 获取图片

        private static void CreateImageThumbnail(File file)
        {
            using Image image = Image.FromFile(file.GetAbsolutePath());
            using Image thumb = image.GetThumbnailImage(240, (int)(240.0 / image.Width * image.Height), () => false, IntPtr.Zero);
            string guid = Guid.NewGuid().ToString();

            thumb.Save(GetThumbnailPath(file), encoder, encParams);
        }

        private static bool CreateVideoThumbnail(File file)
        {
            var thumbnail = GetThumbnailPath(file);
            var cmd = "  -itsoffset -1  -i " + '"' + file.GetAbsolutePath() + '"' + " -vcodec mjpeg -vframes 1 -an -f rawvideo -vf scale=480:-1 " + '"' + thumbnail + '"';

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = P.GetFullPath(FFMpegPath),
                Arguments = cmd,
                UseShellExecute = false,
                CreateNoWindow = true,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
            };
            Process p = Process.Start(startInfo);
            bool result = p.WaitForExit(20000);
            if (!result)
            {
                p.Kill();
                try
                {
                    F.Delete(thumbnail);
                }
                catch
                {
                }
                return false;
            }
            //string output = p.StandardOutput.ReadToEnd();
            //string error = p.StandardError.ReadToEnd();
            if (!F.Exists(thumbnail))
            {
                return false;
            }
            return true;
        }

        private static async Task CreateWin10ThumbnailAsync(File file)
        {
            var sFile = await StorageFile.GetFileFromPathAsync(file.GetAbsolutePath());
            using StorageItemThumbnail thumb = await sFile.GetThumbnailAsync(ThumbnailMode.SingleItem);

            using var bitmap = Bitmap.FromStream(thumb.AsStream()) as Bitmap;
            Color leftTopColor = bitmap.GetPixel(0, 0);
            if (leftTopColor.A == byte.MaxValue)//非透明色
            {
                bitmap.Save(GetWin10IconPath(file), encoder, encParams);
            }
            else//有透明色
            {
                bitmap.Save(GetWin10IconPath(file), ImageFormat.Png);
            }
        }

        #endregion 获取图片

        #region 为文件生成缩略图

        public static bool TryGenerateThumbnail(File file)
        {
            if (file.IsFolder)
            {
                return false;
            }
            string path = file.GetAbsolutePath();
            if (!F.Exists(path))
            {
                return false;
            }
            if (FileUtility.IsImage(path))
            {
                try
                {
                    CreateImageThumbnail(file);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else if (FileUtility.IsVideo(path))
            {
                try
                {
                    return CreateVideoThumbnail(file);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public static bool TryGenerateExplorerIcon(File file)
        {
            string path = file.GetAbsolutePath();
            if (!F.Exists(path))
            {
                return false;
            }
            try
            {
                string ext = P.GetExtension(path).Replace(".", string.Empty);
                if (file.IsFolder)
                {
                    //string guid = GetGuidFromString("folder").ToString();
                    ////文件夹，统一图标
                    //if (!F.Exists(GetExplorerIconPath(guid)))
                    //{
                    //    var iconPath = GetExplorerIconPath(guid);
                    //    ExplorerIcon.GetBitmapFromFolderPath(path, ExplorerIcon.IconSizeEnum.ExtraLargeIcon).Save(iconPath, ImageFormat.Png);
                    //}
                    //file.IconGUID = guid;
                }
                else if (FileUtility.IsExecutable(path))
                {
                    //程序文件，每个图标都不同
                    string guid = Guid.NewGuid().ToString();
                    var iconPath = GetExplorerIconPath(file);
                    ExplorerIcon.GetBitmapFromFilePath(path, ExplorerIcon.IconSizeEnum.ExtraLargeIcon)
                        .Save(iconPath, ImageFormat.Png);
                }
                else
                {
                    var iconPath = GetExplorerIconPath(file);
                    //其他文件，同一个格式的用同一个图标
                    if (F.Exists(iconPath))
                    {
                    }
                    else
                    {
                        string tempPath = P.GetTempFileName();
                        ExplorerIcon.GetBitmapFromFilePath(path, ExplorerIcon.IconSizeEnum.ExtraLargeIcon).Save(tempPath, ImageFormat.Png);

                        try
                        {
                            F.Move(tempPath, iconPath);
                        }
                        catch { }
                    };
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        internal static void DeleteAllThumbnails()
        {
            foreach (var file in D.EnumerateFiles(ThumbnailFolderPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    F.Delete(file);
                }
                catch
                {
                }
            }
        }

        public static bool TryGenerateWin10Icon(File file)
        {
            string path = file.GetAbsolutePath();
            if (!F.Exists(path) && !D.Exists(path))
            {
                return false;
            }

            try
            {
                CreateWin10ThumbnailAsync(file).Wait();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void TryGenerateAllFileIcons(File file)
        {
            TryGenerateExplorerIcon(file);
            TryGenerateWin10Icon(file);
            TryGenerateThumbnail(file);
        }

        #endregion 为文件生成缩略图
    }
}