using System.IO;
using OpenSage.Data.Big;
using OpenSage.Data.Apt;
using Xunit;
using Xunit.Abstractions;
using OpenSage.Data.Tests;
using OpenSage.Data;

namespace OpenSage.Game.Tests.Apt
{
    public class AptFileTests
    {
        private readonly ITestOutputHelper _output;

        public AptFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [GameFact(SageGame.BattleForMiddleEarthII)]
        public void CanReadAptFiles()
        {
            InstalledFilesTestData.ReadFiles(".apt", _output, entry =>
            {
                var aptFile = AptFile.FromFileSystemEntry(entry);

                Assert.NotNull(aptFile);
            });
        }

        [GameFact(SageGame.BattleForMiddleEarthII)]
        public void CheckEntryCount()
        {
            var fileSystem = new FileSystem(InstalledFilesTestData.GetInstallationDirectory(SageGame.BattleForMiddleEarthII));
            var entry = fileSystem.GetFile(@"MainMenu.apt");

            var data = AptFile.FromFileSystemEntry(entry);
            Assert.NotNull(data);
        }
    }
}
