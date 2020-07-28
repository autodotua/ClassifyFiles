using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FI = System.IO.FileInfo;
using DI = System.IO.DirectoryInfo;
using SO = System.IO.SearchOption;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class FileClassUtility
    {
        public static List<Class> GetClassesOfFile(File file)
        {
            using var db = GetNewDb();
            return GetClassesOfFile(db, file);
        }
        public static List<Class> GetClassesOfFile(AppDbContext db, File file)
        {
            //Debug.WriteLine("db: " + nameof(GetClassesOfFileAsync));
            return db.FileClasses
                .Where(p => p.FileID == file.ID && !p.Disabled)
                .IncludeAll()
                .OrderBy(p => p.Class.Name)
                .Select(p => p.Class)
                .ToList();
        }

        public static List<File> GetFilesByClass(Class c)
        {
            Debug.WriteLine("db: " + nameof(GetFilesByClass));
            return db.FileClasses
                 .Where(p => p.ClassID == c.ID)
                 .Where(p => p.Disabled == false)
                 .IncludeAll()
                 .OrderBy(p => p.File.Dir)
                 .ThenBy(p => p.File.Name)
                 .Include(p => p.File.Project)
                 .Select(p => p.File)
                 .ToList();
        }
        public static Dictionary<File, Class[]> GetFilesWithClassesByClass(Class c)
        {
            Debug.WriteLine("db: " + nameof(GetFilesWithClassesByClass));
            var tempFiles = (from f in db.Files
                             join fc in db.FileClasses on f.ID equals fc.FileID
                             select fc)
                             .Include(p => p.File)
                             .ThenInclude(p => p.Project)
                             .Include(p => p.Class)
                             .ToList();
            return tempFiles
                 .GroupBy(p => p.File)
                 .Where(p => p.Any(p => p.ClassID == c.ID))
                 .ToDictionary(p => p.Key, p => p.Select(q => q.Class).ToArray());

        }
        public static IQueryable<FileClass> IncludeAll(this IQueryable<FileClass> fileClassQueryable)
        {
            return fileClassQueryable.Include(p => p.File).Include(p => p.File);
        }

        public static IReadOnlyList<File> AddFilesToClass(UpdateFilesArgs args)
        {
            List<File> fs = new List<File>();

            if (args.Files.Any(p => !p.StartsWith(args.Project.RootPath)))
            {
                throw new Exception("文件不在项目目录下");
            }
            int index = 0;
            int count = args.Files.Count;
            foreach (var path in args.Files)
            {
                File f = new File(new FI(path), args.Project);
                File existedFile = db.Files.FirstOrDefault(p => p.Name == f.Name && p.Dir == f.Dir);
                //文件不存在的话，需要首先新增文件
                if (existedFile == null)
                {
                    if (args.IncludeThumbnails)
                    {
                        FileUtility.TryGenerateThumbnail(f);
                    }
                    if (args.IncludeExplorerIcons)
                    {
                        FileUtility.TryGenerateExplorerIcon(f);
                    }
                    db.Files.Add(f);
                }
                else
                {
                    var existedFileClass = db.FileClasses.FirstOrDefault(p => p.Class == args.Class && p.File == existedFile);
                    //如果文件已存在，就搜索一下是否存在关系，存在关系就不需要继续了
                    if (existedFileClass != null && !existedFileClass.Disabled)
                    {
                        goto next;
                    }
                    //存在但被禁用
                    if (existedFileClass != null && existedFileClass.Disabled)
                    {
                        fs.Add(existedFile);
                        existedFileClass.Disabled = false;
                        db.Entry(existedFileClass).State = EntityState.Modified;
                        goto next;
                    }
                    f = existedFile;
                }
                db.FileClasses.Add(new FileClass(args.Class, f, true));
                fs.Add(f);
            next:
                if (args.Callback != null)
                {
                    if (!args.Callback((++index * 1.0) / count, f))
                    {
                        db.SaveChanges();
                        return null;
                    }
                }
            }
            SaveChanges();
            return fs.AsReadOnly();

        }

        public static int GetFileClassesCount(Project project)
        {
            using var db = GetNewDb();
            return db.FileClasses.Count(p => p.File.Project == project);
        }

        public static void DeleteAllFileClasses(Project project)
        {
            foreach (var fc in db.FileClasses.Where(p => p.File.ProjectID == project.ID).AsEnumerable())
            {
                db.Entry(fc).State = EntityState.Deleted;
            }
            SaveChanges();
        }

        public static IReadOnlyList<File> AddFilesToClass(IEnumerable<File> files, Class c)
        {
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
            return addedFiles.AsReadOnly();
        }
        public static bool RemoveFilesFromClass(IEnumerable<File> files, Class c)
        {
            foreach (var file in files)
            {
                var existed = db.FileClasses
                    .FirstOrDefault(p => p.File == file && p.Class == c && !p.Disabled);
                var test = db.FileClasses.Where(p => p.FileID == file.ID).ToList();
                if (existed != null)
                {
                    existed.Disabled = true;
                    db.Entry(existed).State = EntityState.Modified;
                }
            }
            return SaveChanges() > 0;
        }
        public static void UpdateFilesOfClasses(UpdateFilesArgs args)
        {
            //dbFiles只在需要刷新物理文件时用到，但是因为有两个作用域，所以写在了外层
            HashSet<File> dbFiles = null;//= new HashSet<File>(Queryable.Where(DbUtility.db.Files, (System.Linq.Expressions.Expression<Func<File, bool>>)(p => (bool)(p.Project == args.Project))));

            List<File> files = null;
            if (args.Research)
            {
                dbFiles = new HashSet<File>(db.Files.Where(p => p.ProjectID == args.Project.ID));
                List<System.IO.FileSystemInfo> diskFiles = new DI(args.Project.RootPath).EnumerateFileSystemInfos("*", SO.AllDirectories).ToList();
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
                    if (args.IncludeThumbnails)
                    {
                        FileUtility.TryGenerateThumbnail(f);
                    }
                    if (args.IncludeExplorerIcons)
                    {
                        FileUtility.TryGenerateExplorerIcon(f);
                    }
                }
                else
                {
                    f = file;
                }
                if (args.Reclassify)
                {
                    foreach (var c in db.Classes.Where(p => p.ProjectID == args.Project.ID).ToList())
                    {
                        FileClass fc = IncludeAll(db.FileClasses).FirstOrDefault(p => p.Class == c && p.File == f);
                        bool isMatched = FileUtility.IsMatched(f.FileInfo, c);
                        if (fc == null && isMatched)
                        {
                            //如果匹配并且不存在，那么新增关系
                            db.Add(new FileClass(c, f, false));
                        }
                        else if (fc != null && !isMatched)
                        {
                            //如果存在关系但不匹配，那么应该删除已存在的关系
                            //注意，手动删除的关系并不会走到这一步
                            db.Entry(fc).State = EntityState.Deleted;
                        }
                        //其他情况，既不需要增加，也不需要删除
                    }
                }
                if (args.Callback != null)
                {
                    if (!args.Callback((++index * 1.0) / count, f))
                    {
                        db.SaveChanges();
                        return;
                    }
                }
            }
            db.SaveChanges();
        }
    }
}
