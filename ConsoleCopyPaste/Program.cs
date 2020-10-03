using System;
using System.IO;
using System.Threading;


namespace ConsoleCopyPaste
{
    class Program
    {
        private static int CopyCounter { get; set; } = 0;
        private static int ThreadEnded { get; set; } = 0;
        private static int TargetCount { get; set; } = 0;
        private static object threadEndedLock = new object();
        private static object CopyCounterLock = new object();
        static void Main(string[] args)
        {
            const int NUM_OF_THREADS = 100;
            string srcPath = null, dstPath = null;
            if (!ParseArgs(args, ref srcPath, ref dstPath))
            {
                Console.WriteLine("Использование:\t"+"ConsoleCopyPaste SourcePath DestinationPath");
                return;
            }

            if (!Directory.Exists(srcPath) || !Directory.Exists(dstPath))
            {
                Console.WriteLine("Некорректные директории");
                return;
            }
            TaskQueue taskQueue = new TaskQueue(NUM_OF_THREADS);
            CopyDirectory(srcPath, dstPath, taskQueue);
            while (TargetCount != ThreadEnded)
            {
                Thread.Sleep(300);
            }
            Console.WriteLine("Всего файлов: {0}, удачно скопированно: {1}",TargetCount, CopyCounter);
            taskQueue.Dispose();
        }

        static bool ParseArgs(string[] args, ref string srcPath, ref string dstPath)
        {
            if (args == null || args.Length != 2 || args[0] == null || args[0] == "" ||
                args[1] == null || args[1] == "") return false;
            srcPath = args[0];
            dstPath = args[1];
            return true;
        }

        static void CopyDirectory(string srcPath, string dstPath, TaskQueue taskQueue)
        {
            var files = Directory.GetFiles(srcPath);
            var directories = Directory.GetDirectories(srcPath);
            dstPath = dstPath + @"\" + Path.GetFileName(srcPath);
            Directory.CreateDirectory(dstPath);
            foreach (var filePath in files)
            {
                TargetCount++;
                taskQueue.EnqueueTask(() => CopyFile(filePath, dstPath + @"\" + Path.GetFileName(filePath)));
            }

            foreach (var directory in directories)
            {
                CopyDirectory(directory, dstPath, taskQueue);
            }
        }

        static void CopyFile(string srcPath, string dstPath)
        {
            try
            {
                File.Copy(srcPath, dstPath, true);
                lock (CopyCounterLock)
                {
                    CopyCounter++;
                }
            }
            catch
            {
                // ignored
            }

            Console.WriteLine("Файл [" + srcPath + "] был скопирован в [" + dstPath + "]");
            lock (threadEndedLock)
            {
                ThreadEnded++;
            }
        }

    }
}