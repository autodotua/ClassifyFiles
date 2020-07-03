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
            Manual = manual;
        }

        public Class Class { get; set; }
        public int ClassID { get; set; }
        public File File { get; set; }
        public int FileID { get; set; }
        public bool Manual { get; set; } = false;

    }
}
