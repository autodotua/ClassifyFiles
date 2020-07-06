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

        public File(FileInfo file, Project project)
        {
            Project = project;
            var root = new DirectoryInfo(project.RootPath);
            if (!file.FullName.StartsWith(root.FullName))
            {
                throw new Exception("根目录路径没有被包含在文件路径中");
            }
            if (file.Attributes.HasFlag(FileAttributes.Directory))
            {
                Dir = file.FullName.Substring(root.FullName.Length).Trim('\\');
            }
            else
            {
                Name = file.Name; 
                Dir = file.FullName.Substring(root.FullName.Length, file.FullName.Length - file.Name.Length - root.FullName.Length - 1).Trim('\\');
            }

        }

        [Required]
        public string Dir { get; set; } = "";
        [Required]
        public string Name { get; set; } = "";
        [NotMapped]
        public bool IsFolder => Name == "";
        public byte[] Thumbnail { get; set; }
        [Required]
        public Project Project { get; set; }
        public int ProjectID { get; set; }
        [NotMapped]
        public List<File> SubFiles { get; set; } = new List<File>();
        public override bool Equals(object obj)
        {
            if (!(obj is File item))
            {
                return false;
            }
            return item.Name == Name && item.Dir == Dir;
        }

        public override int GetHashCode()
        {
            return (Dir + Name).GetHashCode();
        }
    }
}
