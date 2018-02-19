using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using System.CommandLine;
using System.Linq;

namespace OpenSage.Launcher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var noShellMap = false;
            var startupGame = SageGame.CncGenerals;

            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("noshellmap", ref noShellMap, false,
                    "Disables loading the shell map, speeding up startup time.");

                string gameName = null;
                var availableMods = GameDefinition.All.Select(def => def.Game.ToString());
                syntax.DefineOption("game", ref gameName, false,
                    $"Chooses which game to start. Valid options: {string.Join(", ", availableMods)}");

                if (!TryGetGameByName(gameName, out startupGame))
                {
                    syntax.ReportError($"Unknown game: {gameName}");
                }
            });

            Platform.CurrentPlatform = new Sdl2Platform();
            Platform.CurrentPlatform.Start();

            // TODO: Support other locators.
            var game = GameFactory.CreateGame(
                GameDefinition.FromGame(startupGame),
                new RegistryInstallationLocator(),
                // TODO: Read game version from assembly metadata or .git folder
                () => Platform.CurrentPlatform.CreateWindow("OpenSAGE (master)", 100, 100, 1024, 768));

            // TODO: Set window icon.

            if (!noShellMap)
            {
                var shellMapName = game.ContentManager.IniDataContext.GameData.ShellMapName;
                var mainMenuScene = game.ContentManager.Load<Scene3D>(shellMapName);
                game.Scene3D = mainMenuScene;
                game.Scripting.Active = true;
            }

            // TODO: Configure this per-game, and make it work with APT.
            game.Scene2D.WndWindowManager.PushWindow("Menus\\MainMenu.wnd");

            while (game.IsRunning)
            {
                game.Tick();
            }

            Platform.CurrentPlatform.Stop();
        }

        private static bool TryGetGameByName(string name, out SageGame game)
        {
            // TODO: Use a short identifier defined in IGameDefinition instead of stringified SageGame
            var gameOrNull = GameDefinition.All.SingleOrDefault(def => def.Game.ToString().ToLower() == name)?.Game;
            if (gameOrNull == null)
            {
                game = SageGame.CncGenerals;
                return false;
            }
            game = gameOrNull.Value;
            return true;
        }
    }
}
