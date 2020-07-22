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
        public static Task DeleteFilesOfProjectAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(DeleteFilesOfProjectAsync));

            return db.Database.ExecuteSqlRawAsync("delete from Files where ProjectID = " + project.ID);
            //await db.SaveChangesAsync();
        }

        public async static Task<List<File>> GetFilesByProjectAsync(int projectID)
        {
            Debug.WriteLine("db: " + nameof(GetFilesByProjectAsync));
            var files = db.Files
                .Where(p => p.Project.ID == projectID)
                .Include(p => p.Project);
            return await files.ToListAsync();
        }
        public async static Task<List<File>> GetNoClassesFilesByProjectAsync(int projectID)
        {
            Debug.WriteLine("db: " + nameof(GetNoClassesFilesByProjectAsync));
            List<File> files = null;
            await Task.Run(() =>
            {
                var tempFiles = (from f in db.Files.Where(p => p.ProjectID == projectID)
                                 join fc in db.FileClasses on f.ID equals fc.FileID into temp
                                 from ffc in temp.DefaultIfEmpty()
                                 where ffc == null
                                 select f).Include(p=>p.Project).AsEnumerable();

                var dirs = db.FileClasses
                .Where(p => p.File.ProjectID == projectID)
                .Select(p => p.File)
                .Distinct()
                .Where(p => string.IsNullOrEmpty(p.Name))
                .Select(p => p.Dir)
                .ToList();

                files = tempFiles.Where(file =>
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

            });
            return files;
        }


        public static Task<int> GetFilesCountAsync(Project project)
        {
            return db.Files.CountAsync(p => p.Project == project);
        }
    }
}
