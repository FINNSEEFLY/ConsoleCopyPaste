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
        private bool isProcessing = true;
        public int CopyCounter { get; set; } = 0;

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

            var processingThread = new Thread(Processing);
            processingThread.Start();
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _taskPull.Enqueue(task);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            isProcessing = false;
        }

        private void Processing()
        {
            var downtimeCounter = 0;
            var onceWorked = false;
            while (isProcessing)
            {
                if (_taskPull.Count == 0)
                {
                    if (onceWorked)
                    {
                        downtimeCounter++;
                        if (downtimeCounter > 10)
                        {
                            downtimeCounter = 0;
                            Thread.Sleep(5000);
                        }
                    }
                    continue;
                }
                onceWorked = true;
                var task = _taskPull.Dequeue();
                Thread thread = null;
                while (thread == null && isProcessing)
                {
                    thread = _threadPull.FirstOrDefault(x => x.ThreadState == ThreadState.Unstarted);
                    if (thread == null) 
                        RestoreThreads();
                    else
                        thread.Start(task);
                }
            }
        }

        private void RestoreThreads()
        {
            var numOfThreads = _threadPull.RemoveAll(x => !x.IsAlive && x.ThreadState == ThreadState.Stopped);
            if (numOfThreads == 0)
            {
                CreateThread();
            }
            else
            {
                for (var i = 0; i < numOfThreads; i++)
                {
                    CreateThread();
                }
            }
        }

        private void CreateThread()
        {
            var thread = new Thread(ExecuteTask);
            _threadPull.Add(thread);
        }

        private void ExecuteTask(object taskDelegate)
        {
            Console.WriteLine("Начал работу поток " + Thread.CurrentThread.ManagedThreadId);
            var task = (TaskDelegate) taskDelegate;
            task();
            CopyCounter++;
            Console.WriteLine("Закончил работу поток " + Thread.CurrentThread.ManagedThreadId);
        }
    }
}