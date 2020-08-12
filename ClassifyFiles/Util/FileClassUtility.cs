using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ClassifyFiles.Util.DbUtility;
using FI = System.IO.FileInfo;

namespace ClassifyFiles.Util
{
    public static class FileClassUtility
    {
        private const int CallbackUpdateMs = 200;

        public static List<Class> GetClassesOfFile(File file)
        {
            using var db = GetNewDb();
            return GetClassesOfFile(db, file);
        }

        public static List<Class> GetClassesOfFile(AppDbContext db, File file)
        {
            Debug.WriteLine("db begin: " + nameof(GetClassesOfFile));
            var result = db.FileClasses
                .Where(p => p.FileID == file.ID && p.Status != FileClassStatus.Disabled)
                .IncludeAll()
                .OrderBy(p => p.Class.Name)
                .Select(p => p.Class)
                .ToList();
            Debug.WriteLine("db end: " + nameof(GetClassesOfFile));
            return result;
        }

        public static List<File> GetFilesByClass(Class c)
        {
            Debug.WriteLine("db begin: " + nameof(GetFilesByClass));
            return db.FileClasses
                 .Where(p => p.ClassID == c.ID)
                 .Where(p => p.Status != FileClassStatus.Disabled)
                 .IncludeAll()
                 .OrderBy(p => p.File.Dir)
                 .ThenBy(p => p.File.Name)
                 .Include(p => p.File.Project)
                 .Select(p => p.File)
                 .ToList();
        }

        public static IEnumerable<KeyValuePair<File, IEnumerable<Class>>> GetFilesWithClassesByClass(Class c)
        {
            Debug.WriteLine("db begin: " + nameof(GetFilesWithClassesByClass));

            var result = db.FileClasses
                .Where(p => p.File.Project == c.Project)
                .Where(p => p.Status != FileClassStatus.Disabled)
                .IncludeAll()//需要包含FileClass.File
                .AsEnumerable()//需要内存分组
                .GroupBy(p => p.File)//按文件分组
                .OrderBy(p => p.Key.Dir)
                .Where(p => p.Any(q => q.ClassID == c.ID))//获取拥有该类的FileClass
                .Select(p => KeyValuePair.Create(p.Key, p.Select(q => q.Class).OrderBy(q => q.Name) as IEnumerable<Class>));
            //因为最后需要转换为UIFile，所以这里不需要直接转换成Dictionary

            //.ToDictionary(p => p.Key, p => p.Select(q => q.Class).ToArray());

            Debug.WriteLine("db end: " + nameof(GetFilesWithClassesByClass));
            return result;
        }

        public static IQueryable<FileClass> IncludeAll(this IQueryable<FileClass> fileClassQueryable)
        {
            return fileClassQueryable.Include(p => p.File).Include(p => p.File);
        }

        public static IReadOnlyList<File> AddFilesToClass(UpdateFilesArgs args)
        {
            Debug.WriteLine("db begin: " + nameof(AddFilesToClass));
            List<File> fs = new List<File>();

            if (args.Files.Any(p => !p.StartsWith(args.Project.RootPath)))
            {
                throw new Exception("文件不在项目目录下");
            }
            int index = 0;
            int count = args.Files.Count;
            DateTime lastCallbackTime = DateTime.MinValue;
            foreach (var path in args.Files)
            {
                File f = new File(new FI(path), args.Project);
                File existedFile = db.Files.FirstOrDefault(p => p.Name == f.Name && p.Dir == f.Dir);
                //文件不存在的话，需要首先新增文件
                if (existedFile == null)
                {
                    args.GenerateThumbnailsMethod?.Invoke(f);
                    //if (args.IncludeThumbnails)
                    //{
                    //    FileUtility.TryGenerateThumbnail(f);
                    //}
                    //if (args.IncludeExplorerIcons)
                    //{
                    //    FileUtility.TryGenerateExplorerIcon(f);
                    //}
                    db.Files.Add(f);
                }
                else
                {
                    var existedFileClass = db.FileClasses.FirstOrDefault(p => p.Class == args.Class && p.File == existedFile);
                    //如果文件已存在，就搜索一下是否存在关系，存在关系就不需要继续了
                    if (existedFileClass != null && existedFileClass.Status != FileClassStatus.Disabled)
                    {
                        goto next;
                    }
                    //存在但被禁用
                    if (existedFileClass != null && existedFileClass.Status == FileClassStatus.Disabled)
                    {
                        fs.Add(existedFile);
                        existedFileClass.Status = FileClassStatus.Auto;
                        db.Entry(existedFileClass).State = EntityState.Modified;
                        goto next;
                    }
                    f = existedFile;
                }
                db.FileClasses.Add(new FileClass(args.Class, f, true));
                fs.Add(f);
            next:
                index++;
                if (args.Callback != null && (DateTime.Now - lastCallbackTime).TotalMilliseconds > CallbackUpdateMs)
                {
                    lastCallbackTime = DateTime.Now;
                    if (!args.Callback(index * 1.0 / count, f))
                    {
                        db.SaveChanges();
                        return null;
                    }
                }
            }
            SaveChanges();
            Debug.WriteLine("db end: " + nameof(AddFilesToClass));

            return fs.AsReadOnly();
        }

        public static int GetFileClassesCount(Project project)
        {
            using var db = GetNewDb();
            return db.FileClasses.Count(p => p.File.Project == project);
        }

        public static void DeleteAllFileClasses(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(DeleteAllFileClasses));
            foreach (var fc in db.FileClasses.Where(p => p.File.ProjectID == project.ID).AsEnumerable())
            {
                db.Entry(fc).State = EntityState.Deleted;
            }
            SaveChanges();
            Debug.WriteLine("db end: " + nameof(DeleteAllFileClasses));
        }

        public static IReadOnlyList<File> AddFilesToClass(IEnumerable<File> files, Class c)
        {
            Debug.WriteLine("db begin: " + nameof(AddFilesToClass));
            List<File> addedFiles = new List<File>();
            foreach (var file in files)
            {
                if (!db.FileClasses.Any(p => p.FileID == file.ID && p.ClassID == c.ID))
                {
                    db.FileClasses.Add(new FileClass(c, file, true));
                    addedFiles.Add(file);
                }
            }
            SaveChanges();
            Debug.WriteLine("db end: " + nameof(AddFilesToClass));
            return addedFiles.AsReadOnly();
        }

        public static bool RemoveFilesFromClass(IEnumerable<File> files, Class c)
        {
            Debug.WriteLine("db begin: " + nameof(RemoveFilesFromClass));
            foreach (var file in files)
            {
                var existed = db.FileClasses
                    .FirstOrDefault(p => p.File == file && p.Class == c && p.Status != FileClassStatus.Disabled);
                var test = db.FileClasses.Where(p => p.FileID == file.ID).ToList();
                if (existed != null)
                {
                    //如果是手动添加的，那么直接删除记录
                    if (existed.Status == FileClassStatus.AddManully)
                    {
                        db.Entry(existed).State = EntityState.Deleted;
                    }
                    else
                    {
                        db.Entry(existed).State = EntityState.Modified;
                    }
                }
            }
            bool result = SaveChanges() > 0;
            Debug.WriteLine("db end: " + nameof(RemoveFilesFromClass));
            return result;
        }

        public static void UpdateFilesOfClasses(UpdateFilesArgs args)
        {
            Debug.WriteLine("db begin: " + nameof(UpdateFilesOfClasses));

            //dbFiles只在需要刷新物理文件时用到，但是因为有两个作用域，所以写在了外层
            HashSet<File> dbFiles = null;//= new HashSet<File>(Queryable.Where(DbUtility.db.Files, (System.Linq.Expressions.Expression<Func<File, bool>>)(p => (bool)(p.Project == args.Project))));

            List<File> files = null;
            if (args.Research)
            {
                dbFiles = new HashSet<File>(db.Files.Where(p => p.ProjectID == args.Project.ID));
                IReadOnlyList<System.IO.FileSystemInfo> diskFiles = FzLib.IO.FileSystem.EnumerateAccessibleFileSystemInfos(args.Project.RootPath);
                if (args.DeleteNonExistentItems)
                {
                    HashSet<string> paths = new HashSet<string>(diskFiles.Select(p => p.FullName));

                    //删除已不存在的文件
                    foreach (var file in dbFiles)
                    {
                        if (!paths.Contains(file.GetAbsolutePath()))
                        {
                            db.Entry(file).State = EntityState.Deleted;
                        }
                    }
                    db.SaveChanges();
                }
                files = diskFiles
                .OfType<FI>()
                .Select(p => new File(p, args.Project)).ToList();
            }
            else
            {
                files = db.Files.Where(p => p.ProjectID == args.Project.ID).Include(p => p.Project).ToList();
            }

            //现在数据库中该项目的所有文件应该都存在相对应的物理文件
            int index = 0;
            int count = files.Count;
            DateTime lastCallbackTime = DateTime.MinValue;
            foreach (var file in files)
            {
                File f = null;// new File(file, args.Project);
                if (args.Research)
                {
                    //先判断一下数据库中是否已存在该文件
                    if (!dbFiles.Contains(file))
                    {
                        f = file;
                        db.Files.Add(f);
                    }
                    else
                    {
                        //如果数据库中存在该文件，则从HashSet中提取该文件
                        if (!dbFiles.TryGetValue(file, out File newF))
                        {
                            //理论上不会进来
                            Debug.Assert(false);
                        }
                        f = newF;
                    }
                    args.GenerateThumbnailsMethod?.Invoke(f);
                    //if (args.IncludeThumbnails)
                    //{
                    //    FileUtility.TryGenerateThumbnail(f);
                    //}
                    //if (args.IncludeExplorerIcons)
                    //{
                    //    FileUtility.TryGenerateExplorerIcon(f);
                    //}
                }
                else
                {
                    f = file;
                }
                if (args.Reclassify)
                {
                    foreach (var c in db.Classes.Where(p => p.ProjectID == args.Project.ID).AsEnumerable())
                    {
                        FileClass fc = IncludeAll(db.FileClasses)
                            .FirstOrDefault(p => p.Class == c && p.File == f);
                        bool isMatched = FileUtility.IsMatched(f.FileInfo, c);
                        if (fc == null && isMatched)
                        {
                            //如果匹配并且不存在，那么新增关系
                            db.Add(new FileClass(c, f, false));
                        }
                        else if (fc != null && !isMatched && fc.Status != FileClassStatus.AddManully)
                        {
                            //如果存在关系但不匹配，那么应该删除已存在的关系
                            //注意，手动删除的关系并不会走到这一步
                            db.Entry(fc).State = EntityState.Deleted;
                        }
                        //其他情况，既不需要增加，也不需要删除
                    }
                }
                index++;
                if (args.Callback != null && (DateTime.Now - lastCallbackTime).TotalMilliseconds > CallbackUpdateMs)
                {
                    lastCallbackTime = DateTime.Now;
                    if (!args.Callback(index * 1.0 / count, f))
                    {
                        db.SaveChanges();
                        return;
                    }
                }
            }

            db.SaveChanges();
            Debug.WriteLine("db end: " + nameof(UpdateFilesOfClasses));
        }
    }
}