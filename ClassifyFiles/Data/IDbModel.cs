using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.Data
{
    public abstract class DbModelBase :INotifyPropertyChanged
    {
        [Key]
        public virtual int ID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

    }

    public abstract class ClassifyItemModelBase : DbModelBase
    {
        [Required]
        public string Name { get; set; } = "";
        public Project Project { get; set; }
        [Required]
        public int ProjectID { get; set; }
        public List<File> Files { get; set; }

    }

}
