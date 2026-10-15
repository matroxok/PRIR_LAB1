using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRIR_LAB1
{
    class Program
    {
        static int sharedResource = 0;
        static SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);

        static void Main(string[] args)
        {
            Task[] tasks = new Task[5];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => AccessSharedResource());
            }

            Task.WaitAll(tasks);
            Console.WriteLine($"Końcowa wartość zasobu: {sharedResource}");
        }

        static void AccessSharedResource()
        {
            for (int i = 0; i < 5; i++)
            {
                semaphore.Wait();
                try
                {
                    sharedResource++;
                    Console.WriteLine($"Wątek {Task.CurrentId} zmodyfikował zasób na: {sharedResource}");
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }
    }
}