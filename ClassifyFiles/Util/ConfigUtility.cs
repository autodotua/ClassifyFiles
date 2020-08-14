using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ConfigUtility
    {
        private static Timer timer = new Timer(new TimerCallback(TimerCallback), null, 0, 10000);
        private static bool needToSave = false;
        private static AppDbContext db = GetNewDb();
        private static System.Collections.Generic.List<Config> configs = db.Configs.ToList();

        private static void TimerCallback(object obj)
        {
            if (needToSave)
            {
                needToSave = false;
                try
                {
                    lock (db)
                    {
                        db.SaveChanges();
                    }
                }
                catch
                {
                    Debug.Assert(false);
                }
            }
        }

        public static int GetInt(string key, int defaultValue)
        {
            string value = (configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : int.Parse(value);
        }

        public static long GetLong(string key, long defaultValue)
        {
            string value = (configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : long.Parse(value);
        }

        public static double GetDouble(string key, double defaultValue)
        {
            string value = (configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : double.Parse(value);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            string value = (configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : bool.Parse(value);
        }

        public static string GetString(string key, string defaultValue)
        {
            string value = (configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value ?? defaultValue;
        }

        public static void Set(string key, object value)
        {
            Config config = configs.FirstOrDefault(p => p.Key == key);
            if (config != null)
            {
                if (config.ToString() == value.ToString())
                {
                    return;
                }
                config.Value = value.ToString();
                lock (db)
                {
                    db.Entry(config).State = EntityState.Modified;
                }
            }
            else
            {
                config = new Config(key, value.ToString());
                configs.Add(config);
                lock (db)
                {
                    var result = db.Configs.Add(config);
                }
            }
            needToSave = true;
        }
    }
}