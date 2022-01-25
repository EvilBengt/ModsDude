using ModsDude.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Cli;

internal class SelectProfileMenu
{
    private readonly Remote _remote;
    private readonly ProfileManager _profileManager;
    private List<string>? _profiles;


    public SelectProfileMenu(Remote remote, ProfileManager profileManager)
    {
        _remote = remote;
        _profileManager = profileManager;
        _profiles = new();
    }


    public async Task Run()
    {
        try
        {
            while (true)
            {
                ViewLoading();

                await FetchProfiles();

                View();

                if (AcceptInput())
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Utils.PrintException(ex);
            Utils.Pause();
            return;
        }
    }


    private static void ViewLoading()
    {
        Utils.PrintHeader("/ Select Profile");

        Console.WriteLine();
        Console.WriteLine("Fetching profile list...");
    }

    private void View()
    {
        Utils.PrintHeader("/ Select Profile");

        Console.WriteLine();
        Console.WriteLine("q. <--");
        Console.WriteLine();
        int counter = 1;
        foreach (string profile in _profiles!)
        {
            Console.WriteLine(counter.ToString() + $". {profile}");
            counter++;
        }
    }

    private async Task FetchProfiles()
    {
        _profiles = (await _remote.FetchProfiles()).ToList();
    }

    private bool AcceptInput()
    {
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (input.Trim() == "q")
        {
            return true;
        }

        if (int.TryParse(input, out int result))
        {
            try
            {
                _profileManager.SelectedProfile = _profiles![result - 1];

                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        return false;
    }
}
