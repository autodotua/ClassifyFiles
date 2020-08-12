using System.Windows;

namespace ClassifyFiles.UI.Dialog
{
    public class DialogWindowBase : WindowBase
    {
        public DialogWindowBase()
        {
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
    }
}