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
        private static AppDbContext db;
        public static AppDbContext Db
        {
            get
            {
                if (db == null)
                {
                    db = new AppDbContext("data.db");
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
        //public static async Task<(List<Class>, List<Class>)> GetTreeAndTileClassesAsync(Project project)
        //{
        //    List<Class> classes = await db.Classes
        //        .Where(p => p.Project == project)
        //        .Include(p => p.MatchConditions)
        //        .ToListAsync();


        //    return (classes.Where(p => p.Parent == null).ToList(), classes);

        //}

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
    }
}
