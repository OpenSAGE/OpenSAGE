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
                //GameDefinition.FromGame(SageGame.CncGenerals),
                //GameDefinition.FromGame(SageGame.CncGeneralsZeroHour),
                //GameDefinition.FromGame(SageGame.Bfme),
                GameDefinition.FromGame(SageGame.Bfme2),
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

                        //BFME2
                        case "armysummarydescription.ini":
                        case "attributemodifier.ini":
                        case "awardsystem.ini":
                        case "bannerui.ini":
                        case "riskcampaign.ini":
                        case "commandbutton.ini":
                        case "commandset.ini":
                        case "crate.ini":
                        case "createaherospecialpowers.ini":
                        case "createaherosystem.ini":
                        case "crowdresponse.ini":
                        case "damagefx.ini":
                        case "aidata.ini":
                        case "eva.ini":
                        case "skirmishaidata.ini":
                        case "soundeffects.ini":
                        case "emotions.ini":
                        case "environment.ini":
                        case "experiencelevels.ini":
                        case "fire.ini":
                        case "firelogicsystem.ini":
                        case "formationassistant.ini":
                        case "fxlist.ini":
                        case "fxparticlesystem.ini":
                        case "gamelod.ini":
                        case "gamelodpresets.ini":
                        case "ingamenotificationbox.ini":
                        case "ingameui.ini":
                        case "largegroupaudio.ini":
                        case "linearcampaign.ini":
                        case "livingworld.ini":
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
                        case "dwarficons.ini":
                        case "elficons.ini":
                        case "isengardicons.ini":
                        case "mordoricons.ini":
                        case "mowicons.ini":
                        case "wildicons.ini":
                        case "livingworldplayers.ini":
                        case "livingworldregioneffects.ini":
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
