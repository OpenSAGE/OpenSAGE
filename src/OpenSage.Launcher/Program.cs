using System;
using System.Linq;
using CommandLine;
using OpenSage.Data;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Mods.BuiltIn;
using OpenSage.Network;
using Veldrid;

namespace OpenSage.Launcher
{
    public static class Program
    {
        public sealed class Options
        {
            //use a string since nullable enums aren't working yet
            [Option('r', "renderer", Default = null, Required = false, HelpText = "Set the renderer backend.")]
            public GraphicsBackend? Renderer { get; set; }

            [Option("noshellmap", Default = false, Required = false, HelpText = "Disables loading the shell map, speeding up startup time.")]
            public bool NoShellmap { get; set; }

            [Option('g', "game", Default = SageGame.CncGenerals, Required = false, HelpText = "Chooses which game to start.")]
            public SageGame Game { get; set; }

            [Option('m', "map", Required = false, HelpText = "Immediately starts a new skirmish with default settings in the specified map. The map file must be specified with the full pathf.")]
            public string Map { get; set; }

            [Option("novsync", Default = false, Required = false, HelpText = "Disable vsync.")]
            public bool DisableVsync { get; set; }

            [Option("developermode", Default = false, Required = false, HelpText = "Enable developer mode.")]
            public bool DeveloperMode { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(opts => Run(opts));
        }

        public static void Run(Options opts)
        {
            var definition = GameDefinition.FromGame(opts.Game);

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

                Console.WriteLine("\n\n Press any key to exit.");

                Console.ReadLine();

                Environment.Exit(1);
            }

            Platform.Start();

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            using (var game = new Game(installation, opts.Renderer))
            {
                game.GraphicsDevice.SyncToVerticalBlank = !opts.DisableVsync;

                game.Configuration.LoadShellMap = !opts.NoShellmap;

                game.DeveloperModeEnabled = opts.DeveloperMode;

                if (opts.Map == null)
                {
                    game.ShowMainMenu();
                }
                else
                {
                    var pSettings = new[]
                    {
                        new PlayerSetting("America", new ColorRgb(255, 0, 0)),
                        new PlayerSetting("GLA", new ColorRgb(255, 255, 255)),
                    };

                    game.StartGame(opts.Map,
                         new EchoConnection(),
                         pSettings,
                         0);
                }

                game.Run();
            }

            Platform.Stop();
        }
    }
}
