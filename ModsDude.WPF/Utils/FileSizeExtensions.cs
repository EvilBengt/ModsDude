using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.WPF.Utils;

public static class FileSizeExtensions
{
    /// <summary>
    /// Credit: Stack overflow user DKH, posted 28 Mar 2018, fetched 27 Jan 2022
    /// Link: https://stackoverflow.com/a/49535675/5696900
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToBytesCount(this long bytes)
    {
        int unit = 1024;
        string unitStr = "B";
        if (bytes < unit)
        {
            return string.Format("{0} {1}", bytes, unitStr);
        }
        int exp = (int)(Math.Log(bytes) / Math.Log(unit));
        return string.Format("{0:##.##} {1}{2}", bytes / Math.Pow(unit, exp), "KMGTPEZY"[exp - 1], unitStr);
    }
}
