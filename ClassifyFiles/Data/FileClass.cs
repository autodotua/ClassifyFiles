using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class FileClass : DbModelBase
    {
        public FileClass()
        {
        }

        public FileClass(Class c, File file,bool manual)
        {
            Class = c;
            File = file;
            if (manual)
            {
                Status = FileClassStatus.AddManully;
            }
            else
            {
                Status = FileClassStatus.Auto;
            }
        }

        public Class Class { get; set; }
        public int ClassID { get; set; }
        public File File { get; set; }
        public int FileID { get; set; }
        public FileClassStatus Status { get; set; } = FileClassStatus.Auto;
    }

    public enum FileClassStatus
    {
        /// <summary>
        /// 通过分类加进来的
        /// </summary>
        Auto=1,
        /// <summary>
        /// 手动加入的
        /// </summary>
        AddManully=2,
        /// <summary>
        /// 被删除的。如果为该值，那么一定是通过分类加进来的。
        /// </summary>
        Disabled=3
    }
}
