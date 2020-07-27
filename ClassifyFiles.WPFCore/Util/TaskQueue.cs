using ClassifyFiles.Util;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    public class TaskQueue
    {
        public event EventHandler<ProcessStatusChangedEventArgs> ProcessStatusChanged;

        ConcurrentQueue<Func<Task>> tasks = new ConcurrentQueue<Func<Task>>();
        public bool IsExcuting { get; private set; } = false;
        bool stopping = false;
        public async void Enqueue(Func<Task> t)
        {
            tasks.Enqueue(t);
            if (!IsExcuting)
            {
                IsExcuting = true;
                ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(true));
                await t();
                while (tasks.Count > 0 && !stopping)
                {
                    Debug.WriteLine("Task count is " + tasks.Count);
                    var currentTasks = tasks.ToArray();
                    tasks.Clear();
                    await Task.Run(() =>
                    {
                        ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = Configs.RefreshThreadCount };
                        Parallel.ForEach(currentTasks, opt, (t2, e) =>
                         {
                             Debug.WriteLine("dsd");
                             if (stopping)
                             {
                                 e.Stop();
                             }
                             try
                             {
                                 t2().Wait();
                             }
                             catch (Exception ex)
                             {

                             }
                         });

                        try
                        {
                            DbUtility.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
                stopping = false;
                ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(false));
            }
            else
            {
                tasks.Enqueue(t);
            }
            IsExcuting = false;
        }

        public async Task Stop()
        {
            if (!IsExcuting)
            {
                return;
            }
            stopping = true;
            while (IsExcuting)
            {
                await Task.Delay(1);
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
