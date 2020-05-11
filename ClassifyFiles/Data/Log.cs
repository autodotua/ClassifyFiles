using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClassifyFiles.Data
{
    public class Log : DbModelBase
    {
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public string Message { get; set; } = "";

    }
}
