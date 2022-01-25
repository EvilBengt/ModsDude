using ModsDude.Cli;
using ModsDude.Core.Services;


Remote remote = new();

ProfileManager profileManager = new();

CreateProfileMenu createProfileMenu = new(remote);
SelectProfileMenu selectProfileMenu = new(remote, profileManager);
MainMenu mainMenu = new(
    profileManager,
    createProfileMenu,
    selectProfileMenu);

await mainMenu.Run();
