using ClassifyFiles.Util;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    public  class TaskQueue
    {
        public event EventHandler<ProcessStatusChangedEventArgs> ProcessStatusChanged;

        ConcurrentQueue<Func<Task>> tasks = new ConcurrentQueue<Func<Task>>();
        bool isExcuting = false;

        public async void Enqueue(Func<Task> t)
        {
            tasks.Enqueue(t);
            if (!isExcuting)
            {
                isExcuting = true;
                ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(true));
                await t();
                while (tasks.Count > 0)
                {
                    Debug.WriteLine("Task count is " + tasks.Count);
                    var currentTasks = tasks.ToArray();
                    tasks.Clear();
                    await Task.Run(() =>
                    {
                        ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = Configs.RefreshThreadCount };
                        Parallel.ForEach(currentTasks, opt, t2 =>
                        {
                            try
                            {
                                t2().Wait();
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                    });
                    try
                    {
                        await DbUtility.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                isExcuting = false;
                ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(false));
            }
            else
            {
                tasks.Enqueue(t);
            }
        }
    }
    public class ProcessStatusChangedEventArgs : EventArgs
    {
        public ProcessStatusChangedEventArgs(bool isRunning)
        {
            IsRunning = isRunning;
        }

        public bool IsRunning { get; }
    }

}
