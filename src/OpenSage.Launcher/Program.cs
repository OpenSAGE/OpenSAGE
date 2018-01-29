using OpenSage.Data;
using OpenSage.LowLevel;
using OpenSage.Mods.BuiltIn;

namespace OpenSage.Launcher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            HostPlatform.Start();

            Platform.CurrentPlatform = new Sdl2Platform();
            Platform.CurrentPlatform.Start();

            // TODO: Get the game from a launch parameter.
            // TODO: Support other locators.
            var game = GameFactory.CreateGame(
                GameDefinition.FromGame(SageGame.CncGenerals),
                new RegistryInstallationLocator(),
                // TODO: Read game version from assembly metadata or .git folder
                () => Platform.CurrentPlatform.CreateWindow("OpenSAGE (master)", 100, 100, 1024, 768));

            // TODO: Set window icon.

            SetupInitialScene(game);

            while (game.IsRunning)
            {
                game.Tick();
            }

            Platform.CurrentPlatform.Stop();
            HostPlatform.Stop();
        }

        // TODO: Extract this logic into a game-specific DLL, or make the scene and menu configurable.
        // TODO: Implement fast startup, where the shellmap is not loaded.
        private static void SetupInitialScene(Game game)
        {
            var mainMenuScene = game.ContentManager.Load<Scene>("maps\\ShellMap1\\ShellMap1.map");
            game.Scene = mainMenuScene;

            mainMenuScene.Scene2D.WndWindowManager.PushWindow("Menus\\MainMenu.wnd");

            game.Scripting.Active = true;
        }
    }
}
