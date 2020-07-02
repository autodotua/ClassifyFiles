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
    public abstract class ProjectPanelBase : UserControlBase, ILoadable 
    {
        public virtual async Task LoadAsync(Project project)
        {
            Project = project;
            if (GetItemsPanel() != null)
            {
                ListPanelBase panel = GetItemsPanel();
                await panel.LoadAsync(project);
                if (selectedItem != null)
                {
                    panel.SelectedItem = selectedItem;
                }
                else if (panel.Items.Count > 0)
                {
                    panel.SelectedItem = panel.Items[0];
                }
                panel.SelectedItemChanged += (p1, p2) =>
                {
                    selectedItem = panel.SelectedItem;
                };
            }
        }
        public abstract ListPanelBase GetItemsPanel();
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
        public static ClassifyItemModelBase selectedItem { get; private set; }
    }
}
