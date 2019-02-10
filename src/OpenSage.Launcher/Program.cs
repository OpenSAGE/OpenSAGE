using System;
using System.Linq;
using CommandLine;
using OpenSage.Data;
using OpenSage.Diagnostics;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Mods.BuiltIn;
using OpenSage.Network;
using Veldrid;
using System.IO;
using System.Collections.Generic;

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

            [Option('m', "map", Required = false, HelpText = "Immediately starts a new skirmish with default settings in the specified map. The map file must be specified with the full pathf.")]
            public string Map { get; set; }

            [Option("novsync", Default = false, Required = false, HelpText = "Disable vsync.")]
            public bool DisableVsync { get; set; }

            [Option('f', "fullscreen", Default = false, Required = false, HelpText = "Enable fullscreen mode.")]
            public bool Fullscreen { get; set; }

            //not implemented yet
            [Option("win", Default = true, Required = false, HelpText = "Force windowed game.")]
            public bool Windowed { get; set; }

            [Option("developermode", Default = false, Required = false, HelpText = "Enable developer mode.")]
            public bool DeveloperMode { get; set; }

            [Option("scriptDebugLite", Default = false, Required = false, HelpText = "Enable developer mode.")]
            public bool ScriptDebugLite { get; set; }

            [Option("scriptDebug2", Default = false, Required = false, HelpText = "Enable developer mode.")]
            public bool ScriptDebug2 { get; set; }

            [Option("tracefile", Default = null, Required = false, HelpText = "Generate trace output to the specified path, for example `--tracefile trace.json`. Trace files can be loaded into Chrome's tracing GUI at chrome://tracing")]
            public string TraceFile { get; set; }

            [Option('p', "gamepath", Default = null, Required = false, HelpText = "Force game to use this gamepath")]
            public string GamePath { get; set; }

            [Option('x', "xres", Default = 1024, Required = false, HelpText = "Diplayresolution width")]
            public int Xresolution { get; set; }

            [Option('y', "yres", Default = 768, Required = false, HelpText = "Diplayresolution height")]
            public int Yresolution { get; set; }

            //not implemented yet
            [Option("fps", Default = 60, Required = false, HelpText = "Force FPS")]
            public int FPS { get; set; }

            //not implemented yet
            [Option("noFPSLimit", Default = false, Required = false, HelpText = "Force windowed game.")]
            public bool NoFPSLimit { get; set; }

            //not implemented yet
            [Option("LogicTickRate", Default = null, Required = false, HelpText = "Force LogicTickRate.")]
            public int LogicTickRate { get; set; }

            //not implemented yet
            [Option("mod", Default = null, Required = false, HelpText = "Load mod file(s) and/or folder(s)")]
            public IEnumerable<string> ModPaths { get; set; }

            //not implemented yet
            [Option("lang", Default = null, Required = false, HelpText = "Try to use a certain language if possible.")]
            public string Language { get; set; }

            //not implemented yet
            [Option("preferLocalFiles", Default = false, Required = false, HelpText = "Local INI/XML files have priority compared to files in archives.")]
            public bool PreferLocalFiles { get; set; }

            [Option("decompress", Default = null, Required = false, HelpText = "Decompress a RefPack compression file (e.g. map file)")]
            public string Decompress { get; set; }

            //not implemented yet
            [Option("compress", Default = null, Required = false, HelpText = "Use RefPack compression for a file (e.g. map file)")]
            public string Compress { get; set; }

            //not implemented yet
            [Option("extractbig", Default = null, Required = false, HelpText = "Extract BIG files")]
            public IEnumerable<string> ExtractBigFileList { get; set; }

            //not implemented yet
            [Option("compressbig", Default = null, Required = false, HelpText = "Compress BIG files")]
            public IEnumerable<string> CompressBigFileList { get; set; }

            //not implemented yet
            [Option("viewasset", Default = null, Required = false, HelpText = "View asset (e.g. w3d file)")]
            public string AssetFile { get; set; }

            //not implemented yet
            [Option('u', "userdata", Default = null, Required = false, HelpText = "Use custom userdata folder")]
            public string UserDataFolder { get; set; }

        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(opts => Run(opts));
        }


        public static void Run(Options opts)
        {
            var ForceUseEnvironmentGamePath = true;

            if (opts.GamePath == null)
            {
                opts.GamePath = Environment.CurrentDirectory;
            }
            if (File.Exists(Path.Combine(opts.GamePath, "generals.exe")) && File.Exists(Path.Combine(opts.GamePath, "INI.big")))
            {
                opts.Game = SageGame.CncGenerals;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "generals.exe")) && File.Exists(Path.Combine(opts.GamePath, "INIZH.big")))
            {
                opts.Game = SageGame.CncGeneralsZeroHour;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "lotrbfme.exe")))
            {
                opts.Game = SageGame.Bfme;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "lotrbfme2.exe")))
            {
                opts.Game = SageGame.Bfme2;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "lotrbfme2ep1.exe")))
            {
                opts.Game = SageGame.Bfme2Rotwk;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "CNC3.exe")))
            {
                opts.Game = SageGame.Cnc3;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "CNC3EP1.exe")))
            {
                opts.Game = SageGame.Cnc3KanesWrath;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "RA3.exe")))
            {
                opts.Game = SageGame.Ra3;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "RA3EP1.exe")))
            {
                opts.Game = SageGame.Ra3Uprising;
            }
            else if (File.Exists(Path.Combine(opts.GamePath, "CNC4.exe")))
            {
                opts.Game = SageGame.Cnc4;
            }
            else
            {
                //default game
                opts.Game = SageGame.CncGenerals;
                ForceUseEnvironmentGamePath = false;
            }

            var definition = GameDefinition.FromGame(opts.Game);

            if (ForceUseEnvironmentGamePath)
            {
                System.Environment.SetEnvironmentVariable(definition.Identifier.ToUpperInvariant() + "_PATH", opts.GamePath);
            }

            var installation = GameInstallation
                .FindAll(new[] { definition }, ForceUseEnvironmentGamePath)
                .FirstOrDefault();

            if (opts.Decompress != null)
            {
                var FileStream = OpenSage.Data.Map.MapFile.Decompress(File.OpenRead(opts.Decompress));
                var UncompressedFilePath = String.Concat(Path.GetFileNameWithoutExtension(opts.Decompress), "_uncompressed", Path.GetExtension(opts.Decompress));
                var NewFileStream = File.Create(UncompressedFilePath);
                byte[] buffer = new byte[8 * 1024];
                int len;
                while ((len = FileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    NewFileStream.Write(buffer, 0, len);
                }
                Console.WriteLine($"Uncompressed file saved to {UncompressedFilePath}");
                Environment.Exit(1);
            }

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

            var traceEnabled = !string.IsNullOrEmpty(opts.TraceFile);
            if (traceEnabled)
            {
                GameTrace.Start(opts.TraceFile);
            }

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            using (var game = new Game(installation, opts.Renderer, opts.Fullscreen, opts.Xresolution, opts.Yresolution))
            {
                game.GraphicsDevice.SyncToVerticalBlank = !opts.DisableVsync;

                game.Configuration.LoadShellMap = !opts.NoShellmap;

                if (opts.DeveloperMode || opts.ScriptDebugLite || opts.ScriptDebug2)
                {
                    game.DeveloperModeEnabled = true;
                }
                
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

                    game.StartMultiPlayerGame(opts.Map,
                         new EchoConnection(),
                         pSettings,
                         0);
                }

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
