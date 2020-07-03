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
}
