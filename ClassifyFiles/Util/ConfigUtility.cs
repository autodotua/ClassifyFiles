using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ConfigUtility
    {
        static ConfigUtility()
        {
            using AppDbContext db = GetNewDb();
            configs = db.Configs.ToDictionary(p => p.Key, p => p.Value);
        }

        private static Dictionary<string, string> configs = null;

        public static int GetInt(string key, int defaultValue)
        {
            bool hasValue = configs.TryGetValue(key, out string value);
            return hasValue ? int.Parse(value) : defaultValue;
        }

        public static long GetLong(string key, long defaultValue)
        {
            bool hasValue = configs.TryGetValue(key, out string value);
            return hasValue ? long.Parse(value) : defaultValue;
        }

        public static double GetDouble(string key, double defaultValue)
        {
            bool hasValue = configs.TryGetValue(key, out string value);
            return hasValue ? double.Parse(value) : defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            bool hasValue = configs.TryGetValue(key, out string value);
            return hasValue ? bool.Parse(value) : defaultValue;
        }

        public static string GetString(string key, string defaultValue)
        {
            bool hasValue = configs.TryGetValue(key, out string value);
            return hasValue ? value : defaultValue;
        }

        public static Task SetAsync(string key, object value)
        {
            return Task.Run(() =>
            {
                using var db = GetNewDb();
                Config config = db.Configs.FirstOrDefault(p => p.Key == key);
                if (config != null)
                {
                    if (config.ToString() == value.ToString())
                    {
                        return;
                    }
                    config.Value = value.ToString();

                    db.Entry(config).State = EntityState.Modified;
                }
                else
                {
                    config = new Config(key, value.ToString());
                    configs.Add(key, value.ToString());
                    var result = db.Configs.Add(config);
                }
                db.SaveChanges();
            });
        }
    }
}