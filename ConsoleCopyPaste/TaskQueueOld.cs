using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleCopyPaste
{

    public class TaskQueueOld : IDisposable
    {
        private Queue<TaskDelegate> _taskPull;
        private List<Thread> _threadPull;
        private bool disposed = false;
        private bool isProcessing = false;
        
        
        public TaskQueueOld()
        {
            _taskPull = new Queue<TaskDelegate>();
            _threadPull = new List<Thread>();
        }

        public TaskQueueOld(int numOfThreads)
        {
            _taskPull = new Queue<TaskDelegate>();
            _threadPull = new List<Thread>();
            for (var i = 0; i < numOfThreads; i++)
            {
                CreateThread();
            }
        }

        public void EnqueueTask(TaskDelegate task)
        {
            if (disposed)
                throw new ObjectDisposedException(null);
            _taskPull.Enqueue(task);
            if (!isProcessing)
            {
                isProcessing = true;
                new Thread(ProcessingTasks).Start();
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
        }

        private void ProcessingTasks()
        {
            while (!disposed && _taskPull.Count != 0)
            {
                var task = _taskPull.Dequeue();
                Thread thread = null;
                while (thread == null && !disposed)
                {
                    thread = _threadPull.FirstOrDefault(x => x.ThreadState == ThreadState.Unstarted);
                    if (thread == null)
                    {
                        RestoreThreads();
                    }
                    else
                        thread.Start(task);
                }
            }
            isProcessing = false;
        }
        
        private void CreateThread()
        {
            _threadPull.Add(new Thread(ExecuteTask));
        }

        private static void ExecuteTask(object taskDelegate)
        {
            var task =(TaskDelegate) taskDelegate;
            task();
        }
        
        private void RestoreThreads()
        {
            var numOfThreads = _threadPull.RemoveAll(x => !x.IsAlive && x.ThreadState == ThreadState.Stopped);
            if (numOfThreads == 0)
            {
                try
                {
                    CreateThread();
                }
                catch
                {
                    Thread.Sleep(2000);
                }
            }
            else
            {
                for (var i = 0; i < numOfThreads; i++)
                {
                    CreateThread();
                }
            }
        }
    }

}