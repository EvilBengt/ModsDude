using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Models;

public class FileOperation
{
    public delegate void StartDelegate(long totalBytes);
    public event StartDelegate? Start;

    public delegate void IncrementDelegate(long increment);
    public event IncrementDelegate? Increment;


    public void OnStart(long totalBytes)
    {
        Start?.Invoke(totalBytes);
    }

    public void OnIncrement(long increment)
    {
        Increment?.Invoke(increment);
    }
}
