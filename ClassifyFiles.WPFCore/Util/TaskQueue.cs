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

        ConcurrentQueue<Task> tasks = new ConcurrentQueue<Task>();
        public bool IsExcuting { get; private set; } = false;
        bool stopping = false;
        public async void Enqueue(Task t)
        {
            tasks.Enqueue(t);
            if (!IsExcuting)
            {
                IsExcuting = true;
                ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(true));
                await t;
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
                             if (stopping)
                             {
                                 e.Stop();
                             }
                             try
                             {
                                 Stopwatch sw = Stopwatch.StartNew();
                                 t2.Wait();
                                 sw.Stop();
                                 Debug.WriteLine($"create thumb use {sw.ElapsedMilliseconds} ms");
                             }
                             catch (Exception ex)
                             {

                             }
                         });
                        if (!stopping)
                        {
                            //防止频繁保存数据库
                            if ((DateTime.Now - lastDbSaveTime).Seconds > 10)
                            {
                                lastDbSaveTime = DateTime.Now;
                                try
                                {
                                    DbUtility.SaveChanges();
                                }
                                catch (Exception ex)
                                {

                                }
                            }
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
      private  DateTime lastDbSaveTime = DateTime.MinValue;

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
