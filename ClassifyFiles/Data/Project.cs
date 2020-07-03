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
