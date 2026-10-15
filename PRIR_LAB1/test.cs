
using System;
using System.Threading;
using System.Threading.Tasks;

int zloze = 2000;
int magazyn = 0;
int liczbaGornikow = 5;

object blokada = new object();

SemaphoreSlim kopalnia = NewMethod();
SemaphoreSlim sklad = new SemaphoreSlim(1);

void Gornik(int numer)
{
    while (true)
    {
        int ladunek = 0;

        kopalnia.Wait();

        for (int i = 0; i < 200; i++)
        {
            Thread.Sleep(10);

            lock (blokada)
            {
                if (zloze == 0) break;

                zloze--;
                ladunek++;
            }
        }

        kopalnia.Release();

        if (ladunek == 0) break;

        Console.WriteLine($"Gornik {numer} wydobyl {ladunek}");

        // Transport do magazynu
        Thread.Sleep(10000);

        sklad.Wait();

        for (int i = 0; i < ladunek; i++)
        {
            Thread.Sleep(10);

            lock (blokada)
                magazyn++;
                
        }

        sklad.Release();

        Console.WriteLine($"Gornik {numer} rozladowal wegiel");

        lock (blokada)
        {
            if (zloze == 0) break;
        }

        // Powrot do kopalni
        Thread.Sleep(10000);
    }
}

Task[] zadania = new Task[liczbaGornikow];

for (int i = 0; i < liczbaGornikow; i++)
{
    int numer = i + 1;
    zadania[i] = Task.Run(() => Gornik(numer));
}

Task.WaitAll(zadania);

Console.WriteLine($"Pozostalo w zlozu: {zloze}");
Console.WriteLine($"W magazynie: {magazyn}");

static SemaphoreSlim NewMethod()
{
    return new SemaphoreSlim(2);
}