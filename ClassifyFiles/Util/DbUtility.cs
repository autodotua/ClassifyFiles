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

            //foreach (var c in classes.ToList())
            //{
            //    if (c.ParentID != null)
            //    {
            //        Class parent = classes.First(p => p.ID == c.ParentID);
            //        if (parent.Children == null)
            //        {
            //            parent.Children = new List<Class>();
            //        }
            //        parent.Children.Add(c);
            //        //classes.Remove(c);
            //    }
            //}

            return classes.Where(p => p.Parent == null).ToList();
            //List<Class> rootClasses =await db.Classes
            //    .Where(p => p.Project == project)
            //    .Where(p => p.Parent == null)
            //    .Include(p=>p.Children)
            //    .ToListAsync();
            //Queue<Class> classes = new Queue<Class>(rootClasses);
            //while(classes.Count>0)
            //{
            //    Class c = classes.Dequeue();
            //    if(c.Children==null || c.Children.Count==0)
            //    {
            //        continue;
            //    }

            //    foreach (var child in c.Children)
            //    {
            //        var subChildren=db.Classes.loa
            //    }
            //}
        }

        public static async Task SaveClassAsync(Class c)
        {
            Db.Entry(c).State = EntityState.Modified;
            await Db.SaveChangesAsync();
        }
    }
}
