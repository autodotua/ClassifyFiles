using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;

namespace ClassifyFiles.Data
{
    [DebuggerDisplay("{ID}: Dir={Dir}, Name={Name}")]
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
        private string thumbnailGUID;
        public string ThumbnailGUID
        {
            //如果为null，则是没有获取；如果为""，则是获取失败
            get => thumbnailGUID;
            set
            {
                thumbnailGUID = value;
                this.Notify(nameof(ThumbnailGUID));
            }
        }
        private string iconGUID;
        public string IconGUID
        {
            //如果为null，则是没有获取；如果为""，则是获取失败
            get => iconGUID;
            set
            {
                iconGUID = value;
                this.Notify(nameof(IconGUID));
            }
        }

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
