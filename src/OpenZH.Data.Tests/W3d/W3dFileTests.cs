using System.IO;
using OpenZH.Data.Big;
using OpenZH.Data.W3d;
using Xunit;

namespace OpenZH.Data.Tests.W3d
{
    public class W3dFileTests
    {
        private const string BigFilePath = @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour\W3DZH.big";

        [Fact]
        public void LoadW3dFromBigFile()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                var entry = bigArchive.GetEntry(@"Art\W3D\ABBarracks_AC.W3D");

                using (var entryStream = entry.Open())
                using (var binaryReader = new BinaryReader(entryStream))
                {
                    var w3dFile = W3dFile.Parse(binaryReader);

                    Assert.Equal(6u, w3dFile.ChunkCount);
                    Assert.Equal(3, w3dFile.Meshes.Length);
                }
            }
        }
    }
}
