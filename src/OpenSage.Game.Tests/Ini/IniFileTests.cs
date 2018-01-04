using System.IO;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Ini
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
            InstalledFilesTestData.ReadFiles(".ini", _output, entry =>
            {
                switch (Path.GetFileName(entry.FilePath).ToLowerInvariant())
                {
                    case "buttonsets.ini": // Doesn't seem to be used?
                    case "scripts.ini": // Only needed by World Builder?
                    case "commandmapdebug.ini": // Only applies to DEBUG and INTERNAL builds
                    case "fxparticlesystemcustom.ini": // Don't know if this is used, it uses Emitter property not used elsewhere
                        return;
                }

                var dataContext = new IniDataContext(entry.FileSystem);

                // BFME I and II need to have GameData.ini loaded before any other INI files,
                // because GameData.ini contains global macro definitions.
                if (entry.FilePath.ToLowerInvariant() != @"data\ini\gamedata.ini")
                {
                    dataContext.LoadIniFile(@"Data\INI\GameData.ini");
                }

                dataContext.LoadIniFile(entry);

                Assert.NotNull(dataContext.CommandMaps);

                foreach (var objectDefinition in dataContext.Objects)
                {
                    foreach (var draw in objectDefinition.Draws)
                    {
                        switch (draw)
                        {
                            case W3dModelDrawModuleData md:
                                //if (md.DefaultConditionState != null)
                                //{
                                //    Assert.True(md.DefaultConditionState.Animations.Count <= 1);
                                //}
                                //foreach (var conditionState in md.ConditionStates)
                                //{
                                //    Assert.True(conditionState.Animations.Count <= 1);
                                //}
                                break;
                        }
                    }
                }
            });
        }
    }
}
