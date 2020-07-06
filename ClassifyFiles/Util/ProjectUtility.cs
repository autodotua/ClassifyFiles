using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

        public static Task ExportProjectAsync(string path, int projectID)
        {
            return Task.Run(() =>
            {
                Project project = db.Projects.Where(p => p.ID == projectID)
                   .Include(p => p.Classes).ThenInclude(p => p.MatchConditions)
                   .FirstOrDefault();
                List<File> files = db.Files.Where(p => p.ProjectID == projectID).ToList();
                List<FileClass> fcs = new List<FileClass>();
                foreach (var c in project.Classes)
                {
                    var fcs2 = db.FileClasses
                     .Where(p => p.Class == c)
                     .ToList();
                    fcs.AddRange(fcs2);
                }
                var newDb = new AppDbContext(path);
                newDb.Database.EnsureDeleted();
                newDb.Database.EnsureCreated();

                newDb.Projects.Add(project);
                newDb.Files.AddRange(files);
                newDb.FileClasses.AddRange(fcs);
                newDb.SaveChanges();
                newDb.Dispose();
            });
        }
        public static Task ExportAllAsync(string path)
        {
            return Task.Run(() =>
            {
                System.IO.File.Copy(DbPath, path);
            });
        }
        public async static Task<Project[]> ImportAsync(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new System.IO.FileNotFoundException();
            }
            List<Project> projects = null;
            await Task.Run(() =>
            {
                var importDb = new AppDbContext(path);
                projects = importDb.Projects
                .Include(p => p.Classes)
                .ThenInclude(p => p.MatchConditions)
                .ToList();
                foreach (var project in projects)
                {
                    List<File> files = importDb.Files.Where(p => p.Project == project).ToList();
                    files.ForEach(p => p.ID = 0);
                    List<FileClass> fcs = new List<FileClass>();
                    foreach (var c in project.Classes)
                    {
                        c.MatchConditions.ForEach(p => p.ID = 0);
                        var fcs2 = importDb.FileClasses
                         .Where(p => p.Class == c)
                         .IncludeAll()
                         .ToList();
                        fcs.AddRange(fcs2);
                        c.ID = 0;
                    }
                    fcs.ForEach(p => p.ID = 0);
                    project.ID = 0;
                    db.Add(project);
                    db.AddRange(files);
                    db.AddRange(fcs);
                }
                db.SaveChanges();
                importDb.Dispose();
            });
            return projects.ToArray();
        }

        public async static Task<Dictionary<CheckType, IReadOnlyList<DbModelBase>>> CheckAsync(int projectID)
        {
            Dictionary<CheckType, IReadOnlyList<DbModelBase>> results = new Dictionary<CheckType, IReadOnlyList<DbModelBase>>();
            await Task.Run(() =>
            {
                var classes = db.Classes.Where(p => p.ProjectID == projectID).ToList();
                foreach (var c in classes)
                {
                    var fcs = db.FileClasses.Where(p => p.Class == c).ToList();
                    var duplicated = fcs.GroupBy(p => p.FileID).Where(p => p.Count() > 1).ToList();
                }
                var files = db.Files.Where(p => p.ProjectID == projectID).ToList();
                foreach (var file in files)
                {
                    var fcs = db.FileClasses.Where(p => p.File == file).ToList();
                    var duplicated = fcs.GroupBy(p => p.ClassID).Where(p => p.Count() > 1).ToList();
                    if (duplicated.Count > 0)
                    {

                    }
                }
            });
            return results;
        }
        public enum CheckType
        {
            DuplicatedFileClasses
        }
    }
}
