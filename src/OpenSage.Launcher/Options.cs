using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Veldrid;

namespace OpenSage.Launcher
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

        [Option('p', "gamepath", Default = null, Required = false, HelpText = "Force game to use this gamepath")]
        public string GamePath { get; set; }

        [Option('u', "uniqueports", Default = false, Required = false, HelpText = "Use a unique port for each client in a multiplayer game. Normally, port 8088 is used, but when we want to run multiple game instances on the same machine (for debugging purposes), each client needs a different port.")]
        public bool UseUniquePorts { get; set; }
    }
}
