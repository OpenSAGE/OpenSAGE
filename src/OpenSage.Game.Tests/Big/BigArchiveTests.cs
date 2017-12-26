using System.IO;
using OpenSage.Data.Big;
using OpenSage.Game.Tests;
using Xunit;

namespace OpenSage.Data.Tests.Big
{
    public class BigArchiveTests
    {
        private readonly string _bigFilePath;

        public BigArchiveTests()
        {
            _bigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGeneralsZeroHour), "W3DZH.big");
        }

        [GameFact(SageGame.CncGeneralsZeroHour)]
        public void OpenBigArchive()
        {
            using (var bigStream = File.OpenRead(_bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                Assert.Equal(4432, bigArchive.Entries.Count);
            }
        }

        [GameFact(SageGame.CncGeneralsZeroHour)]
        public void ReadFirstEntry()
        {
            using (var bigStream = File.OpenRead(_bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var firstEntry = bigArchive.Entries[0];

                Assert.Equal(@"Art\W3D\ABBarracks_AC.W3D", firstEntry.FullName);
                Assert.Equal(9334u, firstEntry.Length);
            }
        }

        [GameFact(SageGame.CncGeneralsZeroHour)]
        public void ReadFirstEntryStream()
        {
            using (var bigStream = File.OpenRead(_bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var firstEntry = bigArchive.Entries[0];

                using (var firstEntryStream = firstEntry.Open())
                {
                    Assert.Equal(9334, firstEntryStream.Length);

                    var buffer = new byte[firstEntryStream.Length];
                    var readBytes = firstEntryStream.Read(buffer, 0, 10000);
                    Assert.Equal(9334, readBytes);
                }
            }
        }

        [GameFact(SageGame.CncGeneralsZeroHour)]
        public void GetEntryByName()
        {
            using (var bigStream = File.OpenRead(_bigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"Art\W3D\ABBarracks_AC.W3D");

                Assert.Equal(@"Art\W3D\ABBarracks_AC.W3D", entry.FullName);
                Assert.Equal(9334u, entry.Length);
            }
        }
    }
}
