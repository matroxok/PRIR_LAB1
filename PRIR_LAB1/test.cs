using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Kopalnia
{
    class Program
    {
        static int coalDeposit = 2000;      // ile węgla jest 
        static int warehouse = 0;           // ile węgla jest w magazynie
        static int vehicleCapacity = 200;   // pojemność pojazdu
        static int mineTime = 10;           // ms na wydobycie 1 jednostki
        static int unloadTime = 10;         // ms na rozładowanie 1 jednostki
        static int travelTime = 10000;      // ms przejazdu do magazynu (10 s)


        static SemaphoreSlim mineSemaphore = new SemaphoreSlim(2, 2);


        static SemaphoreSlim warehouseSemaphore = new SemaphoreSlim(1, 1);


        static object lockObject = new object();

        static void Main(string[] args)
        {
            int minersCount = 5; 

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Task[] tasks = new Task[minersCount];

            for (int i = 0; i < minersCount; i++)
            {
                int minerId = i + 1;  
                tasks[i] = Task.Run(() => Miner(minerId));
            }

            Task.WaitAll(tasks);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Cały węgiel przeniesiony do magazynu.");
            Console.WriteLine("Stan magazynu: " + warehouse + " jednostek.");
            Console.WriteLine("Liczba górników: " + minersCount + ", czas: " + stopwatch.Elapsed.TotalSeconds + " s");
        }

        static void Miner(int id)
        {
            while (true)
            {
                mineSemaphore.Wait();

                int mined = 0;
                while (mined < vehicleCapacity)
                {
                    bool gotCoal = false;

                    lock (lockObject)
                    {
                        if (coalDeposit > 0)
                        {
                            coalDeposit--;
                            gotCoal = true;
                        }
                    }

                    if (gotCoal == false)
                    {
                        break;  
                    }

                    Thread.Sleep(mineTime);
                    mined++;
                }

                mineSemaphore.Release();   


                if (mined == 0)
                {
                    Console.WriteLine("Górnik " + id + " zakończył pracę.");
                    break;
                }

                int left;
                lock (lockObject)
                {
                    left = coalDeposit;
                }
                Console.WriteLine("Górnik " + id + " wydobył " + mined + " jednostek węgla. Pozostało w złożu: " + left + " jednostek.");


                Console.WriteLine("Górnik " + id + " transportuje węgiel do magazynu...");
                Thread.Sleep(travelTime);


                warehouseSemaphore.Wait();

                Console.WriteLine("Górnik " + id + " rozładowuje węgiel...");
                for (int i = 0; i < mined; i++)
                {
                    Thread.Sleep(unloadTime);
                    lock (lockObject)
                    {
                        warehouse++;
                    }
                }

                warehouseSemaphore.Release();
            }
        }
    }
}