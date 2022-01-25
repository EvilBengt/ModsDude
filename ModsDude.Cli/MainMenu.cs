using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Cli;

internal class MainMenu
{
    private readonly ProfileManager _profileManager;
    private readonly CreateProfileMenu _createProfileMenu;
    private readonly SelectProfileMenu _selectProfileMenu;
    private Dictionary<string, Func<Task>> _options;


    public MainMenu(ProfileManager profileManager,
                    CreateProfileMenu createProfileMenu,
                    SelectProfileMenu selectProfileMenu)
    {
        _profileManager = profileManager;
        _createProfileMenu = createProfileMenu;
        _selectProfileMenu = selectProfileMenu;
        _options = new()
        {
            { "1", NewProfile },
            { "2", UpdateRemote },
            { "3", SelectProfile },
            { "4", ApplyProfile }
        };
    }


    public async Task Run()
    {
        while (true)
        {
            View();
            await AcceptInput();
        }
    }


    private void View()
    {
        Utils.PrintHeader();
        Console.WriteLine("1. New profile");
        Console.WriteLine("2. Update remote");
        Console.WriteLine();
        if (_profileManager.SelectedProfile is null)
        {
            Console.WriteLine("3. Select profile");
        }
        else
        {
            Console.WriteLine($"3. Change profile ({_profileManager.SelectedProfile})");
            Console.WriteLine("   4. Apply profile");
        }
    }

    private async Task AcceptInput()
    {
        string? input = Console.ReadLine();

        if (input is null)
        {
            return;
        }

        if (_options.TryGetValue(input, out Func<Task>? action))
        {
            await action();
        }
    }

    private Task NewProfile()
    {
        return _createProfileMenu.Run();
    }

    private Task UpdateRemote()
    {
        throw new NotImplementedException();
    }

    private Task SelectProfile()
    {
        return _selectProfileMenu.Run();
    }

    private Task ApplyProfile()
    {
        throw new NotImplementedException();
    }
}
