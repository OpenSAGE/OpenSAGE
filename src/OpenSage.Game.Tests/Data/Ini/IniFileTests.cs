using System;
using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public void CanReadIniFiles()
        {
            // TODO: Finish INI parsing for BFME2 and subsequent games.
            var gameDefinitions = new[]
            {
                //GameDefinition.FromGame(SageGame.CncGenerals),
                //GameDefinition.FromGame(SageGame.CncGeneralsZeroHour),
                //GameDefinition.FromGame(SageGame.Bfme),
                GameDefinition.FromGame(SageGame.Bfme2),
            };

            foreach(var gameDefinition in gameDefinitions)
            {
                InstalledFilesTestData.ReadFiles(".ini", _output, new List<IGameDefinition> { gameDefinition }, entry =>
                {
                    var filename = Path.GetFileName(entry.FilePath).ToLowerInvariant();

                    //if (filename != "ingamenotificationbox.ini") return;

                    switch (filename)
                    {
                        case "buttonsets.ini": // Doesn't seem to be used?
                        case "scripts.ini": // Only needed by World Builder?
                        case "commandmapdebug.ini": // Only applies to DEBUG and INTERNAL builds
                        case "fxparticlesystemcustom.ini": // Don't know if this is used, it uses Emitter property not used elsewhere
                        case "lightpoints.ini": // Don't know if this is used.
                            return;
                    }

                    switch (gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                            switch (filename)
                            {
                                case "optionregistry.ini": // Don't know if this is used
                                case "localization.ini": // Don't know if we need this
                                    return;
                            }
                            break;
                        case SageGame.Bfme2:
                            switch (filename)
                            {
                                case "ingamenotificationbox.ini":
                                case "commandset.ini": //problem
                                case "awardsystem.ini":
                                case "riskcampaign.ini":
                                case "createaherosystem.ini":
                                case "skirmishaidata.ini":
                                case "firelogicsystem.ini":
                                case "formationassistant.ini":
                                case "linearcampaign.ini":
                                case "meshinstancingmanager.ini":
                                case "scoredkillevaannouncer.ini":
                                case "strategichud.ini":
                                case "map.ini":

                                case "livingworldaitemplate.ini":
                                case "livingworldautoresolvearmor.ini":
                                case "livingworldautoresolvebody.ini":
                                case "livingworldautoresolvecombatchain.ini":
                                case "livingworldautoresolvehandicaps.ini":
                                case "livingworldautoresolveleadership.ini":
                                case "livingworldautoresolvereinforcementschedule.ini":
                                case "livingworldautoresolveresourcebonus.ini":
                                case "livingworldautoresolvesciencepurchasepointbonus.ini":
                                case "livingworldautoresolveweapon.ini":
                                case "livingworldbuildingicons.ini":
                                case "livingworldbuildings.ini":
                                case "livingworldbuildploticons.ini":
                                case "livingworldplayers.ini":
                                case "livingworldregioneffects.ini":
                                    return;
                            }
                            break;
                    }
                    

                    var dataContext = new IniDataContext(entry.FileSystem, gameDefinition.Game);

                    // BFME I and II need to have GameData.ini loaded before any other INI files,
                    // because GameData.ini contains global macro definitions.
                    //TODO create only one context for each game definition and load that gamedata.ini on context creation
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
