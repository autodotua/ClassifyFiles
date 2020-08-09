//#define SingleThread

using ClassifyFiles.Util;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;

namespace ClassifyFiles.Util
{
    public class TaskQueue
    {
        public event EventHandler<ProcessStatusChangedEventArgs> ProcessStatusChanged;

        private ConcurrentStack<Action> tasks = new ConcurrentStack<Action>();
        private object isExcuting = false;
        public bool IsExcuting => (bool)isExcuting;
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
                            //tasks.Enqueue(t);
                            return;
                        }
                        isExcuting = true;
                    }
                    Debug.WriteLineIf(DebugSwitch.TaskQueue,"New Task Queue");

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
                        Debug.WriteLineIf(DebugSwitch.TaskQueue, "Task count is " + tasks.Count);
                        List<Action> currentTasks = new List<Action>();
                        //后进来的先处理，每次处理3倍的线程数份，这样可以让后进来的尽快处理
                        for(int i=0;i< Configs.RefreshThreadCount * 3;i++)
                        {
                            if(tasks.Count>0)
                            {
                                if(tasks.TryPop(out Action act))
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
                    stopping = false;
                    ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(false));
                    lock (isExcuting)
                    {
                        isExcuting = false;
                    }
                }
                //else
                //{
                //    tasks.Enqueue(t);
                //}

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


////#define SingleThread

//using ClassifyFiles.Util;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ClassifyFiles.Util
//{
//    public class TaskQueue
//    {
//        public event EventHandler<ProcessStatusChangedEventArgs> ProcessStatusChanged;

//        private readonly ConcurrentDictionary<string, ConcurrentQueue<Action>> tasks = new ConcurrentDictionary<string, ConcurrentQueue<Action>>();
//        private object isExcuting = false;
//        public bool IsExcuting => (bool)isExcuting;
//        bool stopping = false;

//        /// <summary>
//        /// 将新的任务加入队列
//        /// </summary>
//        /// <param name="t"></param>
//        /// <remarks>
//        /// 当一个Task需要加入队列时，首先判断是否当前有任务正在执行。
//        /// 如果有正在执行的队列，那么就把任务加入队列。
//        /// 如果没有正在执行的任务，就先执行该任务。
//        /// 执行任务的时候，可能有其他任务加了进来。
//        /// 执行完成后，把队列中的任务都提取出来，然后多线程循环处理开始时队列中的所有任务。
//        /// 在处理这些任务的时候，有可能又有新的任务加了进来，
//        /// 所以需要循环处理，直到队列为空。
//        /// </remarks>
//        public Task Enqueue(Action t, string tag)
//        {
//            return Task.Run(() =>
//            {
//                if (!tasks.ContainsKey(tag))
//                {
//                    tasks.TryAdd(tag, new ConcurrentQueue<Action>());
//                }
//                tasks[tag].Enqueue(t);
//                if (false.Equals(isExcuting))
//                {
//                    lock (isExcuting)
//                    {
//                        //多个请求可能会同时执行，所以先要把变量锁了，然后之后的解锁之后就会发现已经在执行了，就加入队列
//                        if (isExcuting.Equals(true))
//                        {
//                            tasks[tag].Enqueue(t);
//                            return;
//                        }
//                        isExcuting = true;
//                    }
//                    Debug.WriteLine("New Task Queue");

//                    ProcessStatusChanged?.Invoke(this, new ProcessStatusChangedEventArgs(true));
//                    try
//                    {
//                        t();
//                    }
//                    catch (Exception ex)
//                    {

//                    }
//                    while (tasks[tag].Count > 0 && !stopping)
//                    {
//                        Debug.WriteLine("Task count is " + tasks.Count);

//                        var currentTasks = tasks[tag].ToArray();
//                        tasks[tag].Clear();
//#if (SingleThread &&DEBUG)

//                        foreach (var task in currentTasks)
//                        {
//                            if (stopping)
//                            {
//                                break;
//                            }
//                            try
//                            {
//                                //执行任务
//                                task();
//                            }
//                            catch (Exception ex)
//                            {

//                            }
//                        }

//#else
//                        ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = Configs.RefreshThreadCount };
//                        Parallel.ForEach(currentTasks, opt, (t2, e) =>
//                        {
//                            if (stopping)
//                            {
//                                e.Stop();
//                            }
//                            try
//                            {
//                                //执行任务
//                                t2();
//                            }
//                            catch (Exception ex)
//                            {

//                            }
//                        });
//#endif
//                    }
//                    if (!stopping)
//                    {
//                        //防止频繁保存数据库
//                        if ((DateTime.Now - lastDbSaveTime).Seconds > 10)
//                        {
//                            lastDbSaveTime = DateTime.Now;
//                            try
//                            {
//                                DbUtility.SaveChanges();
//                            }
//                            catch (Exception ex)
//                            {

//                            }
//                        }
//                    }
//                    stopping = false;
//                    ProcessStatusChanged?.Invoke(this,
//                        new ProcessStatusChangedEventArgs(tasks.Values
//                        .Cast<ConcurrentQueue<Action>>().Any(p => p.Count > 0)));
//                    lock (isExcuting)
//                    {
//                        isExcuting = false;
//                    }
//                }
//                else
//                {
//                    tasks[tag].Enqueue(t);
//                }

//            });
//        }
//        private DateTime lastDbSaveTime = DateTime.MinValue;

//        public async Task Stop()
//        {
//            if (!IsExcuting)
//            {
//                return;
//            }
//            stopping = true;
//            while (IsExcuting)
//            {
//                await Task.Delay(1);
//            }
//        }
//    }
//    public class ProcessStatusChangedEventArgs : EventArgs
//    {
//        public ProcessStatusChangedEventArgs(bool isRunning)
//        {
//            IsRunning = isRunning;
//        }

//        public bool IsRunning { get; }
//    }

//}
