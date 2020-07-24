using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    public static class DbUtility
    {
        public static string DbPath => System.IO.Path.GetFullPath("data.db");
        internal static AppDbContext db = new AppDbContext(DbPath);
        const string dbReplacedMessage = "由于保存出错，数据库上下文被替换，更改已丢失";
        public async static Task ReplaceDbContextAsync()
        {
            await db.DisposeAsync();
            db = new AppDbContext(DbPath);
        }
        public async static Task SaveChangesAsync()
        {
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                db = new AppDbContext(DbPath);
                System.Diagnostics.Debug.WriteLine(dbReplacedMessage);
                try
                {
                    await LogUtility.AddLogAsync(dbReplacedMessage, ex.ToString());
                }
                catch
                {
                }
            }
        }
        public static void SaveChanges()
        {
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                db = new AppDbContext(DbPath);
                System.Diagnostics.Debug.WriteLine(dbReplacedMessage);
                try
                {
                    LogUtility.AddLogAsync(dbReplacedMessage, ex.ToString()).Wait();
                }
                catch
                {

                }
            }
        }
        public static Task ZipAsync()
        {
            return db.Database.ExecuteSqlRawAsync("VACUUM;");
        }
        public static void CancelChanges()
        {
            foreach (var entry in db.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }

        public static void SetObjectModified(object obj)
        {
            db.Entry(obj).State = EntityState.Modified;
        }
    }
}
