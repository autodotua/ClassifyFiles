using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClassifyFiles.Data
{
    public abstract class DbModelBase : INotifyPropertyChanged
    {
        [Key]
        public virtual int ID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}