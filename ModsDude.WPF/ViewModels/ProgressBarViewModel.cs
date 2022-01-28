using ModsDude.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.WPF.ViewModels;

internal class ProgressBarViewModel : ViewModel
{
    FileOperation? _fileOperation;


    private long _maximum = 1;
    public long Maximum
    {
        get
        {
            return _maximum;
        }
        set
        {
            _maximum = value;
            OnPropertyChanged();
        }
    }

    private long _value = 0;
    public long Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }


    public void Bind(FileOperation fileOperation)
    {
        Reset();

        _fileOperation = fileOperation;

        _fileOperation.Start += Start;
        _fileOperation.Increment += Increment;
    }

    public void Reset()
    {
        if (_fileOperation is not null)
        {
            _fileOperation.Start -= Start;
            _fileOperation.Increment -= Increment;
        }

        Value = 0;
        Maximum = 1;
    }


    private void Start(long totalBytes)
    {
        Maximum = totalBytes;
        Value = 0;
    }

    private void Increment(long bytes)
    {
        Value += bytes;
    }
}
