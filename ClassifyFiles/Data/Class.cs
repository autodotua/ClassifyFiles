using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class Class : WithProjectModelBase
    {
        [Required]
        public string Name { get; set; } = "";

        public Class Parent { get; set; }
        public List<Class> Children { get; set; }
        public List<MatchCondition> MatchConditions { get; set; }
        public List<File> Files { get; set; }

    }
}
