using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public void CanReadIniFiles()
        {
            // TODO: Finish INI parsing for BFME2 and subsequent games.
            var gameDefinitions = new[]
            {
                GameDefinition.FromGame(SageGame.CncGenerals),
                GameDefinition.FromGame(SageGame.CncGeneralsZeroHour),
                GameDefinition.FromGame(SageGame.Bfme),
                //GameDefinition.FromGame(SageGame.Bfme2),
            };

            foreach(var gameDefinition in gameDefinitions)
            {
                InstalledFilesTestData.ReadFiles(".ini", _output, new List<IGameDefinition> { gameDefinition }, entry =>
                {
                    switch (Path.GetFileName(entry.FilePath).ToLowerInvariant())
                    {
                        case "buttonsets.ini": // Doesn't seem to be used?
                        case "scripts.ini": // Only needed by World Builder?
                        case "commandmapdebug.ini": // Only applies to DEBUG and INTERNAL builds
                        case "fxparticlesystemcustom.ini": // Don't know if this is used, it uses Emitter property not used elsewhere
                        case "lightpoints.ini": // Don't know if this is used.

                        //BFME
                        case "optionregistry.ini": // Don't know if this is used
                        case "localization.ini": // Don't know if we need this
                            return;
                    }

                    var dataContext = new IniDataContext(entry.FileSystem, gameDefinition.Game);

                    // BFME I and II need to have GameData.ini loaded before any other INI files,
                    // because GameData.ini contains global macro definitions.
                    if (entry.FilePath.ToLowerInvariant() != @"data\ini\gamedata.ini")
                    {
                        dataContext.LoadIniFile(@"Data\INI\GameData.ini");
                    }

                    dataContext.LoadIniFile(entry);
                });
            }
        }
    }
}
