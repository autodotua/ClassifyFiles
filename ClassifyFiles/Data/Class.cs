using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class Class : DbModelBase
    {
        [Required]
        public string Name { get; set; } = "";
        public Project Project { get; set; }
        [Required]
        public int ProjectID { get; set; }
        public List<MatchCondition> MatchConditions { get; set; }

    }
}
