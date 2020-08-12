using ClassifyFiles.Data;
using FzLib.Extension;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace ClassifyFiles.UI.Page
{
    public interface ILoadable
    {
        public Task LoadAsync(Project project);
    }

    public abstract class ProjectPageBase : ModernWpf.Controls.Page, ILoadable, INotifyPropertyChanged
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
    }
}