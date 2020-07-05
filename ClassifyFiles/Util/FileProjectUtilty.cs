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
            var files = db.Files.Where(p => p.Project.ID == projectID).Include(p => p.Project);
            return await files.ToListAsync();
        }


        public static Task<int> GetFilesCountAsync(Project project)
        {
            return db.Files.CountAsync(p => p.Project == project);
        }
    }
}
