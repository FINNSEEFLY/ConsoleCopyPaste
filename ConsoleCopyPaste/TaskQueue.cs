using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleCopyPaste
{
    public delegate void TaskDelegate();

    public class TaskQueue : IDisposable
    {
        private Queue<TaskDelegate> _taskPull;
        private List<Thread> _threadPull;
        private bool disposed = false;
        private bool isProcessing = false;

        public TaskQueue()
        {
            _taskPull = new Queue<TaskDelegate>();
            _threadPull = new List<Thread>();
        }

        public TaskQueue(int numOfThreads)
        {
            _taskPull = new Queue<TaskDelegate>();
            _threadPull = new List<Thread>();
            for (var i = 0; i < numOfThreads; i++)
            {
                CreateThread();
            }

            var processingThread = new Thread(ProcessingTasks);
            processingThread.Start();
        }

        public void EnqueueTask(TaskDelegate task)
        {
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

        private void ExecuteTask(object taskDelegate)
        {
            Console.WriteLine("Начал работу поток " + Thread.CurrentThread.ManagedThreadId);
            var task =(TaskDelegate) taskDelegate;
            task();
            Console.WriteLine("Закончил работу поток " + Thread.CurrentThread.ManagedThreadId);
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