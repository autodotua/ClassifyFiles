using ClassifyFiles.Data;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static ClassifyFiles.Util.FileClassUtility;

namespace ClassifyFiles.UI.Model
{
    public class UIFile : INotifyPropertyChanged
    {
        public UIFile()
        {
        }

        public UIFile(File file) : this()
        {
            File = file;
            SubUIFiles = new ObservableCollection<UIFile>(file.SubFiles.Select(p => new UIFile(p)));
            Display = new UIFileDisplay(file);
        }

        public ObservableCollection<UIFile> SubUIFiles { get; } = new ObservableCollection<UIFile>();
        private UIFile parent;

        public UIFile Parent
        {
            get => parent;
            set
            {
                parent = value;
                this.Notify(nameof(Parent));
            }
        }

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

        public UIFileDisplay Display { get; set; }

        public async Task LoadClassesAsync(AppDbContext db = null, bool force = false)
        {
            if (Classes == null || force)
            {
                IEnumerable<Class> classes = null;
                await Task.Run(() =>
                {
                    try
                    {
                        if (db == null)
                        {
                            classes = GetClassesOfFile(File);
                        }
                        else
                        {
                            classes = GetClassesOfFile(db, File);
                        }
                    }
                    catch (Exception ex)
                    {
                        classes = Array.Empty<Class>();
                    }
                });
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

        public Class Class { get; set; }

        public override string ToString()
        {
            return File.Name + (string.IsNullOrEmpty(File.Dir) ? "" : $" （{File.Dir}）");
        }

        public override bool Equals(object obj)
        {
            if (obj is UIFile file)
            {
                return file.File.Equals(File);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(File);
        }
    }
}