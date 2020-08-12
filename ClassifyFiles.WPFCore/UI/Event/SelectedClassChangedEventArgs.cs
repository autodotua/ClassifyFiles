using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using System;

namespace ClassifyFiles.UI.Event
{
    public class SelectedClassChangedEventArgs : EventArgs
    {
        public SelectedClassChangedEventArgs(Class oldValue, Class newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public Class OldValue { get; set; }
        public Class NewValue { get; set; }
    }

    public class ClassFilesDropEventArgs : EventArgs
    {
        public ClassFilesDropEventArgs(Class c, string[] files)
        {
            Class = c;
            Files = files;
        }

        public ClassFilesDropEventArgs(Class c, UIFile[] files)
        {
            Class = c;
            UIFiles = files;
        }

        public Class Class { get; }
        public string[] Files { get; }
        public UIFile[] UIFiles { get; }
    }
}