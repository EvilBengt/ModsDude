using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using ModsDude.Core.Models;
using ModsDude.Core.Models.Settings;

namespace ModsDude.Core.Services;

public class ModBrowser
{
    private readonly ApplicationSettings _settings;
    private readonly MD5 _hashAlgorithm;


    public ModBrowser(ApplicationSettings settings, string defaultImportPath)
    {
        _settings = settings;
        DefaultImportPath = defaultImportPath;
        FileOperation = new();

        _hashAlgorithm = MD5.Create();
    }


    public string DefaultImportPath { get; }
    public FileOperation FileOperation { get; }


    public IEnumerable<FileInfo> GetCached()
    {
        return GetFiles(_settings.GetValidCacheFolder());
    }

    public IEnumerable<FileInfo> GetActive()
    {
        return GetFiles(_settings.GetValidModsFolder());
    }

    public Task CacheAsync(IEnumerable<FileInfo> files)
    {
        return Task.Run(() => Cache(files));
    }

    public void Cache(IEnumerable<FileInfo> files)
    {
        FileOperation.OnStart(files.Sum(file => file.Length));

        foreach (FileInfo file in files)
        {
            string destinationPath = Path.Combine(_settings.GetValidCacheFolder(), file.Name);
            long size = file.Length;

            file.MoveTo(destinationPath, true);

            FileOperation.OnIncrement(size);
        }
    }

    public Task ActivateAsync(IEnumerable<FileInfo> files)
    {
        return Task.Run(() =>
        {
            FileOperation.OnStart(files.Sum(file => file.Length));

            foreach (FileInfo file in files)
            {
                long size = file.Length;

                file.MoveTo(Path.Combine(_settings.GetValidModsFolder(), file.Name), true);

                FileOperation.OnIncrement(size);
            }
        });
    }

    public void Activate(IEnumerable<FileInfo> files)
    {
        FileOperation.OnStart(files.Sum(file => file.Length));

        foreach (FileInfo file in files)
        {
            long size = file.Length;

            file.MoveTo(Path.Combine(_settings.GetValidModsFolder(), file.Name));

            FileOperation.OnIncrement(size);
        }
    }

    public bool HasFile(string filename)
    {
        return FindFile(filename) is not null;
    }

    public long GetFileSizeOfActive(string filename)
    {
        FileInfo fileInfo = new(Path.Combine(_settings.GetValidModsFolder(), filename));

        return fileInfo.Length;
    }

    public FileInfo? FindFile(string filename)
    {
        FileInfo inMods = new(Path.Combine(_settings.GetValidModsFolder(), filename));
        if (inMods.Exists)
        {
            return inMods;
        }

        FileInfo inCache = new(Path.Combine(_settings.GetValidCacheFolder(), filename));
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

    public void Import(string fullName)
    {
        FileInfo file = new(fullName);

        file.MoveTo(Path.Combine(_settings.GetValidModsFolder(), Path.GetFileName(fullName)), true);
    }

    public Task Recycle(IEnumerable<FileInfo> files)
    {
        return Task.Run(() =>
        {
            FileOperation.OnStart(files.Sum(file => file.Length));

            foreach (FileInfo file in files)
            {
                long size = file.Length;

                FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                FileOperation.OnIncrement(size);
            }
        });
    }

    public async Task SaveStreamAsActiveAsync(Stream stream, string filename)
    {
        FileStream fileStream = File.Open(Path.Combine(_settings.GetValidModsFolder(), filename), FileMode.Create);

        await CopyStreamWithProgress(stream, fileStream);

        stream.Dispose();
        fileStream.Dispose();
    }


    /// <summary>
    /// Adapted from https://stackoverflow.com/a/69826649/5696900
    /// </summary>
    private async Task CopyStreamWithProgress(Stream from, Stream to)
    {
        long totalBytesRead = 0;
        long readCount = 0;
        byte[] buffer = new byte[8192];
        bool isMoreToRead = true;
        long bytesReadSinceLastReport = 0;

        do
        {
            int bytesRead = await from.ReadAsync(buffer);
            if (bytesRead == 0)
            {
                isMoreToRead = false;

                FileOperation.OnIncrement(bytesRead);

                continue;
            }

            await to.WriteAsync(buffer.AsMemory(0, bytesRead));

            totalBytesRead += bytesRead;
            bytesReadSinceLastReport += bytesRead;
            readCount++;

            if (readCount % 10 == 0)
            {
                FileOperation.OnIncrement(bytesReadSinceLastReport);
                bytesReadSinceLastReport = 0;
            }
        }
        while (isMoreToRead);
    }

    private static IEnumerable<FileInfo> GetFiles(string path)
    {
        return Directory.EnumerateFiles(path).Select(fullName => new FileInfo(fullName));
    }


    ~ModBrowser()
    {
        _hashAlgorithm.Dispose();
    }
}
