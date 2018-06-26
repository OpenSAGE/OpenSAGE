using System;
using System.CommandLine;
using System.Linq;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using OpenSage.Network;
using Veldrid;

namespace OpenSage.Launcher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var noShellMap = false;
            var definition = GameDefinition.FromGame(SageGame.CncGenerals);
            string mapName = null;
            GraphicsBackend? preferredBackend = null;

            ArgumentSyntax.Parse(args, syntax =>
            {
                string preferredBackendString = null;
                syntax.DefineOption("renderer", ref preferredBackendString, false, $"Choose which renderer backend should be used. Valid options: {string.Join(",", Enum.GetNames(typeof(GraphicsBackend)))}");
                if (preferredBackendString != null)
                {
                    if (Enum.TryParse<GraphicsBackend>(preferredBackendString, out var preferredBackendTemp))
                    {
                        preferredBackend = preferredBackendTemp;
                    }
                    else
                    {
                        syntax.ReportError($"Unknown renderer backend: {preferredBackendString}");
                    }
                }

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

            var installation = GameInstallation
                .FindAll(new[] { definition })
                .FirstOrDefault();

            if (installation == null)
            {
                Console.WriteLine($"OpenSAGE was unable to find any installations of {definition.DisplayName}.\n");

                Console.WriteLine("You can manually specify the installation path by setting the following environment variable:");
                Console.WriteLine($"\t{definition.Identifier.ToUpper()}_PATH=<installation path>\n");

                Console.WriteLine("OpenSAGE doesn't yet detect every released version of every game. Please report undetected versions to our GitHub page:");
                Console.WriteLine("\thttps://github.com/OpenSAGE/OpenSAGE/issues");

                Environment.Exit(1);
            }

            Platform.Start();
            LanguageSetting.ReadFromRegistry(definition);

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            using (var window = new GameWindow("OpenSAGE (master)", 100, 100, 1024, 768, preferredBackend))
            using (var gamePanel = GamePanel.FromGameWindow(window))
            using (var game = GameFactory.CreateGame(installation, installation.CreateFileSystem(), gamePanel))
            {
                game.Configuration.LoadShellMap = !noShellMap;

                if (mapName == null)
                {
                    game.ShowMainMenu();
                }
                else
                {
                    game.StartGame(mapName, new EchoConnection(), new[] { "America", "GLA" }, 0);
                }

                while (game.IsRunning)
                {
                    if (!window.PumpEvents())
                    {
                        break;
                    }

                    game.Tick();
                }
            }

            Platform.Stop();
        }
    }
}
