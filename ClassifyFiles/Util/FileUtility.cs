using ClassifyFiles.Data;
using System;
using System.Collections.Generic;
using Dir = System.IO.Directory;
using F = System.IO.File;
using P = System.IO.Path;
using FI = System.IO.FileInfo;
using DI = System.IO.DirectoryInfo;
using SO = System.IO.SearchOption;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace ClassifyFiles.Util
{
    public static class FileUtility
    {
        public static readonly IReadOnlyList<string> imgExtensions = new List<string>() {
        "jpg",
        "jpeg",
        "png",
        "tif",
        "tiff",
        "bmp",
        }.AsReadOnly();
        public async static Task<Dictionary<Class, List<File>>> GetFilesAsync(DI dir, IEnumerable<Class> classes, bool includeThumbnails, Action<double> percentCallback = null)
        {
            Dictionary<Class, List<File>> classFiles = new Dictionary<Class, List<File>>();
            await Task.Run(async () =>
            {
                foreach (var c in classes)
                {
                    classFiles.Add(c, new List<File>());
                }
                var files = dir.EnumerateFiles("*", SO.AllDirectories).ToList();
                int index = 0;
                int count = files.Count;
                foreach (var file in files)
                {
                    byte[] thumb = null;
                    foreach (var c in classes)
                    {
                        if (IsMatched(file, c))
                        {
                            File f = new File(file, dir, c);
                            if (includeThumbnails)
                            {
                                if (thumb == null)
                                {
                                    await GenerateThumbnailAsync(f, dir);
                                    thumb = f.Thumbnail;
                                }
                                else
                                {
                                    f.Thumbnail = thumb;
                                }
                            }
                            classFiles[c].Add(f);
                        }
                    }
                    percentCallback?.Invoke((++index * 1.0) / count);
                }
            });
            return classFiles;
        }

        public static Task GenerateThumbnailAsync(File file, DI dir)
        {
            return Task.Run(() =>
            {
                string path = file.GetAbsolutePath(dir.FullName);
                if (imgExtensions.Contains(System.IO.Path.GetExtension(path).ToLower().Trim('.')))
                {
                    try
                    {
                        using Image image = Image.FromFile(path);

                        using Image thumb = image.GetThumbnailImage(240, (int)(240.0 / image.Width * image.Height), () => false, IntPtr.Zero);
                        //string guid = Guid.NewGuid().ToString();
                        //file.ImageID = guid;
                        //string thumbPath = System.IO.Path.Combine(dir, guid + ".jpg");
                        //thumb.Save(thumbPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        using var ms = new System.IO.MemoryStream();
                        thumb.Save(ms, ImageFormat.Jpeg);
                        file.Thumbnail = ms.ToArray();
                    }
                    catch (Exception ex)
                    {

                    }
                }

            });

        }
        private static bool IsMatched(FI file, Class c)
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
                MatchType.InFileName => file.Name.Contains(value),
                MatchType.InDirName => file.DirectoryName.Contains(value),
                MatchType.WithExtension => file.Extension.ToLower().Replace(".","")==value.ToLower(),
                MatchType.InPath => file.FullName.Contains(value),
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
            File root = new File() { Name = "根" };

            foreach (var file in files)
            {
                var dirs = file.Dir.Split('/', '\\');
                var current = root;
                foreach (var dir in dirs)
                {
                    var sub = current.SubFiles.FirstOrDefault(p => p.Name == dir);
                    if (sub != null)
                    {
                        current = sub;
                    }
                    else
                    {
                        sub = new File() { Name = dir };
                        current.SubFiles.Add(sub);
                        current = sub;
                    }
                }
                current.SubFiles.Add(file);
            }
            return root;
        }

        public static string GetAbsolutePath(this File file, string rootPath, bool dirOnly = false)
        {
            if (dirOnly)
            {
                return System.IO.Path.Combine(rootPath, file.Dir);
            }
            return System.IO.Path.Combine(rootPath, file.Dir, file.Name);
        }

        public async static Task Export(string distFolder,
                                         Project project,
                                         ExportFormat format,
                                         Action<string, string> exportMethod,
                                         string splitter = "-",
                                         Action<File> afterExportAFile = null)
        {
            var classes = await DbUtility.GetClassesAsync(project);
            foreach (var c in classes)
            {
                var files = await DbUtility.GetFilesAsync(c);
                await Task.Run(() =>
                {
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
                                exportMethod(file.GetAbsolutePath(project.RootPath), newPath);
                                afterExportAFile?.Invoke(file);
                            }
                            break;
                        case ExportFormat.Path:
                            foreach (var file in files)
                            {
                                string newName = P.Combine(folder, file.Name).Replace(":", splitter).Replace("\\", splitter).Replace("/", splitter);
                                newName = GetUniqueFileName(newName, folder);
                                string newPath = P.Combine(folder, newName);
                                exportMethod(file.GetAbsolutePath(project.RootPath), newPath);
                                afterExportAFile?.Invoke(file);
                            }
                            break;
                        case ExportFormat.Tree:
                            var tree = GetFileTree(files);
                            void ExportSub(File file, string currentFolder)
                            {
                                if (string.IsNullOrEmpty(file.Dir))//是目录
                                {
                                    foreach (var sub in file.SubFiles)
                                    {
                                        string newFolder = P.Combine(currentFolder, file.Name);
                                        Dir.CreateDirectory(newFolder);
                                        ExportSub(sub, newFolder);
                                    }
                                }
                                else
                                {
                                    string newName = GetUniqueFileName(file.Name, folder);
                                    string newPath = P.Combine(currentFolder, newName);
                                    exportMethod(file.GetAbsolutePath(project.RootPath), newPath);
                                    afterExportAFile?.Invoke(file);
                                }
                            }
                            ExportSub(tree, folder);
                            break;
                    }
                });
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
    }
    public enum ExportFormat
    {
        [Description("文件名")]
        FileName,
        [Description("路径")]
        Path,
        [Description("树型")]
        Tree
    }
}