﻿using System.IO;
using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Mods.BuiltIn;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Ini
{
    public class IniFileTests
    {
        private readonly ITestOutputHelper _output;

        public IniFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // TODO: Need to rewrite this test to use new ini loading mechanism.

        //[Fact]
        //public void CanReadIniFiles()
        //{
        //    var gameDefinitions = new[]
        //    {
        //        GameDefinition.FromGame(SageGame.CncGenerals),
        //        GameDefinition.FromGame(SageGame.CncGeneralsZeroHour),
        //        GameDefinition.FromGame(SageGame.Bfme),
        //        GameDefinition.FromGame(SageGame.Bfme2),
        //        GameDefinition.FromGame(SageGame.Bfme2Rotwk),
        //    };

        //    foreach (var gameDefinition in gameDefinitions)
        //    {
        //        var rootDirectories = InstallationLocators.FindAllInstallations(gameDefinition).Select(i => i.Path).ToList();

        //        foreach (var rootDirectory in rootDirectories)
        //        {
        //            using (var fileSystem = new FileSystem(rootDirectory))
        //            {
        //                var dataContext = new IniDataContext(fileSystem, gameDefinition.Game, null);

        //                switch (gameDefinition.Game)
        //                {
        //                    case SageGame.Bfme:
        //                    case SageGame.Bfme2:
        //                    case SageGame.Bfme2Rotwk:
        //                        dataContext.LoadIniFile(@"Data\INI\GameData.ini");
        //                        break;
        //                }

        //                foreach (var file in fileSystem.Files)
        //                {
        //                    if (Path.GetExtension(file.FilePath).ToLowerInvariant() != ".ini")
        //                    {
        //                        continue;
        //                    }

        //                    var filename = Path.GetFileName(file.FilePath).ToLowerInvariant();

        //                    switch (filename)
        //                    {
        //                        case "buttonsets.ini": // Doesn't seem to be used?
        //                        case "scripts.ini": // Only needed by World Builder?
        //                        case "commandmapdebug.ini": // Only applies to DEBUG and INTERNAL builds
        //                        case "fxparticlesystemcustom.ini": // Don't know if this is used, it uses Emitter property not used elsewhere
        //                        case "lightpoints.ini": // Don't know if this is used.

        //                        //added in BFME and subsequent games
        //                        case "optionregistry.ini": // Don't know if this is used
        //                        case "localization.ini": // Don't know if we need this
        //                            continue;

        //                        case "credits.ini":
        //                            if(gameDefinition.Game == SageGame.Bfme2Rotwk) //corrupted in rotwk (start of the block is commented out)
        //                            {
        //                                continue;
        //                            }
        //                            break;

        //                        //mods specific

        //                        //edain mod
        //                        case "einstellungen.ini":
        //                        case "einstellungendeakt.ini":
        //                        case "einstellungenedain.ini":
        //                        case "news.ini":
        //                            continue;

        //                        //unofficial patch 2.02
        //                        case "desktop.ini": //got into a big file somehow
        //                        case "2.01.ini":
        //                        case "disable timer.ini":
        //                        case "enable timer.ini":
        //                        case "old music.ini":
        //                            continue;
        //                        default:
        //                            if (filename.StartsWith("2.02")) { continue; }
        //                            break;
        //                    }

        //                    _output.WriteLine($"Reading file {file.FilePath}.");

        //                    dataContext.LoadIniFile(file);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
