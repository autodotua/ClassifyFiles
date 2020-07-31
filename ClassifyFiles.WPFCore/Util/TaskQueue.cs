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

        ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();
        public bool IsExcuting { get; private set; } = false;
        bool stopping = false;

        /// <summary>
        /// 将新的任务加入队列
        /// </summary>
        /// <param name="t"></param>
        /// <remarks>
        /// 当一个Task需要加入队列时，首先判断是否当前有任务正在执行。
        /// 如果有正在执行的队列，那么就把任务加入队列。
        /// 如果没有正在执行的任务，就先执行该任务。
        /// 执行任务的时候，可能有其他任务加了进来。
        /// 执行完成后，把队列中的任务都提取出来，然后多线程循环处理开始时队列中的所有任务。
        /// 在处理这些任务的时候，有可能又有新的任务加了进来，
        /// 所以需要循环处理，直到队列为空。
        /// </remarks>
        public  Task Enqueue(Action t)
        {
            return Task.Run(() =>
            {
                tasks.Enqueue(t);
                if (!IsExcuting)
                {
                    IsExcuting = true;
                    ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(true));
                    t();
                    while (tasks.Count > 0 && !stopping)
                    {
                        Debug.WriteLine("Task count is " + tasks.Count);

                        var currentTasks = tasks.ToArray();
                        tasks.Clear();
                        ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = Configs.RefreshThreadCount };
                        Parallel.ForEach(currentTasks, opt, (t2, e) =>
                         {
                             if (stopping)
                             {
                                 e.Stop();
                             }
                             try
                             {
                                 //执行任务
                                 Stopwatch sw = Stopwatch.StartNew();
                                 t2();
                                 sw.Stop();
                                 Debug.WriteLine($"create thumb use {sw.ElapsedMilliseconds} ms");
                             }
                             catch (Exception ex)
                             {

                             }

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
            });
        }
        private DateTime lastDbSaveTime = DateTime.MinValue;

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
