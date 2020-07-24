using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using System;

namespace ClassifyFiles.UI.Event
{
    public class ClickTagEventArgs : EventArgs
    {
        public ClickTagEventArgs(Class c)
        {
            Class = c;
        }

        public Class Class { get; }
    }

}
