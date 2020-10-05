using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ConsoleCopyPaste
{
    public delegate void TaskDelegate();
    
    public class TaskQueue : IDisposable
    {
        private ConcurrentQueue<TaskDelegate> _taskPull;
        private bool disposed = false;
        
        public TaskQueue(int numOfThreads)
        {
            _taskPull = new ConcurrentQueue<TaskDelegate>();
            for (var i = 0; i < numOfThreads; i++)
            {
                new Thread(ExecuteTasks).Start();
            }
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _taskPull.Enqueue(task);
        }

        public void Dispose()
        {
            if (disposed) return;
                disposed = true;
        }

        public void ExecuteTasks()
        {
            TaskDelegate task;
            while (!disposed)
            {
                _taskPull.TryDequeue(out task);
                if (task == null)
                {
                    Thread.Sleep(30);
                }
                else
                {
                    task();
                }
            }
        }
    }
}