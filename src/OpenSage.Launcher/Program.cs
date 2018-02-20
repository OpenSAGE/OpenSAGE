using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using System.CommandLine;
using System.Linq;
using OpenSage.Gui.Wnd;

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

            var definition = GameDefinition.FromGame(startupGame);
            var locator = new RegistryInstallationLocator();

            // TODO: Support other locators.
            var game = GameFactory.CreateGame(
                definition,
                locator,
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

            definition.MainMenu?.AddToScene(game.ContentManager, game.Scene2D);

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
