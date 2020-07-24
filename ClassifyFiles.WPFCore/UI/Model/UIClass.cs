using ClassifyFiles.Data;
using ClassifyFiles.Util;
using FzLib.Extension;
using System.ComponentModel;
using System.Threading.Tasks;
using static ClassifyFiles.Util.ClassUtility;

namespace ClassifyFiles.UI.Model
{
    public class UIClass : INotifyPropertyChanged
    {
        public UIClass(Class c)
        {
            Class = c;
        }
        public async Task UpdatePropertiesAsync()
        {
            FileCount = await GetFilesCountOfClassAsync(Class);
            MatchConditionsCount = Class.MatchConditions.Count;
        }

        public Class Class { get; private set; }
        private int fileCount;
        public int FileCount
        {
            get => fileCount;
            private set
            {
                fileCount = value;
                this.Notify(nameof(FileCount));
            }
        }     
        private int matchConditionsCount;
        public int MatchConditionsCount
        {
            get => matchConditionsCount;
            private set
            {
                matchConditionsCount = value;
                this.Notify(nameof(MatchConditionsCount));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
