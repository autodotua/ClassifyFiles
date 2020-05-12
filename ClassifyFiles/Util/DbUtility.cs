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
        public static async Task<(List<Class>, List<Class>)> GetTreeAndTileClassesAsync(Project project)
        {
            List<Class> classes = await db.Classes
                .Where(p => p.Project == project)
                .Include(p => p.MatchConditions)
                .ToListAsync();


            return (classes.Where(p => p.Parent == null).ToList(), classes);

        }

        public static async Task SaveClassAsync(Class c)
        {
            if (await Db.Classes.AnyAsync(p => p.ID == c.ID))
            {
                Db.Entry(c).State = EntityState.Modified;
                await Db.SaveChangesAsync();
            }
        }

        public static async Task AddClassAsync(Class c, Class reference, bool inside)
        {
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
        }
        public static async Task DeleteClassAsync(Class c)
        {
            Db.Entry(c).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
    }
}
