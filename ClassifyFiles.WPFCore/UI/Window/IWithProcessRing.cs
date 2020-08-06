using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyFiles.UI
{
    public interface IWithProcessRing
    {
        public Task DoProcessAsync(Task task);
        public void SetProcessRingMessage(string message);
    }
}
