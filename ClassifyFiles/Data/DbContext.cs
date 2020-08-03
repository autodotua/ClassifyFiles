using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.Data
{
    public class AppDbContext : DbContext
    {
        public string DbPath { get; }
        private static bool created = false;
        /// <summary>
        /// 从配置文件读取链接字符串
        /// </summary>
        public AppDbContext(string dbPath) :
            base()
        {
            Debug.WriteLine("db begin create");
            DbPath = dbPath;
            if (!created)
            {
                created = true;
                Database.EnsureCreated();
            }
            Debug.WriteLine("db end create");
        }

        public DbSet<Log> Logs { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<FileClass> FileClasses { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<MatchCondition> MatchConditions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@$"Data Source={DbPath}");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Class>()
                .HasOne(p => p.Project)
                .WithMany(p => p.Classes)
                .HasForeignKey(p => p.ProjectID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<File>()
                .HasOne(p => p.Project)
                .WithMany(/*p => p.Files*/)
                .HasForeignKey(p => p.ProjectID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchCondition>()
                .HasOne(p => p.Class)
                .WithMany(p => p.MatchConditions)
                .HasForeignKey(p => p.ClassID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FileClass>()
                .HasOne(p => p.Class)
                .WithMany(/*p => p.Files*/)
                .HasForeignKey(p => p.ClassID)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<FileClass>()
                .HasOne(p => p.File)
                .WithMany(/*p => p.Classes*/)
                .HasForeignKey(p => p.FileID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Config>()
                 .HasIndex(p => p.Key);


            modelBuilder.Entity<FileClass>()
                .Property(e => e.Status)
           .HasConversion(
               v => (int)v,
               v => (FileClassStatus)v);
            modelBuilder.Entity<MatchCondition>()
                .Property(e => e.ConnectionLogic)
           .HasConversion(
               v => (int)v,
               v => (Logic)v);

            modelBuilder.Entity<MatchCondition>()
                .Property(e => e.Type)
           .HasConversion(
               v => (int)v,
               v => (MatchType)v);

        }
        private void InsertTestDatas()
        {
            Project project = new Project() { Name = "测试项目" };
            Projects.Add(project);
            int i = 0;
            string GetName()
            {
                return "分类" + (++i);
            }
            Class c = new Class()
            {
                Name = GetName(),
                Project = project,

            };
            Classes.Add(c);
            SaveChanges();
        }

    }
}
