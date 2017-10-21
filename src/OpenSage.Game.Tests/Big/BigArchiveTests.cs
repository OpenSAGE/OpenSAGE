using System.IO;
using OpenSage.Data.Big;
using Xunit;

namespace OpenSage.Data.Tests.Big
{
    public class BigArchiveTests
    {
        private string BigFilePath;

        public BigArchiveTests()
        {
            BigFilePath = Path.Combine(InstalledFilesTestData.GetInstallationDirectory(GameId.ZeroHour), "W3DZH.big");
        }

        [Fact]
        public void OpenBigArchive()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                Assert.Equal(4432, bigArchive.Entries.Count);
            }
        }

        [Fact]
        public void ReadFirstEntry()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var firstEntry = bigArchive.Entries[0];

                Assert.Equal(@"Art\W3D\ABBarracks_AC.W3D", firstEntry.FullName);
                Assert.Equal(9334u, firstEntry.Length);
            }
        }

        [Fact]
        public void ReadFirstEntryStream()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
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

        [Fact]
        public void GetEntryByName()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"Art\W3D\ABBarracks_AC.W3D");

                Assert.Equal(@"Art\W3D\ABBarracks_AC.W3D", entry.FullName);
                Assert.Equal(9334u, entry.Length);
            }
        }
    }
}
