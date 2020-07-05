﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class Config : DbModelBase
    {
        public Config()
        {

        }
        public Config(string key, string value)
        {
            Key = key;
            Value = value;
        }
        public string Key { get; set; }
        public string Value { get; set; }
    }
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
