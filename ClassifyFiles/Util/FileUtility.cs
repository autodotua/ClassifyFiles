using ClassifyFiles.Data;
using FzLib.Basic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ClassifyFiles.Util.DbUtility;
using Dir = System.IO.Directory;
using F = System.IO.File;
using D = System.IO.Directory;
using FI = System.IO.FileInfo;
using P = System.IO.Path;

namespace ClassifyFiles.Util
{
    public static class FileUtility
    {
        private static readonly IReadOnlyList<string> imgExtensions = new List<string>() {
        "jpg",
        "jpeg",
        "png",
        "tif",
        "tiff",
        "bmp",
        }.AsReadOnly();
        private static readonly IReadOnlyList<string> videoExtensions = new List<string>() {
        "mp4",
        "mkv",
        "avi",
        "mov"
        }.AsReadOnly();
        private static readonly IReadOnlyList<string> programExtensions = new List<string>() {
        "exe",
        "msi"
        }.AsReadOnly();
        public static bool IsImage(string path)
        {
            return imgExtensions.Contains(P.GetExtension(path).RemoveStart(".").ToLower());
        }
        public static bool IsExecutable(string path)
        {
            return programExtensions.Contains(P.GetExtension(path).RemoveStart(".").ToLower());
        }
        public static bool IsVideo(string path)
        {
            return videoExtensions.Contains(P.GetExtension(path).RemoveStart(".").ToLower());
        }
        public static bool IsImage(this FI file)
        {
            return IsImage(file.Extension);
        }

        public static bool IsMatched(FI file, Class c)
        {
            List<List<MatchCondition>> orGroup = new List<List<MatchCondition>>();
            for (int i = 0; i < c.MatchConditions.Count; i++)
            {
                List<MatchCondition> matchConditions = c.MatchConditions.OrderBy(p => p.Index).ToList();
                var mc = matchConditions[i];
                if (i == 0)//首项直接加入列表
                {
                    orGroup.Add(new List<MatchCondition>() { mc });
                    continue;
                }

                //第二项开始，如果是“与”，则加入列表的最后一项列表中；如果是或，则新建一个列表项
                switch (mc.ConnectionLogic)
                {
                    case Logic.And:
                        orGroup.Last().Add(mc);
                        break;
                    case Logic.Or:
                        orGroup.Add(new List<MatchCondition>() { mc });
                        break;
                }
            }
            //此时，分类已经完毕。相邻的“与”为一组，每一组之间为“或”的关系。

            foreach (var andGroup in orGroup)
            {
                bool andResult = true;
                foreach (var and in andGroup)
                {
                    if (!IsMatched(file, and))
                    {
                        andResult = false;
                        break;
                    }
                }
                if (andResult)//如果and组通过了，那么直接通过
                {
                    return true;
                }
            }
            return false;//如果每一组or都不通过，那么只好不通过
        }
        private static bool IsMatched(FI file, MatchCondition mc)
        {

            string value = mc.Value;
            bool? result = mc.Type switch
            {
                MatchType.InFileName => file.Name.ToLower().Contains(value.ToLower()),
                MatchType.InDirName => file.DirectoryName.ToLower().Contains(value.ToLower()),
                MatchType.WithExtension => IsExtensionMatched(file.Extension, value),
                MatchType.InPath => file.FullName.ToLower().Contains(value.ToLower()),
                MatchType.InFileNameWithRegex => Regex.IsMatch(file.Name, value),
                MatchType.InDirNameWithRegex => Regex.IsMatch(file.DirectoryName, value),
                MatchType.InPathWithRegex => Regex.IsMatch(file.FullName, value),
                MatchType.SizeSmallerThan => GetFileSize(value).HasValue ? file.Length <= GetFileSize(value) : (bool?)null,
                MatchType.SizeLargerThan => GetFileSize(value).HasValue ? file.Length >= GetFileSize(value) : (bool?)null,
                MatchType.TimeEarlierThan => file.LastAccessTime <= GetTime(),
                MatchType.TimeLaterThan => file.LastAccessTime >= GetTime(),
                _ => throw new NotImplementedException(),
            };
            if (!result.HasValue)
            {
                return false;
            }
            if (mc.Not)
            {
                result = !result;
            }
            return result.Value;

            DateTime GetTime()
            {
                return DateTime.Parse(mc.Value);
            }

        }
        private static Dictionary<string, string[]> splitedExtensions = new Dictionary<string, string[]>();
        private static bool IsExtensionMatched(string ext, string target)
        {
            string[] splited = null;
            if (!splitedExtensions.ContainsKey(target))
            {
                splited = target.ToLower().Split(',', '|', ' ', '\t');
                splitedExtensions.Add(target, splited);
            }
            else
            {
                splited = splitedExtensions[target];
            }
            return splited.Contains(ext.ToLower().TrimStart('.'));
        }
        public static long? GetFileSize(string value)
        {
            if (long.TryParse(value, out long result))
            {
                return result;
            }
            if (Regex.IsMatch(value.ToUpper(), @"^(?<num>[0-9]+(\.[0-9]+)?) *(?<unit>B|KB|MB|GB|TB)"))
            {
                var match = Regex.Match(value.ToUpper(), @"^(?<num>[0-9]+(\.[0-9]+)?) *(?<unit>B|KB|MB|GB|TB)");
                double num = double.Parse(match.Groups["num"].Value);
                string unit = match.Groups["unit"].Value;
                num *= unit switch
                {
                    "B" => 1.0,
                    "KB" => 1024.0,
                    "MB" => 1024.0 * 1024,
                    "GB" => 1024.0 * 1024 * 1024,
                    "TB" => 1024.0 * 1024 * 1024 * 1024,
                    _ => throw new Exception()
                };
                return Convert.ToInt64(num);
            }
            return null;
        }
        public static File GetFileTree(IEnumerable<File> files)
        {
            Dictionary<File, Queue<string>> fileDirs = new Dictionary<File, Queue<string>>();
            File root = new File() { Dir = "根", Project = files.First().Project };

            foreach (var file in files)
            {
                if (string.IsNullOrEmpty(file.Dir))
                {
                    //位于根目录
                    root.SubFiles.Add(file);
                }
                else
                {
                    var dirs = file.Dir.Split('/', '\\');
                    var current = root;
                    foreach (var dir in dirs)
                    {
                        if (current.SubFiles.FirstOrDefault(p => p.Dir == dir) is File sub)
                        {
                            current = sub;
                        }
                        else
                        {
                            sub = new File() { Dir = dir, Project = file.Project };
                            current.SubFiles.Add(sub);
                            current = sub;
                        }
                    }
                    current.SubFiles.Add(file);
                }
            }
            return root;
        }

        public static T GetFileTree<T>(Project project,
        IEnumerable<T> files,
        Func<File, T> getNewItem,
        Func<T, File> getFile,
        Func<T, IList<T>> getSubFiles,
        Action<T, T> setParent
        ) where T : class
        {
            Dictionary<T, Queue<string>> fileDirs = new Dictionary<T, Queue<string>>();
            T root = getNewItem(new File() { Dir = "", Project = project });

            foreach (var file in files)
            {
                if (string.IsNullOrEmpty(getFile(file).Dir))
                {
                    //位于根目录
                    getSubFiles(root).Add(file);
                }
                else
                {
                    string[] dirs = getFile(file).Dir.Split('/', '\\');
                    if (getFile(file).IsFolder)
                    {
                        //如果是文件夹，那么层级上需要减少最后一个目录级别，
                        //因为最后一个目录级别就是文件夹本身
                        string[] newDirs = new string[dirs.Length - 1];
                        Array.Copy(dirs, newDirs, newDirs.Length);
                        dirs = newDirs;
                    }
                    var current = root;
                    string path = "";
                    foreach (var dir in dirs)
                    {
                        //dir是单个层级的名称，File.Dir是路径名，因此需要进行连接
                        if (path.Length == 0)
                        {
                            path += dir;
                        }
                        else
                        {
                            path += "\\" + dir;
                        }
                        if (getSubFiles(current).FirstOrDefault(p => getFile(p).Dir == path) is T sub)
                        {
                            //如果是已经存在的子目录，那么直接获取
                            current = sub;
                        }
                        else
                        {
                            sub = getNewItem(new File() { Dir = path, Project = project });
                            getSubFiles(current).Add(sub);
                            setParent(sub, current);
                            current = sub;
                        }
                    }
                    getSubFiles(current).Add(file);
                    setParent(file, current);
                }
            }
            return root;
        }

        public static string GetAbsolutePath(this File file, bool dirOnly = false)
        {
            string rootPath = file.Project.RootPath;

            if (dirOnly || file.IsFolder)
            {
                return P.Combine(rootPath, file.Dir);
            }
            return P.Combine(rootPath, file.Dir, file.Name);
        }

        public static void Export(string distFolder,
                                         Project project,
                                         ExportFormat format,
                                         Action<string, string> exportMethod,
                                         string splitter = "-",
                                         Action<File> afterExportAFile = null)
        {
            var classes = ClassUtility.GetClasses(project);
            foreach (var c in classes)
            {
                var files = FileClassUtility.GetFilesByClass(c);

                string folder = P.Combine(distFolder, GetValidFileName(c.Name));
                if (!Dir.Exists(folder))
                {
                    Dir.CreateDirectory(folder);
                }
                switch (format)
                {
                    case ExportFormat.FileName:
                        foreach (var file in files)
                        {
                            string newName = GetUniqueFileName(file.Name, folder);
                            string newPath = P.Combine(folder, newName);
                            exportMethod(file.GetAbsolutePath(), newPath);
                            afterExportAFile?.Invoke(file);
                        }
                        break;
                    case ExportFormat.Path:
                        foreach (var file in files)
                        {
                            string newName = P.Combine(folder, file.Name).Replace(":", splitter).Replace("\\", splitter).Replace("/", splitter);
                            newName = GetUniqueFileName(newName, folder);
                            string newPath = P.Combine(folder, newName);
                            exportMethod(file.GetAbsolutePath(), newPath);
                            afterExportAFile?.Invoke(file);
                        }
                        break;
                    case ExportFormat.Tree:
                        var tree = GetFileTree(files);
                        File root = new File();
                        root.SubFiles = tree.SubFiles;
                        void ExportSub(File file, string currentFolder)
                        {
                            if (file.IsFolder)//是目录
                            {
                                foreach (var sub in file.SubFiles)
                                {
                                    string newFolder = P.Combine(currentFolder, file.Dir);
                                    Dir.CreateDirectory(newFolder);
                                    ExportSub(sub, newFolder);
                                }
                            }
                            else
                            {
                                string newName = GetUniqueFileName(file.Name, folder);
                                string newPath = P.Combine(currentFolder, newName);
                                exportMethod(file.GetAbsolutePath(), newPath);
                                afterExportAFile?.Invoke(file);
                            }
                        }
                        ExportSub(root, folder);
                        break;
                }
            }

        }

        private static string GetValidFileName(string name)
        {
            foreach (char c in P.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        private static string GetUniqueFileName(string name, string dir)
        {
            if (!F.Exists(P.Combine(dir, name)))
            {
                return name;
            }
            string newName;
            int i = 1;
            do
            {
                newName = P.GetFileNameWithoutExtension(name) + $" ({++i})" + P.GetExtension(name);
            } while (F.Exists(P.Combine(dir, newName)));
            return newName;
        }
        public static bool SaveFiles(IEnumerable<File> files)
        {
            files.ForEach(p => db.Entry(p).State = EntityState.Modified);
            return SaveChanges() > 0;
        }

        public static void DeleteFilesRecord(IEnumerable<File> files)
        {
            foreach (var file in files)
            {
                db.Entry(file).State = EntityState.Deleted;
            }
            SaveChanges();
        }
        public static void RecoverFiles(IEnumerable<File> files)
        {
            foreach (var file in files)
            {
                foreach (var fc in db.FileClasses.Where(p => p.Status != FileClassStatus.Auto))
                {
                    if (fc.Status != FileClassStatus.AddManully)
                    {
                        db.Entry(fc).State = EntityState.Deleted;
                    }
                    else if (fc.Status != FileClassStatus.Disabled)
                    {
                        fc.Status = FileClassStatus.Auto;
                        db.Entry(fc).State = EntityState.Modified;
                    }
                }

            }
            SaveChanges();
        }
        public static void DeleteAllThumbnails()
        {
            foreach (var file in db.Files)
            {
                if (file.ThumbnailGUID != null)
                {
                    file.ThumbnailGUID = null;
                    db.Entry(file).State = EntityState.Modified;
                }
                if (file.IconGUID != null)
                {
                    file.IconGUID = null;
                    db.Entry(file).State = EntityState.Modified;
                }     
                if (file.Win10IconGUID != null)
                {
                    file.Win10IconGUID = null;
                    db.Entry(file).State = EntityState.Modified;
                }
            }
          
            int changes = SaveChanges();
        }
        public static void DeleteThumbnails(Project project, Func<File, string> getThumbnailPath)
        {
            var files = db.Files.Where(p => p.ProjectID == project.ID).AsEnumerable();

            foreach (var file in files)
            {
                if (file.ThumbnailGUID != null && file.ThumbnailGUID.Length > 0)
                {
                    string path = getThumbnailPath(file);
                    if (F.Exists(path))
                    {
                        try
                        {
                            F.Delete(path);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    file.ThumbnailGUID = null;
                    db.Entry(file).State = EntityState.Modified;
                }
                else if (file.ThumbnailGUID != null && file.ThumbnailGUID.Length == 0)
                {
                    file.ThumbnailGUID = null;
                    db.Entry(file).State = EntityState.Modified;
                }
                if (file.IconGUID != null && file.IconGUID.Length > 0)
                {
                    file.IconGUID = null;
                    db.Entry(file).State = EntityState.Modified;
                }
            }
            db.SaveChanges();

        }

        public static (int DeleteFromDb,
            int DeleteFromDisk,
            int RemainsCount,
            List<string> FailedFiles) OptimizeThumbnailsAndIcons(string thumbPath)
        {
            int deletedFromDb = 0;
            var files = D.EnumerateFiles(thumbPath).ToDictionary(p => P.GetFileNameWithoutExtension(p));
            foreach (var dbFile in db.Files)
            {
                Check(dbFile, dbFile.ThumbnailGUID, () => dbFile.ThumbnailGUID = null);
                Check(dbFile, dbFile.IconGUID, () => dbFile.IconGUID = null);
            }

            void Check(File item, string guid, Action setNull)
            {
                if (guid != null)
                {
                    if (guid.Length == 0)//重置缩略图状态
                    {
                        setNull();
                        db.Entry(item).State = EntityState.Modified;
                    }
                    else if (files.ContainsKey(guid))
                    {
                        files.Remove(guid);
                    }
                    else//没有物理文件
                    {
                        deletedFromDb++;
                        setNull();
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
            }

            int deletedFromDisk = files.Count;

            List<string> failedFiles = new List<string>();
            foreach (var file in files.Values)
            {
                //删除孤立的缩略图文件
                try
                {
                    F.Delete(file);
                }
                catch (Exception ex)
                {
                    failedFiles.Add(file);
                }
            }
            SaveChanges();
            int remainsCount = db.Files.Where(p => p.ThumbnailGUID != null).Count();
            return (deletedFromDb, deletedFromDisk, remainsCount, failedFiles==null?new List<string>():failedFiles);
        }

        private static bool? canWriteInCurrentDirectory = null;
        public static bool CanWriteInCurrentDirectory()
        {
            if (canWriteInCurrentDirectory == null)
            {
                string path = P.Combine(D.GetCurrentDirectory(), Guid.NewGuid().ToString());
                try
                {
                    using (F.Create(path)) { }
                    F.Delete(path);
                    canWriteInCurrentDirectory = true;
                }
                catch
                {
                    canWriteInCurrentDirectory = false;
                }
            }
            return canWriteInCurrentDirectory.Value;
        }
    }


}