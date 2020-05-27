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
    public abstract class ProjectPanelBase : UserControlBase
    {
        public virtual async Task LoadAsync(Project project)
        {
            //if (Project == null || project == null)
            //{
            //    return;
            //}
            Project = project;
            if (GetClassesPanel() != null)
            {
                await GetClassesPanel().LoadAsync(project);
                if (SelectedClass != null)
                {
                    GetClassesPanel().SelectClass(SelectedClass);
                }
                else if (GetClassesPanel().Classes.Count > 0)
                {
                    GetClassesPanel().SelectClass(GetClassesPanel().Classes[0]);
                }
                GetClassesPanel().SelectedClassChanged += (p1, p2) =>
                {
                    SelectedClass = GetClassesPanel().SelectedClass;
                };
            }
        }
        public abstract ClassesPanel GetClassesPanel();
        private Project project;

        public ProjectPanelBase() : base()
        {
        }

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
        public static Class SelectedClass { get; private set; }
    }
}
