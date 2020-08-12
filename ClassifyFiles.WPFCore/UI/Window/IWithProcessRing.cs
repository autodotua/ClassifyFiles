using System.Threading.Tasks;

namespace ClassifyFiles.UI
{
    public interface IWithProcessRing
    {
        public Task DoProcessAsync(Task task);

        public void SetProcessRingMessage(string message);
    }
}