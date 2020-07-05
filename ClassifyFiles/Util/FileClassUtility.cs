using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DI = System.IO.DirectoryInfo;
using SO = System.IO.SearchOption;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class FileClassUtility
    {
        public static Task<List<Class>> GetClassesOfFileAsync(int fileID)
        {
            //Debug.WriteLine("db: " + nameof(GetClassesOfFileAsync));
            return db.FileClasses
                .Where(p => p.FileID == fileID)
                .IncludeAll()
                .Select(p => p.Class).ToListAsync();
        }
        public async static Task<List<File>> GetFilesByClassAsync(int classID)
        {
            Debug.WriteLine("db: " + nameof(GetFilesByClassAsync));
            var files = db.FileClasses
                .Where(p => p.Class.ID == classID)
                .Where(p => p.Disabled == false)
                .IncludeAll()
                .OrderBy(p => p.File.Dir)
                .ThenBy(p => p.File.Name)
                .Include(p => p.File.Project)
                .Select(p => p.File);

            return await files.ToListAsync();
        }
        public static IQueryable<File> GetFilesByClass(int classID)
        {
            Debug.WriteLine("db: " + nameof(GetFilesByClass));
            return db.FileClasses
                 .Where(p => p.Class.ID == classID)
                 .Where(p => p.Disabled == false)
                 .IncludeAll()
                 .OrderBy(p => p.File.Dir)
                 .ThenBy(p => p.File.Name)
                 .Include(p => p.File.Project)
                 .Select(p => p.File);
        }

        public static IQueryable<FileClass> IncludeAll(this IQueryable<FileClass> fileClassQueryable)
        {
            return fileClassQueryable.Include(p => p.File).Include(p => p.File);
        }

        public async static Task<IReadOnlyList<File>> AddFilesToClass(IEnumerable<string> files, Class c, bool includeThumbnails)
        {
            List<File> fs = new List<File>();
            //var existed = await GetFilesByProjectAsync(c.Project.ID);
            await Task.Run((Action)(() =>
            {
                if (files.Any(p => !p.StartsWith(c.Project.RootPath)))
                {
                    throw new Exception("文件不在项目目录下");
                }
                foreach (var path in files)
                {
                    File f = new File(new System.IO.FileInfo(path), c.Project);
                    File existedFile = Queryable.FirstOrDefault<File>(DbUtility.db.Files, (System.Linq.Expressions.Expression<Func<File, bool>>)(p => (bool)(p.Name == f.Name && p.Dir == f.Dir)));
                    //文件不存在的话，需要首先新增文件
                    if (existedFile == null)
                    {
                        if (includeThumbnails)
                        {
                            FileUtility.TryGenerateThumbnail(f);
                        }
                        DbUtility.db.Files.Add(f);
                    }
                    else
                    {
                        //如果文件已存在，就搜索一下是否存在关系，存在关系就不需要继续了
                        if (Queryable.Any<FileClass>(DbUtility.db.FileClasses, (System.Linq.Expressions.Expression<Func<FileClass, bool>>)(p => (bool)(p.Class == c && p.File == existedFile))))
                        {
                            continue;
                        }
                        f = existedFile;
                    }
                    DbUtility.db.FileClasses.Add(new FileClass(c, f, true));
                    fs.Add(f);
                }
                DbUtility.db.SaveChanges();
            }));
            return fs.AsReadOnly();

        }

        public static Task<int> GetFileClassesCountAsync(Project project)
        {
            return db.FileClasses.CountAsync(p => p.File.Project == project);
        }

        public async static Task AddFilesToClass(IEnumerable<File> files, Class c)
        {
            foreach (var file in files)
            {
                if (!await db.FileClasses.AnyAsync(p => p.FileID == file.ID && p.ClassID == c.ID))
                {
                    db.FileClasses.Add(new FileClass(c, file, true));
                }
            }
            await db.SaveChangesAsync();
        }
        public async static Task RemoveFilesFromClass(IEnumerable<File> files, Class c)
        {
            foreach (var file in files)
            {
                var existed = await db.FileClasses.FirstOrDefaultAsync(p => p.FileID == file.ID);
                if (existed != null)
                {
                    existed.Disabled = true;
                    db.Entry(existed).State = EntityState.Modified;
                }
            }
            await db.SaveChangesAsync();
        }
        public static Task UpdateFilesOfClassesAsync(UpdateFilesArgs args)
        {
            return Task.Run(() =>
            {
                //dbFiles只在需要刷新物理文件时用到，但是因为有两个作用域，所以写在了外层
                HashSet<File> dbFiles = null;//= new HashSet<File>(Queryable.Where(DbUtility.db.Files, (System.Linq.Expressions.Expression<Func<File, bool>>)(p => (bool)(p.Project == args.Project))));

                List<File> files = null;
                if (args.Research)
                {
                    dbFiles = new HashSet<File>(db.Files.Where(p => p.ProjectID == args.Project.ID));
                    List<System.IO.FileInfo> diskFiles = new DI(args.Project.RootPath).EnumerateFiles("*", SO.AllDirectories).ToList();
                    HashSet<string> paths = new HashSet<string>(diskFiles.Select(p => p.FullName));

                    //删除已不存在的文件
                    foreach (var file in dbFiles)
                    {
                        if (!paths.Contains(file.GetAbsolutePath()))
                        {
                            db.Entry<File>(file).State = EntityState.Deleted;
                        }
                    }
                    db.SaveChanges();
                    files = diskFiles.Select(p => new File(p, args.Project)).ToList();
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
                            DbUtility.db.Files.Add(f);
                            if (args.IncludeThumbnails)
                            {
                                FileUtility.TryGenerateThumbnail(f);
                            }
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
                            bool isMatched = FileUtility.IsMatched(f.GetFileInfo(), c);
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
            });
        }
    }
}
