using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using ClassifyFiles.UI.Panel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FzLib.Extension;
using System.Windows;
using System.ComponentModel;
using ClassifyFiles.WPFCore;

namespace ClassifyFiles.UI.Page
{
    public interface ILoadable
    {
        public Task LoadAsync(Project project);

    }
    public abstract class ProjectPageBase : ModernWpf.Controls.Page, ILoadable ,INotifyPropertyChanged
    {
        public ProjectPageBase()
        {
            Initialized += (p1, p2) =>
            {
                (Content as FrameworkElement).DataContext = this;
            };
        }
        public virtual async Task LoadAsync(Project project)
        {
            Project = project;
        }
        private Project project;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual Project Project
        {
            get => project;
            set
            {
                project = value;
                this.Notify(nameof(Project));
            }
        }
        protected ProgressDialog GetProgress()
        {
            return (App.Current.MainWindow as MainWindow).Progress;
        }
    }
}
