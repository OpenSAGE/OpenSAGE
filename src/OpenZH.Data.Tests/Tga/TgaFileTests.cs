using System.IO;
using System.Linq;
using OpenZH.Data.Big;
using OpenZH.Data.Tga;
using Xunit;

namespace OpenZH.Data.Tests.Tga
{
    public class TgaFileTests
    {
        private const string BigFilePath = @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour\TexturesZH.big";

        [Fact]
        public void CanReadTgaTextures()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                foreach (var entry in bigArchive.Entries.Where(x => Path.GetExtension(x.FullName).ToLowerInvariant() == ".tga"))
                {
                    using (var entryStream = entry.Open())
                    using (var binaryReader = new BinaryReader(entryStream))
                    {
                        var tgaFile = TgaFile.Parse(binaryReader);

                        Assert.True(tgaFile.Header.ImagePixelSize == 24 || tgaFile.Header.ImagePixelSize == 32);
                    }
                }
            }
        }
    }
}
