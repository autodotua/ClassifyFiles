using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ClassUtility
    {

        public static List<Class> GetClasses(Project project)
        {
            Debug.WriteLine("db: " + nameof(GetClasses));

            List<Class> classes = db.Classes
                .Where(p => p.Project == project)
                .OrderBy(p => p.Index)
                .Include(p => p.MatchConditions)
                .ToList();

            return classes;
        }

        public static int GetFilesCountOfClass(Class c)
        {
            using var db = GetNewDb();
            return db.FileClasses
                .Where(p => p.Class == c)
                .Where(p => !p.Disabled)
                .Count();
        }

        public static Class AddClass(Project project)
        {
            Debug.WriteLine("db: " + nameof(AddClass));

            int maxIndex = db.Classes
                .Where(p => p.Project == project)
                .Max(p => p.Index);

            Class c = new Class() { Project = project, Name = "未命名类", Index = maxIndex + 1 };

            db.Classes.Add(c);
            SaveChanges();
            return c;
        }
        public static bool DeleteClass(Class c)
        {
            Debug.WriteLine("db: " + nameof(DeleteClass));
            db.Entry(c).State = EntityState.Deleted;
            return SaveChanges() > 0;
        }

        public static bool SaveClass(Class c)
        {
            Debug.WriteLine("db: " + nameof(SaveClass));
            db.Entry(c).State = EntityState.Modified;
            return SaveChanges() > 0;
        }
        public static bool SaveClasses(IEnumerable<Class> classes)
        {
            Debug.WriteLine("db: " + nameof(SaveClass));
            foreach (var c in classes)
            {
                db.Entry(c).State = EntityState.Modified;
            }
            return SaveChanges() > 0;
        }

        public static int GetClassesCount(Project project)
        {
            using var db = GetNewDb();
            return db.Classes.Count(p => p.Project == project);
        }
    }
}
