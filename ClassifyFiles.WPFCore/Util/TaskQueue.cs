//#define SingleThread

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClassifyFiles.Util
{
    public class TaskQueue
    {
        public event EventHandler<ProcessStatusChangedEventArgs> ProcessStatusChanged;

        private ConcurrentStack<Action> tasks = new ConcurrentStack<Action>();
        private object isExcuting = false;
        public bool IsExcuting => (bool)isExcuting;
        private bool stopping = false;
        private const int TasksMaxCount = 300;

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
        public Task Enqueue(Action t)
        {
            return Task.Run(() =>
            {
                tasks.Push(t);
                if (false.Equals(isExcuting))
                {
                    lock (isExcuting)
                    {
                        //多个请求可能会同时执行，所以先要把变量锁了，然后之后的解锁之后就会发现已经在执行了，就加入队列
                        if (isExcuting.Equals(true))
                        {
                            return;
                        }
                        isExcuting = true;
                    }
                    Debug.WriteLineIf(DebugSwitch.TaskQueue, "New Task Queue");

                    ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(true));
                    try
                    {
                        t();
                    }
                    catch (Exception ex)
                    {
                    }
                    while (tasks.Count > 0 && !stopping)
                    {
                        if (tasks.Count > TasksMaxCount)
                        {
                            Stack<Action> temp = new Stack<Action>();
                            for (int i = 0; i < TasksMaxCount; i++)
                            {
                                if (tasks.TryPop(out Action act))
                                {
                                    temp.Push(act);
                                }
                            }
                            tasks.Clear();
                            while (temp.Count > 0)
                            {
                                tasks.Push(temp.Pop());
                            }
                        }
                        Debug.WriteLineIf(DebugSwitch.TaskQueue, "Task count is " + tasks.Count);
                        List<Action> currentTasks = new List<Action>();
                        //后进来的先处理，每次处理2倍的线程数份，这样可以让后进来的尽快处理
                        for (int i = 0; i < Configs.RefreshThreadCount * 2; i++)
                        {
                            if (tasks.Count > 0)
                            {
                                if (tasks.TryPop(out Action act))
                                {
                                    currentTasks.Add(act);
                                }
                            }
                        }

#if (SingleThread &&DEBUG)

                        foreach (var task in currentTasks)
                        {
                            if (stopping)
                            {
                                break;
                            }
                            try
                            {
                                //执行任务
                                task();
                            }
                            catch (Exception ex)
                            {
                            }
                        }

#else
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
                                t2();
                            }
                            catch (Exception ex)
                            {
                            }
                        });
#endif
                    }
                    stopping = false;
                    ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(false));
                    lock (isExcuting)
                    {
                        isExcuting = false;
                    }
                }
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