using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;

namespace ClassifyFiles.Data
{
    [DebuggerDisplay("Dir={Dir}, Name={Name}")]
    public class File : DbModelBase
    {
        public File()
        {
        } 
        public File(FileInfo file,DirectoryInfo root,Class c)
        {
            Name = file.Name;
            Class = c;
            if(!file.FullName.Contains(root.FullName))
            {
                throw new Exception("根目录路径没有被包含在文件路径中");
            }

            Dir = file.FullName.Replace(root.FullName, "").Replace(file.Name,"").Trim('\\');
        }

        [Required]
        public string Dir { get; set; } = "";
        [Required]
        public string Name { get; set; } = "";
        public Class Class { get; set; }
        public int ClassID { get; set; }
        [NotMapped]
        public List<File> SubFiles { get; set; } = new List<File>();
    }
}
