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

namespace ClassifyFiles.UI.Panel
{
    public interface ILoadable
    {
        public Task LoadAsync(Project project);

    }
    public abstract class ProjectPanelBase<T> : UserControlBase, ILoadable where T : ClassifyItemModelBase
    {
        public virtual async Task LoadAsync(Project project)
        {
            Project = project;
            if (GetItemsPanel() != null)
            {
                await GetItemsPanel().LoadAsync(project);
                if (SelectedItem != null)
                {
                    GetItemsPanel().SelectedItem = SelectedItem;
                }
                else if (GetItemsPanel().Items.Count > 0)
                {
                    GetItemsPanel().SelectedItem = GetItemsPanel().Items[0];
                }
                GetItemsPanel().PropertyChanged += (p1, p2) =>
                {
                    if (p2.PropertyName == nameof(ListPanelBase<ClassifyItemModelBase>.SelectedItem))
                    {
                        SelectedItem = GetItemsPanel().SelectedItem;
                    }
                };
            }
        }
        public abstract ListPanelBase<T> GetItemsPanel();
        private Project project;
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
            return (Window.GetWindow(this) as MainWindow).Progress;
        }
        public static T SelectedItem { get; private set; }
    }
}
