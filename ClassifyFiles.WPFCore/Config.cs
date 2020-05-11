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
    public class GUIConfig : FzLib.DataStorage.Serialization.JsonSerializationBase,INotifyPropertyChanged
    {
        private static GUIConfig instance;

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

        public static GUIConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = OpenOrCreate<GUIConfig>(IOPath.Combine(DataPath, "guiConfig.json"));
                }
                return instance;
            }
        }

        /// <summary>
        /// 两色暗色主题。1为亮色，-1为暗色，0为跟随系统
        /// </summary>
        public int Theme { get; set; } = 0;
        /// <summary>
        /// 0为不播放，1为默认，2为
        /// </summary>
        public int Ring { get; set; } = 0;
        public string CustomRingName { get; set; } = null;
        [Newtonsoft.Json.JsonIgnore]
        public string CustomRingPath => CustomRingName == null ? null : IOPath.Combine(DataPath, CustomRingName);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
