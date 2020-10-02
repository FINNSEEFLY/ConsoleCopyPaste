using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleCopyPaste
{
    public delegate void TaskDelegate();

    public class TaskQueue
    {
        private Queue<TaskDelegate> _taskPull;
        private List<Thread> _threadPull;
        private bool isProcessing { get; set; }

        public TaskQueue()
        {
            _taskPull = new Queue<TaskDelegate>();
            _threadPull = new List<Thread>();
            isProcessing = false;
        }

        public TaskQueue(int numOfThreads)
        {
            _taskPull = new Queue<TaskDelegate>();
            _threadPull = new List<Thread>();
            isProcessing = false;
            for (var i = 0; i < numOfThreads; i++)
            {
                var thread = new Thread(ExecuteTask);
                thread.Name = "Thread[" + i + "]";
                _threadPull.Add(thread);
            }
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _taskPull.Enqueue(task);
        }

        public void StartProcessing()
        {
            isProcessing = true;
            Thread processingThread = new Thread(Processing);
            processingThread.Start();
        }

        public void StopProcessing()
        {
            isProcessing = false;
        }

        private void Processing()
        {
            while (isProcessing)
            {
                if (_taskPull.Count == 0) continue;
                var task = _taskPull.Dequeue();
                Thread thread = null;
                while (thread == null && isProcessing)
                {
                    thread = _threadPull.FirstOrDefault(x => x.ThreadState==ThreadState.Unstarted);
                    thread?.Start(task);
                }
            }
        }

        private static void ExecuteTask(object taskDelegate)
        {
            Console.WriteLine("Начал работу поток " + Thread.CurrentThread.Name);
            TaskDelegate task = (TaskDelegate) taskDelegate;
            task();
            Console.WriteLine("Закончил работу поток " + Thread.CurrentThread.Name);
        }
    }
}