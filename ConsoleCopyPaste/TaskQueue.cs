using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ConsoleCopyPaste
{
    public delegate void TaskDelegate();
    
    public class TaskQueue : IDisposable
    {
        private readonly ConcurrentQueue<TaskDelegate> _taskPull;
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
            if (disposed)
                throw new ObjectDisposedException(null);
            _taskPull.Enqueue(task);
        }

        public void Dispose()
        {
            if (disposed) return;
                disposed = true;
        }

        private void ExecuteTasks()
        {
            while (!disposed)
            {
                _taskPull.TryDequeue(out var task);
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