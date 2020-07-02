using FzLib.Extension;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class Project : DbModelBase
    {
        [Required]
        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                this.Notify(nameof(Name));
            }
        }
        private List<Class> classes;
        public List<Class> Classes
        {
            get => classes;
            set
            {
                classes = value;
                this.Notify(nameof(Classes));
            }
        }
        private ClassifyType type = ClassifyType.FileProps;
        /// <summary>
        /// 分类的方式：0代表文件属性，1代表标签
        /// </summary>
        public ClassifyType Type
        {
            get => type; set
            {
                type = value;
                this.Notify(nameof(Type));
            }
        }
        private string rootPath;
        public string RootPath
        {
            get => rootPath;
            set
            {
                rootPath = value;
                this.Notify(nameof(RootPath));
            }
        }
        public enum ClassifyType
        {
            FileProps,
            Tag
        }
    }
}
