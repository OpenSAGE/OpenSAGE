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

   
        [Fact]
        public void CanReadConstFiles()
        {
            var bigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(SageGame.BattleForMiddleEarthII), "apt/MainMenu.big");

            using (var bigStream = File.OpenRead(bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"MainMenu.const");

                var data = ConstantData.FromFileSystemEntry(new FileSystemEntry(null, entry.FullName, entry.Length, entry.Open));
                Assert.NotNull(data);
            }
        }
    }
}
