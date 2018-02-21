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
            var definition = GameDefinition.FromGame(SageGame.CncGenerals);

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
            });

            Platform.CurrentPlatform = new Sdl2Platform();
            Platform.CurrentPlatform.Start();

            // TODO: Support other locators.
            var locator = new RegistryInstallationLocator();

            var game = GameFactory.CreateGame(
                definition,
                locator,
                // TODO: Read game version from assembly metadata or .git folder
                // TODO: Set window icon.
                () => Platform.CurrentPlatform.CreateWindow("OpenSAGE (master)", 100, 100, 1024, 768));

            game.Configuration.LoadShellMap = !noShellMap;


            // The main menu also loads the shell map.
            definition.MainMenu?.AddToScene(game.ContentManager, game.Scene2D);

            while (game.IsRunning)
            {
                game.Tick();
            }

            Platform.CurrentPlatform.Stop();
        }
    }
}
