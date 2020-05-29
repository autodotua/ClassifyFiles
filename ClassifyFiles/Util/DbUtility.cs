using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    public static class DbUtility
    {
        public static string DbPath { get; private set; } = "data.db";
        private static AppDbContext db;
        public static AppDbContext Db
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
            return await Db.Projects.ToListAsync();
        }
        public static async Task<List<Class>> GetClassesAsync(Project project)
        {
            List<Class> classes = await db.Classes
                .Where(p => p.Project == project)
                .Include(p => p.MatchConditions)
                .ToListAsync();


            return classes.Where(p => p.Parent == null).ToList();

        }

        public static async Task SaveClassAsync(Class c)
        {
            if (await Db.Classes.AnyAsync(p => p.ID == c.ID))
            {
                Db.Entry(c).State = EntityState.Modified;
                await Db.SaveChangesAsync();
            }
        }
        public static async Task UpdateProjectAsync(Project project)
        {
            Db.Entry(project).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        public static async Task DeleteProjectAsync(Project project)
        {
            Db.Entry(project).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }

        public static async Task<Project> AddProjectAsync()
        {
            Project project = new Project() { Name = "未命名" };
            Db.Projects.Add(project);
            await Db.SaveChangesAsync();
            return project;
        }
        public static async Task<Class> AddClassAsync(Project project, Class reference, bool inside)
        {
            Class c = new Class() { Project = project, Name = "未命名" };
            if (reference != null)
            {
                if (inside)
                {
                    c.Parent = reference;
                }
                else
                {
                    c.Parent = reference.Parent;
                }
            }
            Db.Classes.Add(c);
            await db.SaveChangesAsync();
            return c;
        }
        public static async Task DeleteClassAsync(Class c)
        {
            Db.Entry(c).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }

        public static async Task UpdateFilesAsync(Dictionary<Class, List<File>> classFiles)
        {
            foreach (var item in classFiles)
            {
                Class c = item.Key;
                List<File> files = item.Value;
                c.Files = files;
                Db.Entry(c).State = EntityState.Modified;
            }
            await Db.SaveChangesAsync();
        }
        public static Task<List<File>> GetFilesAsync(Class c)
        {
            var files = Db.Files.Where(p => p.Class == c);
            return files.ToListAsync();
        }

        public async static Task ExportProject(string path, int projectID)
        {
            Project project = await Db.Projects.Where(p => p.ID == projectID)
               .Include(p => p.Classes).ThenInclude(p => p.Files)
               .Include(p => p.Classes).ThenInclude(p => p.MatchConditions)
               .Include(p => p.Classes).ThenInclude(p => p.Files)
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
              .Include(p => p.Classes).ThenInclude(p => p.Files)
              .Include(p => p.Classes).ThenInclude(p => p.MatchConditions)
              .Include(p => p.Classes).ThenInclude(p => p.Files)
              .FirstAsync();
                await Task.Run(() =>
                {
                    projects.Add(project);
                    project.ID = 0;
                    foreach (var c in project.Classes)
                    {
                        c.ID = 0;
                        foreach (var file in c.Files)
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
    }
}
