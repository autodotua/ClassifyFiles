using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class Project : DbModelBase
    {
        [Required]
        public string Name { get; set; } = "";
        public List<Class> Classes { get; set; }
        public string RootPath { get; set; }
    }
}
