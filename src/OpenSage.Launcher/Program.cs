using System;
using System.Linq;
using CommandLine;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using OpenSage.Network;
using Veldrid;

namespace OpenSage.Launcher
{
    public static class Program
    {
        //must match Veldrid.GraphicsBackends. Can be removed once nullable enums work
        public enum Renderer : byte
        {
            Direct3D11 = 0,
            Vulkan = 1,
            OpenGL = 2,
            Metal = 3,
            OpenGLES = 4,
            Default
        }

        public class Options
        {
            //use a string since nullable enums aren't working yet
            [Option('r', "renderer", Default = Renderer.Default, Required = false, HelpText = "Set the renderer backend.")]
            public Renderer Renderer { get; set; }

            [Option("noshellmap", Default = false, Required = false, HelpText = "Disables loading the shell map, speeding up startup time.")]
            public bool NoShellmap { get; set; }

            [Option('g', "game", Default = SageGame.CncGenerals, Required = false, HelpText = "Chooses which game to start.")]
            public SageGame Game { get; set; }

            [Option('m', "map", Required = false, HelpText = "Immediately starts a new skirmish with default settings in the specified map. The map file must be specified with the full pathf.")]
            public string Map { get; set; }
        }

        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts => Run(opts));
        }

        public static void Run(Options opts)
        {
            var definition = GameDefinition.FromGame(opts.Game);
            GraphicsBackend? preferredBackend = null;

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

            if (opts.Renderer != Renderer.Default)
            {
                preferredBackend = (GraphicsBackend) opts.Renderer;
            }

            Platform.Start();

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            using (var window = new GameWindow("OpenSAGE (master)", 100, 100, 1024, 768, preferredBackend))
            using (var gamePanel = GamePanel.FromGameWindow(window))
            using (var game = GameFactory.CreateGame(installation, installation.CreateFileSystem(), gamePanel))
            {
                game.Configuration.LoadShellMap = !opts.NoShellmap;

                if (opts.Map == null)
                {
                    game.ShowMainMenu();
                }
                else
                {
                    game.StartGame(opts.Map, new EchoConnection(), new[] { "America", "GLA" }, 0);
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
