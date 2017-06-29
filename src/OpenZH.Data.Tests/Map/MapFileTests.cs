using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenZH.Data.Big;
using OpenZH.Data.Map;
using Xunit;

namespace OpenZH.Data.Tests.Map
{
    public class MapFileTests
    {
        private const string BigFilePath = @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour\MapsZH.big";

        [Fact]
        public void CanReadMaps()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                foreach (var entry in bigArchive.Entries.Where(x => Path.GetExtension(x.FullName).ToLowerInvariant() == ".map"))
                {
                    if (!entry.FullName.Contains("Alpine"))
                        continue; // TODO: Remove this.

                    using (var entryStream = entry.Open())
                    using (var binaryReader = new BinaryReader(entryStream))
                    {
                        var mapFile = MapFile.Parse(binaryReader);

                        //Assert.True(tgaFile.Header.ImagePixelSize == 24 || tgaFile.Header.ImagePixelSize == 32);
                    }
                }
            }
        }

        [Fact]
        public void MapTestSuite()
        {
            foreach (var entry in Directory.GetFiles(@"C:\Users\Tim Jones\Desktop\ZH", "*.map", SearchOption.AllDirectories))
            {
                Debug.WriteLine(entry);
                using (var entryStream = File.OpenRead(entry))
                using (var binaryReader = new BinaryReader(entryStream))
                {
                    var mapFile = MapFile.Parse(binaryReader);

                    //Assert.True(tgaFile.Header.ImagePixelSize == 24 || tgaFile.Header.ImagePixelSize == 32);
                }
            }
        }
    }
}
