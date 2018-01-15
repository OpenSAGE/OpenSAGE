using System.IO;
using OpenSage.Data.Big;
using OpenSage.Data.Apt;
using Xunit;
using Xunit.Abstractions;
using OpenSage.Data.Tests;
using OpenSage.Data;

namespace OpenSage.Game.Tests.Apt
{
    public class ConstFileTests
    {
        private readonly ITestOutputHelper _output;

        public ConstFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [GameFact(SageGame.BattleForMiddleEarth, SageGame.BattleForMiddleEarthII)]
        public void CanReadConstFiles()
        {
            InstalledFilesTestData.ReadFiles(".const", _output, entry =>
            {
                var constFile = ConstantData.FromFileSystemEntry(entry);

                Assert.NotNull(constFile);
            });
        }

        [GameFact(SageGame.BattleForMiddleEarthII)]
        public void CheckEntryCount()
        {
            var bigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(SageGame.BattleForMiddleEarthII), "apt/MainMenu.big");

            using (var bigStream = File.OpenRead(bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"MainMenu.const");

                var data = ConstantData.FromFileSystemEntry(new FileSystemEntry(null, entry.FullName, entry.Length, entry.Open));
                Assert.NotNull(data);

                //requires unmodified main menu
                Assert.Equal(412, data.Entries.Count);
            }
        }
    }
}
