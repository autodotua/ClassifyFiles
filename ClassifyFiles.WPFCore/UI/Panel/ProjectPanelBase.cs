using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using ClassifyFiles.UI.Panel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.UI.Panel
{
    public abstract class ProjectPanelBase : UserControlBase
    {
        public async Task LoadAsync(Project project)
        {
            if (Project != null || project == null)
            {
                return;
            }
            Project = project;
            GetClassesPanel().Classes = await DbUtility.GetClassesAsync(Project);
        }
        public abstract ClassesPanel GetClassesPanel();
        public virtual Project Project { get; private set; }
    }
}
