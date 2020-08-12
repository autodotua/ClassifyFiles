using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ProjectUtility
    {
        public static List<Project> GetProjects()
        {
            Debug.WriteLine("db begin: " + nameof(GetProjects));

            return db.Projects.Include(p => p.Classes).ToList();
        }

        public static bool UpdateProject(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(UpdateProject));

            db.Entry(project).State = EntityState.Modified;
            return SaveChanges() > 0;
        }

        public static bool DeleteProject(Project project)
        {
            Debug.WriteLine("db begin: " + nameof(DeleteProject));

            db.Entry(project).State = EntityState.Deleted;
            return SaveChanges() > 0;
        }

        public static Project AddProject()
        {
            Debug.WriteLine("db begin: " + nameof(AddProject));

            Project project = new Project() { Name = "未命名" };
            db.Projects.Add(project);
            SaveChanges();
            return project;
        }

        public static void ExportProject(string path, Project project)
        {
            List<File> files = db.Files.Where(p => p.ProjectID == p.ID).ToList();
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
        }

        public static void ExportAll(string path)
        {
            System.IO.File.Copy(DbPath, path);
        }

        public static List<Project> Import(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new System.IO.FileNotFoundException();
            }
            List<Project> projects = null;
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
            SaveChanges();
            importDb.Dispose();
            return projects.ToList();
        }

        public static Dictionary<CheckType, IReadOnlyList<DbModelBase>> Check(Project project)
        {
            Dictionary<CheckType, IReadOnlyList<DbModelBase>> results = new Dictionary<CheckType, IReadOnlyList<DbModelBase>>();

            var classes = db.Classes.Where(p => p.ProjectID == project.ID).ToList();
            foreach (var c in classes)
            {
                var fcs = db.FileClasses.Where(p => p.Class == c).ToList();
                var duplicated = fcs.GroupBy(p => p.FileID).Where(p => p.Count() > 1).ToList();
            }
            var files = db.Files.Where(p => p.ProjectID == project.ID).ToList();
            foreach (var file in files)
            {
                var fcs = db.FileClasses.Where(p => p.File == file).ToList();
                var duplicated = fcs.GroupBy(p => p.ClassID).Where(p => p.Count() > 1).ToList();
                if (duplicated.Count > 0)
                {
                }
            }
            return results;
        }

        public enum CheckType
        {
            DuplicatedFileClasses
        }
    }
}