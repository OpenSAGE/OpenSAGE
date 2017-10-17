using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            var tasks = new List<Task>();

            InstalledFilesTestData.ReadFiles(".ini", _output, entry =>
            {
                if (Path.GetFileName(entry.FilePath) == "ButtonSets.ini")
                    return; // This file doesn't seem to be used?

                if (Path.GetFileName(entry.FilePath).ToLowerInvariant() == "scripts.ini")
                    return; // Only needed by World Builder?

                if (Path.GetFileName(entry.FilePath).ToLowerInvariant() != "particlesystem.ini")
                    return;

                tasks.Add(Task.Run(() =>
                {
                    var dataContext = new IniDataContext(entry.FileSystem);
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
                }));
            });

            Task.WaitAll(tasks.ToArray());
        }
    }
}
