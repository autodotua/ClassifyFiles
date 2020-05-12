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

namespace ClassifyFiles.UI.Panel
{
    public abstract class ProjectPanelBase : UserControlBase
    {
        public virtual async Task LoadAsync(Project project)
        {
            if (Project != null || project == null)
            {
                return;
            }
            Project = project;
         await   GetClassesPanel().LoadAsync(project);
        }
        public abstract ClassesPanel GetClassesPanel();
        private  Project project;
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
