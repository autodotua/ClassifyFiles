using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace ClassifyFiles
{
    public class Config : FzLib.DataStorage.Serialization.JsonSerializationBase,INotifyPropertyChanged
    {
        private static Config instance;
        private bool backgroundTask=true;

        public static string DataPath
        {
            get
            {
#if DEBUG
                string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FzLib.Program.App.ProgramName, "Debug");
#else
                string path= IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FzLib.Program.App.ProgramName);
#endif
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = OpenOrCreate<Config>(IOPath.Combine(DataPath, "coreConfig.json"));
                }
                return instance;
            }
        }
        /// <summary>
        /// 程序语言，支持zh-CN和en-US
        /// </summary>
        public string Language { get; set; } = "zh-CN";
        [Newtonsoft.Json.JsonIgnore]
        public Encoding Encoding => Encoding.UTF8;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
