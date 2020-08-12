using ClassifyFiles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class LogUtility
    {
        private static AppDbContext db = GetNewDb();

        public static List<Log> GetLogs(DateTime from, DateTime to)
        {
            return db.Logs.Where(p => p.Time > from && p.Time < to).ToList();
        }

        public static void AddLog(string message, string details)
        {
            Log log = new Log() { Time = DateTime.Now, Message = message, Details = details };
            db.Logs.Add(log);
            SaveChanges(db);
        }
    }
}