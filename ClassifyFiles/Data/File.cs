using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public class File : WithProjectModelBase
    {
        [Required]
        public string Dir { get; set; } = "";
        [Required]
        public string Name { get; set; } = "";
        public Class Class { get; set; }
        public int ClassID { get; set; }

    }
}
