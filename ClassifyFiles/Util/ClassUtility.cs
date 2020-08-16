using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ClassUtility
    {
        public static List<Class> GetClasses(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(GetClasses));

            List<Class> classes = db.Classes
                .Where(p => p.Project == project)
                .OrderBy(p => p.GroupName)
                .ThenBy(p => p.Index)
                .Include(p => p.MatchConditions)
                .ToList();
            Debug.WriteLine("db end: " + nameof(GetClasses));

            return classes;
        }

        public static int GetFilesCountOfClass(Class c)
        {
            Debug.WriteLine("db begin: " + nameof(GetFilesCountOfClass));

            //using var db = GetNewDb();
            int result = db.FileClasses
                .Where(p => p.Class == c)
                .Where(p => p.Status != FileClassStatus.Disabled)
                .Count();
            Debug.WriteLine("db end: " + nameof(GetFilesCountOfClass));
            return result;
        }

        public static Class AddClass(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(AddClass));

            int maxIndex = db.Classes
                .Where(p => p.Project == project).Count() == 0 ? 0 :
                db.Classes
                .Where(p => p.Project == project)
                .Max(p => p.Index);

            Class c = new Class() { Project = project, Name = "未命名类", Index = maxIndex + 1 };

            db.Classes.Add(c);
            SaveChanges();
            Debug.WriteLine("db end: " + nameof(AddClass));
            return c;
        }

        public static bool DeleteClass(Class c)
        {
            Debug.WriteLine("db begin: " + nameof(DeleteClass));
            db.Entry(c).State = EntityState.Deleted;
            bool result = SaveChanges() > 0;

            Debug.WriteLine("db end: " + nameof(DeleteClass));
            return result;
        }

        public static bool SaveClass(Class c)
        {
            Debug.WriteLine("db begin: " + nameof(SaveClass));
            db.Entry(c).State = EntityState.Modified;
            bool result = SaveChanges() > 0;
            Debug.WriteLine("db end: " + nameof(SaveClass));

            return result;
        }

        public static bool SaveClasses(IEnumerable<Class> classes)
        {
            Debug.WriteLine("db begin: " + nameof(SaveClass));
            foreach (var c in classes)
            {
                db.Entry(c).State = EntityState.Modified;
            }
            bool result = SaveChanges() > 0;
            Debug.WriteLine("db end: " + nameof(SaveClass));
            return result;
        }

        public static int GetClassesCount(Project project)
        {
            using var db = GetNewDb();
            return db.Classes.Count(p => p.Project == project);
        }
    }
}