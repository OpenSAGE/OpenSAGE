using System.IO;
using System.Linq;
using OpenZH.Data.Big;
using OpenZH.Data.Dds;
using Xunit;

namespace OpenZH.Data.Tests.Dds
{
    public class DdsFileTests
    {
        private const string BigFilePath = @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour\TexturesZH.big";

        [Fact]
        public void CanReadDdsTextures()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                foreach (var entry in bigArchive.Entries.Where(x => Path.GetExtension(x.FullName).ToLowerInvariant() == ".dds"))
                {
                    using (var entryStream = entry.Open())
                    using (var binaryReader = new BinaryReader(entryStream))
                    {
                        var ddsFile = DdsFile.Parse(binaryReader);

                        Assert.True(ddsFile.MipMaps.Length > 1);
                    }
                }
            }
        }
    }
}
