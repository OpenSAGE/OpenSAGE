using System;
using System.IO;
using System.Linq;
using CommandLine;
using NLog.Targets;
using OpenSage.Data;
using OpenSage.Diagnostics;
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
            [Option('r', "renderer", Default = null, Required = false, HelpText = "Set the renderer backend.")]
            public GraphicsBackend? Renderer { get; set; }

            [Option("noshellmap", Default = false, Required = false, HelpText = "Disables loading the shell map, speeding up startup time.")]
            public bool NoShellmap { get; set; }

            [Option('g', "game", Default = SageGame.CncGenerals, Required = false, HelpText = "Chooses which game to start.")]
            public SageGame Game { get; set; }

            [Option('m', "map", Required = false, HelpText = "Immediately starts a new skirmish with default settings in the specified map. The map file must be specified with the full path.")]
            public string Map { get; set; }

            [Option("novsync", Default = false, Required = false, HelpText = "Disable vsync.")]
            public bool DisableVsync { get; set; }

            [Option('f', "fullscreen", Default = false, Required = false, HelpText = "Enable fullscreen mode.")]
            public bool Fullscreen { get; set; }

            [Option('d', "renderdoc", Default = false, Required = false, HelpText = "Enable renderdoc debugging.")]
            public bool RenderDoc { get; set; }

            [Option("developermode", Default = false, Required = false, HelpText = "Enable developer mode.")]
            public bool DeveloperMode { get; set; }

            [Option("tracefile", Default = null, Required = false, HelpText = "Generate trace output to the specified path, for example `--tracefile trace.json`. Trace files can be loaded into Chrome's tracing GUI at chrome://tracing")]
            public string TraceFile { get; set; }

            [Option("replay", Default = null, Required = false, HelpText = "Specify a replay file to immediately start replaying")]
            public string ReplayFile { get; set; }
        }

        public static void Main(string[] args)
        {
            Target.Register<Core.InternalLogger>("OpenSage");

            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(opts => Run(opts));
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Run(Options opts)
        {
            logger.Info("Starting...");

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

            logger.Debug($"Have installation of {definition.DisplayName}");

            Platform.Start();

            var traceEnabled = !string.IsNullOrEmpty(opts.TraceFile);
            if (traceEnabled)
            {
                GameTrace.Start(opts.TraceFile);
            }

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            var config = new Configuration()
            {
                UseFullscreen = opts.Fullscreen,
                UseRenderDoc = opts.RenderDoc,
                LoadShellMap = !opts.NoShellmap,
            };

            logger.Debug($"Have configuration");

            using (var game = new Game(installation, opts.Renderer, config))
            {
                game.GraphicsDevice.SyncToVerticalBlank = !opts.DisableVsync;

                game.DeveloperModeEnabled = opts.DeveloperMode;

                if (opts.ReplayFile != null)
                {
                    using (var fileSystem = new FileSystem(Path.Combine(game.UserDataFolder, "Replays")))
                    {
                        game.LoadReplayFile(fileSystem.GetFile(opts.ReplayFile));
                    }
                }
                else if (opts.Map != null)
                {
                    var mapCache = game.AssetStore.MapCaches.GetByName(opts.Map);
                    if (mapCache == null)
                    {
                        logger.Debug("Could not find MapCache entry for map " + opts.Map);
                        game.ShowMainMenu();
                    }
                    else if (mapCache.IsMultiplayer)
                    {
                        var pSettings = new PlayerSetting?[]
                        {
                            new PlayerSetting(null, game.AssetStore.PlayerTemplates.GetByName("FactionAmerica"), new ColorRgb(255, 0, 0)),
                            new PlayerSetting(null, game.AssetStore.PlayerTemplates.GetByName("FactionGLA"), new ColorRgb(255, 255, 255)),
                        };

                        logger.Debug("Starting multiplayer game");

                        game.StartMultiPlayerGame(opts.Map,
                            new EchoConnection(),
                            pSettings,
                            0);
                    }
                    else
                    {
                        logger.Debug("Starting singleplayer game");

                        game.StartSinglePlayerGame(opts.Map);
                    }
                }
                else
                {
                    logger.Debug("Showing main menu");
                    game.ShowMainMenu();
                }

                logger.Debug("Starting game");

                game.Run();
            }

            if (traceEnabled)
            {
                GameTrace.Stop();
            }

            Platform.Stop();
        }
    }
}
