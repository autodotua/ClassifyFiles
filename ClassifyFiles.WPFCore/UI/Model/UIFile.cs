using ClassifyFiles.Data;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static ClassifyFiles.Data.Project;
using static ClassifyFiles.Util.ClassUtility;
using static ClassifyFiles.Util.FileClassUtility;
using static ClassifyFiles.Util.FileProjectUtilty;
using static ClassifyFiles.Util.ProjectUtility;
using static ClassifyFiles.Util.DbUtility;
using ClassifyFiles.UI.Component;
using System.ComponentModel;
using System.Diagnostics;

namespace ClassifyFiles.UI.Model
{
    public class UIFile:INotifyPropertyChanged
    {
        public UIFile()
        {

        }
        public UIFile(File file) : this()
        {
            File = file;
            SubUIFiles = file.SubFiles.Select(p => new UIFile(p)).ToList();
            Display = new UIFileDisplay(file);
            Size = new UIFileSize();

        }
        public List<UIFile> SubUIFiles { get; private set; } = new List<UIFile>();

        private File file;
        public File File
        {
            get => file;
            set
            {
                file = value;
                this.Notify(nameof(File));
            }
        }
        private bool loaded = false;
        public UIFileSize Size { get; set; }
        public UIFileDisplay Display { get; set; }
        public async Task LoadAsync(AppDbContext db = null)
        {
            if (!loaded)
            {
                loaded = true;
                IEnumerable<Class> classes;
                if (db == null)
                {
                    classes = await GetClassesOfFileAsync(File.ID);
                }
                else
                {
                    classes = await GetClassesOfFileAsync(db, File.ID);
                }
                Classes = new ObservableCollection<Class>(classes);
            }
        }

        private ObservableCollection<Class> classes;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Class> Classes
        {
            get => classes;
            set
            {
                classes = value;
                this.Notify(nameof(Classes));
            }
        }
        public override string ToString()
        {
            return File.Name + (string.IsNullOrEmpty(File.Dir) ? "" : $" （{File.Dir}）");
        }

        public override bool Equals(object obj)
        {
            if(obj is UIFile file)
            {
                return file.File.Equals(File);
            }
            return false;
        }
    }
}
