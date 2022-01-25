using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Cli;

internal static class Utils
{
    public static void PrintHeader(string breadcrumbs = "")
    {
        Console.Clear();
        Console.WriteLine("======== ModsDude v1 ========");
        Console.WriteLine(breadcrumbs);
        Console.WriteLine();
    }

    public static void PrintException(Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("ERROR: " + ex.Message);
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
