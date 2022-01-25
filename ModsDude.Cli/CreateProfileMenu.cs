using ModsDude.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Cli;

internal class CreateProfileMenu
{
    private readonly Remote _remote;


    public CreateProfileMenu(Remote remote)
    {
        _remote = remote;
    }


    public async Task Run()
    {
        while (true)
        {
            View();
            if (await AcceptInput())
            {
                Utils.Pause();
                return;
            }
        }
    }

    
    private void View()
    {
        Utils.PrintHeader();
        Console.WriteLine("Profile Name:");
    }

    private async Task<bool> AcceptInput()
    {
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            await _remote.CreateProfile(input.Trim());
            Console.WriteLine("Profile created.");
        }
        catch (Exception ex)
        {
            Utils.PrintException(ex);
        }

        return true;
    }
}
