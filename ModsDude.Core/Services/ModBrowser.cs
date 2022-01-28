using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Services;

public class ModBrowser
{
    private readonly MD5 _hashAlgorithm;
    private string _modsFolderPath;
    private string _cacheFolderPath;


    public ModBrowser(string modsFolderPath, string cacheFolderPath)
    {
        _modsFolderPath = modsFolderPath;
        _cacheFolderPath = cacheFolderPath;

        _hashAlgorithm = MD5.Create();
    }


    public IEnumerable<string> GetCached()
    {
        return GetFiles(_cacheFolderPath);
    }

    public IEnumerable<string> GetActive()
    {
        return GetFiles(_modsFolderPath);
    }

    public void Cache(IEnumerable<string> filenames)
    {
        foreach (string filename in filenames)
        {
            string sourcePath = Path.Combine(_modsFolderPath, filename);
            string destinationPath = Path.Combine(_cacheFolderPath, filename);

            try
            {
                File.Move(sourcePath, destinationPath, true);
            }
            catch (FileNotFoundException)
            {
            }
        }
    }

    public IEnumerable<string> Activate(IEnumerable<string> filenames)
    {
        List<string> notFound = new();

        foreach (string filename in filenames)
        {
            string sourcePath = Path.Combine(_modsFolderPath, filename);
            string destinationPath = Path.Combine(_cacheFolderPath, filename);

            if (!File.Exists(sourcePath))
            {
                notFound.Add(filename);
                continue;
            }

            try
            {
                File.Move(sourcePath, destinationPath, true);
            }
            catch (FileNotFoundException)
            {
                notFound.Add(filename);
            }
        }

        return notFound;
    }

    public bool HasFile(string filename)
    {
        return FindFile(filename) is not null;
    }

    public long GetFileSizeOfActive(string filename)
    {
        FileInfo fileInfo = new(Path.Combine(_modsFolderPath, filename));

        return fileInfo.Length;
    }

    public FileInfo? FindFile(string filename)
    {
        FileInfo inMods = new(Path.Combine(_modsFolderPath, filename));
        if (inMods.Exists)
        {
            return inMods;
        }

        FileInfo inCache = new(Path.Combine(_cacheFolderPath, filename));
        if (inCache.Exists)
        {
            return inCache;
        }

        return null;
    }

    public Task<string> Hash(string fullPath)
    {
        return Task.Run(() =>
        {
            using FileStream stream = File.OpenRead(fullPath);

            byte[] checksum = _hashAlgorithm.ComputeHash(stream);

            return Convert.ToBase64String(checksum);
        });
    }


    private IEnumerable<string> GetFiles(string path)
    {
        return Directory.EnumerateFiles(path).Select(mod => Path.GetFileName(mod));
    }


    ~ModBrowser()
    {
        _hashAlgorithm.Dispose();
    }
}
