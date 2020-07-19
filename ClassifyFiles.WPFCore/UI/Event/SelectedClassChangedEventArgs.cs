using System;
using ClassifyFiles.Data;

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
}
