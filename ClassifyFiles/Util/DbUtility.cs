using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DI = System.IO.DirectoryInfo;
using SO = System.IO.SearchOption;


namespace ClassifyFiles.Util
{
    public static class DbUtility
    {
        public static string DbPath { get; private set; } = "data.db";
        private static AppDbContext db;
        private static AppDbContext Db
        {
            get
            {
                if (db == null)
                {
                    db = new AppDbContext(DbPath);
                }
                return db;
            }
        }
        public static async Task<List<Project>> GetProjectsAsync()
        {
            Debug.WriteLine("db: " + nameof(GetProjectsAsync));

            return await Db.Projects.ToListAsync();
        }
        public static async Task<List<Class>> GetClassesAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(GetClassesAsync));

            List<Class> classes = await db.Classes
                .Where(p => p.Project == project)
                .Include(p => p.MatchConditions)
                .ToListAsync();

            return classes;
            //return classes.Where(p => p.Parent == null).ToList();

        }

        public static Task SaveChangesAsync()
        {
            return Db.SaveChangesAsync();
        }
        public static async Task SaveClassAsync(Class c)
        {
            Debug.WriteLine("db: " + nameof(SaveClassAsync));

            if (await Db.Classes.AnyAsync(p => p.ID == c.ID))
            {
                Db.Entry(c).State = EntityState.Modified;
                await Db.SaveChangesAsync();
            }
        }
        public static async Task UpdateProjectAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(UpdateProjectAsync));

            Db.Entry(project).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        public static async Task DeleteProjectAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(DeleteProjectAsync));

            Db.Entry(project).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        public static Task DeleteFilesOfProjectAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(DeleteFilesOfProjectAsync));

            return Db.Database.ExecuteSqlRawAsync("delete from Files where ProjectID = " + project.ID);
            //await db.SaveChangesAsync();
        }

        public static async Task<Project> AddProjectAsync() { 
            Debug.WriteLine("db: " + nameof(AddProjectAsync));
        
            Project project = new Project() { Name = "未命名" };
            Db.Projects.Add(project);
            await Db.SaveChangesAsync();
            return project;
        }
        public static async Task<Class> AddClassAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(AddClassAsync));

            Class c = new Class() { Project = project, Name = "未命名类" };
            Db.Classes.Add(c);
            await db.SaveChangesAsync();
            return c;
        }
        public static async Task DeleteClassAsync(Class c) { 
            Debug.WriteLine("db: " + nameof(DeleteClassAsync));
            Db.Entry(c).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }

        public static Task<List<Class>> GetClassesOfFileAsync(int fileID)
        {
            Debug.WriteLine("db: " + nameof(GetClassesOfFileAsync));
            return Db.FileClasses
                .Where(p => p.FileID == fileID)
                .IncludeAll()
                .Select(p => p.Class).ToListAsync();
        }
        public async static Task<List<File>> GetFilesByClassAsync(int classID)
        {
            Debug.WriteLine("db: " + nameof(GetFilesByClassAsync));
            var files = Db.FileClasses
                .Where(p => p.Class.ID == classID)
                .Where(p=>p.Disabled==false)
                .IncludeAll()
                .OrderBy(p => p.File.Dir)
                .ThenBy(p => p.File.Name)
                .Include(p => p.File.Project)
                .Select(p => p.File);

            return await files.ToListAsync();
        }
        public async static Task<List<File>> GetFilesByProjectAsync(int projectID)
        {
            Debug.WriteLine("db: " + nameof(GetFilesByProjectAsync));
            var files = Db.Files.Where(p => p.Project.ID == projectID).Include(p=>p.Project);
            return await files.ToListAsync();
        }


        public async static Task ExportProject(string path, int projectID)
        {
            Project project = await Db.Projects.Where(p => p.ID == projectID)
               .Include(p => p.Classes).ThenInclude(p => p.MatchConditions)
               .FirstOrDefaultAsync();

            var newDb = new AppDbContext(path);
            newDb.Database.EnsureDeleted();
            newDb.Database.EnsureCreated();

            newDb.Projects.Add(project);
            await newDb.SaveChangesAsync();
            await newDb.DisposeAsync();
        }
        public static Task ExportAll(string path)
        {
            return Task.Run(() =>
            {
                System.IO.File.Copy(DbPath, path);
            });
        }
        public static IQueryable<FileClass> IncludeAll(this IQueryable<FileClass> fileClassQueryable)
        {
            return fileClassQueryable.Include(p => p.File).Include(p => p.File);
        }
        public async static Task<Project[]> Import(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new System.IO.FileNotFoundException();
            }
            var importDb = new AppDbContext(path);
            List<Project> projects = new List<Project>();
            foreach (var projectID in await importDb.Projects.Select(p => p.ID).ToListAsync())
            {
                Project project = await importDb.Projects.Where(p => p.ID == projectID)
              .Include(p => p.Classes).ThenInclude(p => p.MatchConditions)
              .FirstAsync();
                await Task.Run(() =>
                {
                    projects.Add(project);
                    project.ID = 0;
                    foreach (var c in project.Classes)
                    {
                        c.ID = 0;
                        foreach (var file in Db.FileClasses.Where(p => p.Class == c).IncludeAll().Select(p => p.File))
                        {
                            file.ID = 0;
                        }
                        foreach (var m in c.MatchConditions)
                        {
                            m.ID = 0;
                        }
                    }
                    Db.Add(project);
                });
            }
            await Db.SaveChangesAsync();
            await importDb.DisposeAsync();
            return projects.ToArray();
        }

        public async static Task<IReadOnlyList<File>> AddFilesToClass(IEnumerable<string> files, Class c, bool includeThumbnails)
        {
            var existed = await GetFilesByClassAsync(c.ID);
            List<File> fs = new List<File>();
            foreach (var path in files)
            {
                File f = new File(new System.IO.FileInfo(path), c.Project);
                //重写了HashCode，因此可以这样来判断
                if (!existed.Contains(f))
                {
                    if (includeThumbnails)
                    {
                        FileUtility.TryGenerateThumbnail(f);
                    }
                    Db.Files.Add(f);
                    Db.FileClasses.Add(new FileClass(c, f, true));
                    fs.Add(f);
                }
            }
            await Db.SaveChangesAsync();
            return fs.AsReadOnly();

        }
        public async static Task AddFilesToClass(IEnumerable<File> files, Class c)
        {
            foreach (var file in files)
            {
                if (!await Db.FileClasses.AnyAsync(p => p.FileID == file.ID && p.ClassID==c.ID))
                {
                    Db.FileClasses.Add(new FileClass(c, file, true));
                }
            }
            await Db.SaveChangesAsync();
        }
        public async static Task RemoveFilesFromClass(IEnumerable<File> files, Class c)
        {
            foreach (var file in files)
            {
                var existed =await Db.FileClasses.FirstOrDefaultAsync(p => p.FileID == file.ID);
                if (existed!=null)
                {
                    existed.Disabled = true;
                    Db.Entry(existed).State = EntityState.Modified;
                }
            }
            await Db.SaveChangesAsync();
        }
        public static async Task UpdateFilesOfClassesAsync(UpdateFilesArgs args)
        {
            await Task.Run(() =>
            {
                var files = new DI(args.Project.RootPath).EnumerateFiles("*", SO.AllDirectories).ToList();
                HashSet<string> paths = new HashSet<string>(files.Select(p => p.FullName));
                HashSet<File> dbFiles = new HashSet<File>(Db.Files.Where(p => p.Project == args.Project));

                //删除已不存在的文件
                foreach (var file in dbFiles)
                {
                    if (!paths.Contains(file.GetAbsolutePath()))
                    {
                        Db.Entry(file).State = EntityState.Deleted;
                    }
                }
                Db.SaveChanges();

                //现在数据库中该项目的所有文件应该都存在相对应的物理文件
                int index = 0;
                int count = files.Count;
                foreach (var file in files)
                {
                    File f = new File(file, args.Project);
                    //重写了HashCode，因此可以这样来判断
                    if (!dbFiles.Contains(f))
                    {
                        Db.Files.Add(f);
                        if (args.IncludeThumbnails)
                        {
                            FileUtility.TryGenerateThumbnail(f);
                        }
                    }
                    else
                    {
                        if (!dbFiles.TryGetValue(f, out File newF))
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        f = newF;
                    }

                    if (args.RefreshClasses)
                    {
                        foreach (var c in args.Classes)
                        {
                            FileClass fc = Db.FileClasses.IncludeAll().FirstOrDefault(p => p.Class == c && p.File == f);
                            bool isMatched = FileUtility.IsMatched(file, c);
                            if (fc == null && isMatched)
                            {
                                Db.Add(new FileClass(c, f, false));
                            }
                            else if (fc != null && !isMatched)
                            {
                                Db.Entry(fc).State = EntityState.Deleted;
                            }
                            else if (fc != null && isMatched)
                            {

                            }
                        }
                    }
                    args.Callback?.Invoke((++index * 1.0) / count, f);
                }
                Db.SaveChanges();
            });
        }
        public static Task<int> GetFilesCountAsync(Project project)
        {
            return Db.Files.CountAsync(p => p.Project == project);
        }
        public static Task<int> GetFileClassesCountAsync(Project project)
        {
            return Db.FileClasses.CountAsync(p => p.File.Project == project);
        }
        public static Task<int> GetClassesCountAsync(Project project)
        {
            return Db.Classes.CountAsync(p => p.Project == project);
        }
    }

    public class UpdateFilesArgs
    {
        public Project Project { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public bool IncludeThumbnails { get; set; }
        public Action<double, File> Callback { get; set; }
        public bool RefreshClasses { get; set; }
    }
}
