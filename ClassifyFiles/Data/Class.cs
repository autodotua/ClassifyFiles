using FzLib.Extension;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassifyFiles.Data
{
    public class Class : DbModelBase
    {
        private string name = "";
        [Required]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                this.Notify(nameof(Name));
            }
        }
        public Project Project { get; set; }
        [Required]
        public int ProjectID { get; set; }
        public bool Secondary { get; set; } = false;
        public List<MatchCondition> MatchConditions { get; set; } = new List<MatchCondition>();
        [Required]
        public int Index { get; set; } = 0;

        public string DisplayNameFormat { get; set; }

    }
}
