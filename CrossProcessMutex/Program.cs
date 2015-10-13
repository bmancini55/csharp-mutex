using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossProcessMutex
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Please provide a file path");
                return;
            }

            var path = args[0];
            var mutexName = "Global\\" + path.Replace("\\", ""); // mutex names don't like \

            // create a global mutex
            using (var mutex = new Mutex(false, mutexName))
            {
                Console.WriteLine("Waiting for mutex");
                var mutexAcquired = false;
                try
                {
                    // acquire the mutex (or timeout after 60 seconds)
                    // will return false if it timed out
                    mutexAcquired = mutex.WaitOne(60000);
                }
                catch(AbandonedMutexException)
                {
                    // abandoned mutexes are still acquired, we just need
                    // to handle the exception and treat it as acquisition
                    mutexAcquired = true;
                }

                // if it wasn't acquired, it timed out, so can handle that how ever we want
                if (!mutexAcquired)
                {
                    Console.WriteLine("I have timed out acquiring the mutex and can handle that somehow");
                    return;
                }
                
                // otherwise, we've acquired the mutex and should do what we need to do,
                // then ensure that we always release the mutex
                try
                {
                    DoWork(path);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                
            }
        }

        static void DoWork(string path)
        {
            Console.WriteLine("Mutex acquired");

            // delay for a little so we can see some blocking
            Thread.Sleep(15000);

            // create the specified directory if it doesn't exist
            Console.WriteLine("Creating directory " + path);
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
                directory.Create();

            // write a file to it
            var filePath = Path.Combine(path, DateTime.UtcNow.Ticks.ToString());
            Console.WriteLine("Creating file " + filePath);
            var file = new FileInfo(filePath);
            file.Create();
        }
    }
}
