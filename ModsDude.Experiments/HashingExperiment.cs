using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ModsDude.Experiments;

internal class HashingExperiment
{
    public void Run()
    {
        using MD5 md5 = MD5.Create();

        DateTime start = DateTime.Now;

        foreach (string filename in Directory.EnumerateFiles(@"C:\Users\Anton\Documents\My Games\FarmingSimulator2022\mods"))
        {
            Console.WriteLine(filename);
            using FileStream stream = File.OpenRead(filename);

            byte[] checksum = md5.ComputeHash(stream);
            Console.WriteLine(BitConverter.ToString(checksum));
        }

        Console.WriteLine(start);
        Console.WriteLine(DateTime.Now);
        Console.WriteLine(DateTime.Now - start);

        Console.ReadKey();
    }
}
