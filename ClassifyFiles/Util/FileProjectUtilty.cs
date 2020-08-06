using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class FileProjectUtilty
    {
        public static void DeleteFilesOfProject(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(DeleteFilesOfProject));

            db.Database.ExecuteSqlRaw("delete from Files where ProjectID = " + project.ID);
            Debug.WriteLine("db end: " + nameof(DeleteFilesOfProject));
        }

        public static IEnumerable<KeyValuePair<File, IEnumerable<Class>>> GetFilesWithClassesByProject(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(GetFilesWithClassesByProject));
            var tempFiles = (from f in db.Files.Where(p => p.ProjectID == project.ID).Include(p => p.Project)
                             join fc in db.FileClasses on f.ID equals fc.FileID into temp
                             from fcc in temp.DefaultIfEmpty()
                             select new { File = f, fcc.Class })
                             .ToList();
            var result = tempFiles
                 .GroupBy(p => p.File)
                 .Select( p => KeyValuePair.Create( p.Key,  p
                 .Where(q => q.Class != null)//由于上面用了左连接，因此会有一些Class为null的出现，需要剔除。
                 .Select(q => q.Class)));
            Debug.WriteLine("db end: " + nameof(GetFilesWithClassesByProject));
            return result;
        }
        public static IEnumerable<KeyValuePair<File, IEnumerable<Class>>> GetManualFilesWithClassesByProject(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(GetManualFilesWithClassesByProject));
            var result = db.FileClasses
                      .Where(p => p.File.Project == project)
                      .Where(p => p.Status == FileClassStatus.AddManully)
                      .IncludeAll()//需要包含FileClass.File
                      .AsEnumerable()//需要内存分组
                      .GroupBy(p => p.File)//按文件分组
                      .Select(p => KeyValuePair.Create(p.Key, p.Select(q => q.Class)));
            Debug.WriteLine("db end: " + nameof(GetManualFilesWithClassesByProject));
            return result;
        }
        public static IEnumerable<KeyValuePair<File, IEnumerable<Class>>> GetDisabledFilesWithClassesByProject(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(GetDisabledFilesWithClassesByProject));
            var result = db.FileClasses
                      .Where(p => p.File.Project == project)
                      .Where(p => p.Status == FileClassStatus.Disabled)
                      .IncludeAll()//需要包含FileClass.File
                      .AsEnumerable()//需要内存分组
                      .GroupBy(p => p.File)//按文件分组
                      .Select(p => KeyValuePair.Create(p.Key, p.Select(q => q.Class)));
            Debug.WriteLine("db end: " + nameof(GetDisabledFilesWithClassesByProject));
            return result;
        }

        public static List<File> GetNoClassesFilesByProject(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(GetNoClassesFilesByProject));
            var tempFiles = (from f in db.Files.Where(p => p.ProjectID == project.ID)
                             join fc in db.FileClasses on f.ID equals fc.FileID into temp
                             from ffc in temp.DefaultIfEmpty()
                             where ffc == null
                             select f).Include(p => p.Project).AsEnumerable();

            var dirs = db.FileClasses
            .Where(p => p.File.ProjectID == project.ID)
            .Select(p => p.File)
            .Distinct()
            .Where(p => string.IsNullOrEmpty(p.Name))
            .Select(p => p.Dir)
            .ToList();

            var result = tempFiles.Where(file =>
             {
                 foreach (var dir in dirs)
                 {
                     if (file.Dir.StartsWith(dir))
                     {
                         return false;
                     }
                 }
                 return true;
             }).ToList();
            Debug.WriteLine("db end: " + nameof(GetNoClassesFilesByProject));
            return result;
        }

        public static int GetFilesCount(Project project)
        {
            using var db = GetNewDb();
            return db.Files.Count(p => p.Project == project);
        }
    }
}
