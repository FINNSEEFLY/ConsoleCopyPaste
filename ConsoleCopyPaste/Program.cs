using System;
using System.IO;
using System.Threading;


namespace ConsoleCopyPaste
{
    class Program
    {
        static void Main(string[] args)
        {
            const int NUM_OF_THREADS = 100;
            string srcPath = null, dstPath = null;
            if (!ParseArgs(args, ref srcPath, ref dstPath))
            {
                Console.WriteLine("Не верные входные данные");
                return;
            }

            if (!Directory.Exists(srcPath) || !Directory.Exists(dstPath))
            {
                Console.WriteLine("Некорректные директории");
                return;
            }

            TaskQueue taskQueue = new TaskQueue(NUM_OF_THREADS);
            CopyDirectory(srcPath, dstPath, taskQueue);
            Console.ReadKey();
            Console.WriteLine("Всего было скопировано: {0}",taskQueue.CopyCounter);
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
                taskQueue.EnqueueTask(() => CopyFile(filePath, dstPath + @"\" + Path.GetFileName(filePath)));
            }

            foreach (var directory in directories)
            {
                CopyDirectory(directory, dstPath, taskQueue);
            }
        }

        static void CopyFile(string srcPath, string dstPath)
        {
            File.Copy(srcPath, dstPath, true);
            Console.WriteLine("Файл [" + srcPath + "] был скопирован в [" + dstPath + "]");
        }
    }
}