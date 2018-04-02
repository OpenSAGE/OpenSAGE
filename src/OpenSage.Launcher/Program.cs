using System.CommandLine;
using System.Linq;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using OpenSage.Network;

namespace OpenSage.Launcher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var noShellMap = false;
            var definition = GameDefinition.FromGame(SageGame.CncGenerals);
            string mapName = null;

            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("noshellmap", ref noShellMap, false, "Disables loading the shell map, speeding up startup time.");

                string gameName = null;
                var availableGames = string.Join(", ", GameDefinition.All.Select(def => def.Game.ToString()));

                syntax.DefineOption("game", ref gameName, false, $"Chooses which game to start. Valid options: {availableGames}");

                // If a game has been specified, make sure it's valid.
                if (gameName != null && !GameDefinition.TryGetByName(gameName, out definition))
                {
                    syntax.ReportError($"Unknown game: {gameName}");
                }

                syntax.DefineOption("map", ref mapName, false,
                    "Immediately starts a new skirmish with default settings in the specified map. The map file must be specified with the full path.");
            });

            Platform.Start();

            // TODO: Support other locators.
            var locator = new RegistryInstallationLocator();

            var gameWindow = new GameWindow("OpenSAGE (master)", 100, 100, 1024, 768);

            var game = GameFactory.CreateGame(
                definition,
                locator,
                // TODO: Read game version from assembly metadata or .git folder
                // TODO: Set window icon.
                gameWindow);

            game.Configuration.LoadShellMap = !noShellMap;

            if (mapName == null)
            {
                game.ShowMainMenu();
            }
            else
            {
                game.StartGame(mapName, new EchoConnection(), new[] {"America", "GLA"}, 0);
            }

            while (game.IsRunning)
            {
                game.Tick();
            }

            gameWindow.Dispose();

            Platform.Stop();
        }
    }
}
