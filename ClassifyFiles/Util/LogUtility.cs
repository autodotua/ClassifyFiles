using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class LogUtility
    {
        public static Task<List<Log>> GetLogsAsync(DateTime from,DateTime to)
        {
            return db.Logs.Where(p => p.Time > from && p.Time < to).ToListAsync() ;
        }

        public async static Task AddLogAsync(string message)
        {
            Log log = new Log() { Time = DateTime.Now, Message = message };
            db.Logs.Add(log);
            await db.SaveChangesAsync();
        }
    }
}
