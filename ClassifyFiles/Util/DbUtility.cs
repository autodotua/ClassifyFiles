using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    public static class DbUtility
    {
        public const string DbInAppDataFolderMarkerFileName = "dbInAppDataFolder";
        public static string DbPath
        {
            get
            {
                if (!FileUtility.CanWriteInCurrentDirectory() || System.IO.File.Exists(DbInAppDataFolderMarkerFileName))
                {
                    string path = System.IO.Path.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(ClassifyFiles), "data.db");
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                    return path;
                }
                return System.IO.Path.GetFullPath("data.db");
            }
        }
        internal static AppDbContext db = new AppDbContext(DbPath);
        /// <summary>
        /// 获取新的
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 对于某些查询，并不需要保留查询结果与数据库的关系。
        /// 为了防止一个上下文被多个查询访问，因此可以对每个查询使用单独的上下文。
        /// </remarks>
        internal static AppDbContext GetNewDb()
        {
            return new AppDbContext(DbPath);
        }
        const string dbReplacedMessage = "数据库保存出错";
        public static bool IgnoreDbSavingError { get; set; } = true;
        public static event UnhandledExceptionEventHandler DbSavingException;
        public async static Task ReplaceDbContextAsync()
        {
            await db.DisposeAsync();
            db = new AppDbContext(DbPath);
        }
        private static bool isSavingDb = false;
        public static int SaveChanges()
        {
            Debug.WriteLine("db begin Save Changes");
            if (isSavingDb)
            {
                return -2;
            }
            int result = -1;
            isSavingDb = true;
            try
            {
                lock (db)
                {
                    result = db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(dbReplacedMessage);
                try
                {
                    LogUtility.AddLog(dbReplacedMessage, ex.ToString());
                }
                catch
                {

                }
                if (!IgnoreDbSavingError)
                {
                    throw;
                }
                DbSavingException?.Invoke(null, new UnhandledExceptionEventArgs(ex, false));
            }
            finally
            {
                isSavingDb = false;
            }
            Debug.WriteLine("db end Save Changes");
            return result;
        }
        public static int SaveChanges(AppDbContext db)
        {
            try
            {
                return db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static void Zip()
        {
            db.Database.ExecuteSqlRaw("VACUUM;");
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
            lock (db)
            {
                db.Entry(obj).State = EntityState.Modified;
            }
        }
    }
}
