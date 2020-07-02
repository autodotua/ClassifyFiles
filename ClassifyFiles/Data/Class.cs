using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class Tag : ClassifyItemModelBase
    {
    }
    public class Class : ClassifyItemModelBase
    {
        public List<MatchCondition> MatchConditions { get; set; }

    }
}
