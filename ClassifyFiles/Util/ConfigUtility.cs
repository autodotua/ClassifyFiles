using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ConfigUtility
    {
        public static int GetInt(string key, int defaultValue)
        {
            string value = (db.Configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : int.Parse(value);
        }
        public static long GetLong(string key, long defaultValue)
        {
            string value = (db.Configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : long.Parse(value);
        }
        public static double GetDouble(string key, double defaultValue)
        {
            string value = (db.Configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : double.Parse(value);
        }
        public static bool GetBool(string key, bool defaultValue)
        {
            string value = (db.Configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value == null ? defaultValue : bool.Parse(value);
        }
        public static string GetString(string key, string defaultValue)
        {
            using var db= GetNewDb();
            string value = (db.Configs.FirstOrDefault(p => p.Key == key))?.Value;
            return value ?? defaultValue;
        }
        public static void Set(string key, object value)
        {
            using var db = GetNewDb();
            Config config = db.Configs.FirstOrDefault(p => p.Key == key);
            if (config != null)
            {
                if(config.ToString()==value.ToString())
                {
                    return;
                }
                config.Value = value.ToString();
                db.Entry(config).State = EntityState.Modified;
            }
            else
            {
                config = new Config(key, value.ToString());
                db.Configs.Add(config);
            }

            SaveChanges();

        }
    }
}
