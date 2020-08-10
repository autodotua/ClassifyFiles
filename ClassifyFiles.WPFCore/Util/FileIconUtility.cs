using System;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using F = System.IO.File;
using D = System.IO.Directory;
using P = System.IO.Path;
using ClassifyFiles.Util.Win32;
using Windows.Storage;
using System.IO;
using File = ClassifyFiles.Data.File;
using Windows.Storage.FileProperties;
using System.Windows.Media.Imaging;

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
                if (!FileUtility.CanWriteInCurrentDirectory() ||F.Exists(DbUtility.DbInAppDataFolderMarkerFileName))
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
        private static Guid GetGuidFromString(string str)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(str));
            Guid result = new Guid(hash);
            return result;
        }

        public static string GetThumbnailPath(string guid)
        {
            return P.GetFullPath(P.Combine(ThumbnailFolderPath, "media", guid + ".jpg"));
        }
        public static string GetWin10IconPath(string guid)
        {
            return P.GetFullPath(P.Combine(ThumbnailFolderPath, "win10", guid + ".png"));
        }
        public static string GetExplorerIconPath(string guid)
        {
            return P.GetFullPath(P.Combine(ThumbnailFolderPath, "exp", guid + ".png"));
        }

        #endregion

        #region 获取图片
        private static string CreateImageThumbnail(string img)
        {
            using Image image = Image.FromFile(img);
            using Image thumb = image.GetThumbnailImage(240, (int)(240.0 / image.Width * image.Height), () => false, IntPtr.Zero);
            string guid = Guid.NewGuid().ToString();

            thumb.Save(GetThumbnailPath(guid), encoder, encParams);
            return guid;
        }
        private static string CreateVideoThumbnail(string video)
        {
            string guid = Guid.NewGuid().ToString();
            var thumbnail = GetThumbnailPath(guid);
            var cmd = "  -itsoffset -1  -i " + '"' + video + '"' + " -vcodec mjpeg -vframes 1 -an -f rawvideo -vf scale=480:-1 " + '"' + thumbnail + '"';

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
                return null;
            }
            //string output = p.StandardOutput.ReadToEnd();
            //string error = p.StandardError.ReadToEnd();
            if (!F.Exists(thumbnail))
            {
                return null;
            }
            return guid;
        }
        private static async Task<string> CreateWin10ThumbnailAsync(string path)
        {
            var sFile = await StorageFile.GetFileFromPathAsync(path);
            using StorageItemThumbnail thumb = await sFile.GetThumbnailAsync(ThumbnailMode.SingleItem);

            using var bitmap = Bitmap.FromStream(thumb.AsStream()) as Bitmap;
            string guid = Guid.NewGuid().ToString();

            bitmap.Save(GetWin10IconPath(guid), ImageFormat.Png);
            return guid;

        }

        #endregion


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
                    file.ThumbnailGUID = CreateImageThumbnail(path);
                    return true;
                }
                catch (Exception ex)
                {
                    file.ThumbnailGUID = "";
                    return false;
                }
            }
            else if (FileUtility.IsVideo(path))
            {
                try
                {
                    file.ThumbnailGUID = CreateVideoThumbnail(path);
                    return true;
                }
                catch (Exception ex)
                {
                    file.ThumbnailGUID = "";
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
                    string guid = GetGuidFromString("folder").ToString();
                    //文件夹，统一图标
                    if (!F.Exists(GetExplorerIconPath(guid)))
                    {
                        var iconPath = GetExplorerIconPath(guid);
                        ExplorerIcon.GetBitmapFromFolderPath(path, ExplorerIcon.IconSizeEnum.ExtraLargeIcon).Save(iconPath, ImageFormat.Png);
                    }
                    file.IconGUID = guid;
                }
                else if (FileUtility.IsExecutable(path))
                {
                    //程序文件，每个图标都不同
                    string guid = Guid.NewGuid().ToString();
                    var iconPath = GetExplorerIconPath(guid);
                    ExplorerIcon.GetBitmapFromFilePath(path, ExplorerIcon.IconSizeEnum.ExtraLargeIcon).Save(iconPath, ImageFormat.Png);
                    file.IconGUID = guid;
                }
                else
                {
                    string guid = GetGuidFromString(ext).ToString();
                    var iconPath = GetExplorerIconPath(guid);
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
                    }
                    file.IconGUID = guid;
                }
            }
            catch (Exception ex)
            {
                file.IconGUID = "";
            }
            return false;
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
                file.Win10IconGUID = CreateWin10ThumbnailAsync(path).Result;
                return true;
            }
            catch (Exception ex)
            {
                file.Win10IconGUID = "";
                return false;
            }
        }
        public static void TryGenerateAllFileIcons(File file)
        {
            TryGenerateExplorerIcon(file);
            TryGenerateWin10Icon(file);
            TryGenerateThumbnail(file);
        }
        #endregion
    }
}
