using System;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class Log : DbModelBase
    {
        [Required]
        public DateTime Time { get; set; }

        [Required]
        public string Message { get; set; } = "";

        [Required]
        public string Details { get; set; } = "";
    }
}