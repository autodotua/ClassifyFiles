using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ProjectUtility
    {
        public static async Task<List<Project>> GetProjectsAsync()
        {
            Debug.WriteLine("db: " + nameof(GetProjectsAsync));

            return await db.Projects.ToListAsync();
        }
        public static async Task UpdateProjectAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(UpdateProjectAsync));

            db.Entry(project).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        public static async Task DeleteProjectAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(DeleteProjectAsync));

            db.Entry(project).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        public static async Task<Project> AddProjectAsync()
        {
            Debug.WriteLine("db: " + nameof(AddProjectAsync));

            Project project = new Project() { Name = "未命名" };
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            return project;
        }

        public async static Task ExportProjectAsync(string path, int projectID)
        {
            Project project = await db.Projects.Where(p => p.ID == projectID)
               .Include(p => p.Classes).ThenInclude(p => p.MatchConditions)
               .FirstOrDefaultAsync();

            var newDb = new AppDbContext(path);
            await newDb.Database.EnsureDeletedAsync();
            await newDb.Database.EnsureCreatedAsync();

            newDb.Projects.Add(project);
            await newDb.SaveChangesAsync();
            await newDb.DisposeAsync();
        }
        public static Task ExportAllAsync(string path)
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
              .Include(p => p.Classes).ThenInclude(p => p.MatchConditions)
              .FirstAsync();
                await Task.Run(() =>
                {
                    projects.Add(project);
                    project.ID = 0;
                    foreach (var c in project.Classes)
                    {
                        foreach (var file in FileClassUtility.GetFilesByClass(c.ID))
                        {
                            file.ID = 0;
                        }
                        c.ID = 0;
                        foreach (var m in c.MatchConditions)
                        {
                            m.ID = 0;
                        }
                    }
                    db.Add(project);
                });
            }
            await db.SaveChangesAsync();
            await importDb.DisposeAsync();
            return projects.ToArray();
        }

    }
}
