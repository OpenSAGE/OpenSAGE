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

        [Fact]
        public void CanReadAptFiles()
        {
            InstalledFilesTestData.ReadFiles(".apt", _output, entry =>
            {
                var aptFile = AptFile.FromFileSystemEntry(entry);

                Assert.NotNull(aptFile);
            });
        }

        [Fact]
        public void CheckEntryCount()
        {
            var bigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(SageGame.BattleForMiddleEarthII), "apt/MainMenu.big");

            using (var bigStream = File.OpenRead(bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"MainMenu.apt");

                var data = AptFile.FromFileSystemEntry(new FileSystemEntry(null, entry.FullName, entry.Length, entry.Open));
                Assert.NotNull(data);

            }
        }
    }
}
