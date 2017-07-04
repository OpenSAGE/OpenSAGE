using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenZH.Data.Big;
using OpenZH.Data.Map;
using Xunit;
using Xunit.Abstractions;

namespace OpenZH.Data.Tests.Map
{
    public class MapFileTests
    {
        private const string BigFilePath = @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour\MapsZH.big";

        private readonly ITestOutputHelper _output;

        public MapFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadMaps()
        {
            using (var bigStream = File.OpenRead(BigFilePath))
            using (var bigArchive = new BigArchive(bigStream))
            {
                foreach (var entry in bigArchive.Entries.Where(x => Path.GetExtension(x.FullName).ToLowerInvariant() == ".map"))
                {
                    using (var entryStream = entry.Open())
                    using (var binaryReader = new BinaryReader(entryStream))
                    {
                        var mapFile = MapFile.Parse(binaryReader);

                        for (var y = 0; y < mapFile.HeightMapData.Height; y++)
                        {
                            for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                            {
                                Assert.InRange(
                                    mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex,
                                    0,
                                    mapFile.BlendTileData.Textures.Length - 1);
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public void MapTestSuite()
        {
            foreach (var entry in Directory.GetFiles(@"C:\Users\Tim Jones\Desktop\ZH", "*.map", SearchOption.AllDirectories))
            {
                if (!entry.Contains("Alpine Assault Uncompressed"))
                    continue;

                Debug.WriteLine(entry);
                using (var entryStream = File.OpenRead(entry))
                using (var binaryReader = new BinaryReader(entryStream))
                {
                    var mapFile = MapFile.Parse(binaryReader);

                    //Assert.True(tgaFile.Header.ImagePixelSize == 24 || tgaFile.Header.ImagePixelSize == 32);
                }
            }
        }

        [Fact]
        public void BlendTileData_SingleTexture_128_NoBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(1, mapFile.BlendTileData.Textures.Length);
            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[0].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    Assert.Equal(0, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);
                }
            }
        }

        [Fact]
        public void BlendTileData_SingleTexture_384_NoBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(2, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("CliffLargeType6", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[0].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    Assert.Equal(0, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);
                }
            }
        }

        [Fact]
        public void BlendTileData_TwoTextures_384_128_NoBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(2, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("CliffLargeType2", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[0].CellSize);

            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[1].Name);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[1].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[1].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[1].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    var textureIndex = x == 1 && y == 0 ? 1 : 0;
                    Assert.Equal(textureIndex, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);
                }
            }
        }

        [Fact]
        public void BlendTileData_MultipleTextures_TwoWayBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(5, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[0].CellSize);

            Assert.Equal("SandLargeType3Light", mapFile.BlendTileData.Textures[1].Name);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[1].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[1].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[1].CellSize);

            Assert.Equal("CliffLargeType8", mapFile.BlendTileData.Textures[2].Name);
            Assert.Equal(40u, mapFile.BlendTileData.Textures[2].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[2].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[2].CellSize);

            Assert.Equal("GrassLargeType1", mapFile.BlendTileData.Textures[3].Name);
            Assert.Equal(76u, mapFile.BlendTileData.Textures[3].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[3].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[3].CellSize);

            Assert.Equal("SnowLargeType1", mapFile.BlendTileData.Textures[4].Name);
            Assert.Equal(112u, mapFile.BlendTileData.Textures[4].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[4].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[4].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    int textureIndex;
                    if (y == 1 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 1;
                    else if (y == 4 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 2;
                    else if (y == 7 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 3;
                    else if (y == 10 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 4;
                    else
                        textureIndex = 0;
                    Assert.Equal(textureIndex, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);

                    Assert.Equal(0, mapFile.BlendTileData.ThreeWayBlends[x, y]);
                }
            }

            void assertBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction)
            {
                var blendIndex = mapFile.BlendTileData.Blends[x, y];

                var blend = mapFile.BlendTileData.BlendDescriptions[blendIndex - 1];

                Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[blend.SecondaryTextureTile].TextureIndex);

                Assert.Equal(direction, blend.BlendDirection);
            }

            void assertBlends(int startY, int textureIndex)
            {
                assertBlend(0, startY + 0, textureIndex, BlendDirection.BottomLeft);
                assertBlend(1, startY + 0, textureIndex, BlendDirection.Bottom);
                assertBlend(2, startY + 0, textureIndex, BlendDirection.Bottom);
                assertBlend(3, startY + 0, textureIndex, BlendDirection.Bottom);
                assertBlend(4, startY + 0, textureIndex, BlendDirection.Bottom);
                assertBlend(5, startY + 0, textureIndex, BlendDirection.BottomRight);

                assertBlend(0, startY + 1, textureIndex, BlendDirection.Left);
                assertBlend(5, startY + 1, textureIndex, BlendDirection.Right);

                assertBlend(0, startY + 2, textureIndex, BlendDirection.TopLeft);
                assertBlend(1, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(2, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(3, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(4, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(5, startY + 2, textureIndex, BlendDirection.TopRight);
            }

            assertBlends(0, 1);
            assertBlends(3, 2);
            assertBlends(6, 3);
        }

        [Fact]
        public void BlendTileData_TwoTextures_ThreeWayBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(3, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[0].CellSize);

            Assert.Equal("SandLargeType3Light", mapFile.BlendTileData.Textures[1].Name);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[1].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[1].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[1].CellSize);

            Assert.Equal("CliffLargeType3b", mapFile.BlendTileData.Textures[2].Name);
            Assert.Equal(40u, mapFile.BlendTileData.Textures[2].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[2].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[2].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    int textureIndex;
                    if (y == 1 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 1;
                    else if (y == 3 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 2;
                    else
                        textureIndex = 0;
                    Assert.Equal(textureIndex, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);

                    if (y == 2 && (x == 1 || x == 2 || x == 3 || x == 4))
                    {
                        Assert.NotEqual(0, mapFile.BlendTileData.ThreeWayBlends[x, y]);
                    }
                    else
                    {
                        Assert.Equal(0, mapFile.BlendTileData.ThreeWayBlends[x, y]);
                    }
                }
            }

            void assertBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction)
            {
                var blendIndex = mapFile.BlendTileData.Blends[x, y];

                var blend = mapFile.BlendTileData.BlendDescriptions[blendIndex - 1];

                Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[blend.SecondaryTextureTile].TextureIndex);

                Assert.Equal(direction, blend.BlendDirection);
            }

            void assertBlends(int startY, int textureIndex, bool includeBottom)
            {
                if (includeBottom)
                {
                    assertBlend(0, startY + 0, textureIndex, BlendDirection.BottomLeft);
                    assertBlend(1, startY + 0, textureIndex, BlendDirection.Bottom);
                    assertBlend(2, startY + 0, textureIndex, BlendDirection.Bottom);
                    assertBlend(3, startY + 0, textureIndex, BlendDirection.Bottom);
                    assertBlend(4, startY + 0, textureIndex, BlendDirection.Bottom);
                    assertBlend(5, startY + 0, textureIndex, BlendDirection.BottomRight);
                }

                assertBlend(0, startY + 1, textureIndex, BlendDirection.Left);
                assertBlend(5, startY + 1, textureIndex, BlendDirection.Right);

                assertBlend(0, startY + 2, textureIndex, BlendDirection.TopLeft);
                assertBlend(1, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(2, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(3, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(4, startY + 2, textureIndex, BlendDirection.Top);
                assertBlend(5, startY + 2, textureIndex, BlendDirection.TopRight);
            }

            assertBlends(0, 1, true);
            assertBlends(2, 2, false);

            void assertThreeWayBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction)
            {
                var threeWayBlend = mapFile.BlendTileData.ThreeWayBlends[x, y];

                var blendDescription = mapFile.BlendTileData.BlendDescriptions[threeWayBlend - 1];

                Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[blendDescription.SecondaryTextureTile].TextureIndex);

                Assert.Equal(direction, blendDescription.BlendDirection);
            }

            assertThreeWayBlend(1, 2, 2, BlendDirection.Bottom);
            assertThreeWayBlend(2, 2, 2, BlendDirection.Bottom);
            assertThreeWayBlend(3, 2, 2, BlendDirection.Bottom);
            assertThreeWayBlend(4, 2, 2, BlendDirection.Bottom);

            //for (var i = 0; i < mapFile.BlendTileData.BlendDescriptions.Length; i++)
            //{
            //    var blend = mapFile.BlendTileData.BlendDescriptions[i];
            //    var binary = string.Empty;
            //    foreach (var value in BitConverter.GetBytes(blend.SecondaryTextureTile))
            //    {
            //        binary += Convert.ToString(value, 2).PadLeft(8, '0') + " ";
            //    }
            //    foreach (var value in blend.BlendDirectionBytes)
            //    {
            //        binary += value + " ";
            //    }
            //    binary = binary.TrimEnd(' ');
            //    var usages = string.Empty;
            //    for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            //    {
            //        for (var x = 0; x < mapFile.HeightMapData.Width; x++)
            //        {
            //            var blendIndex = mapFile.BlendTileData.Blends[x, y];
            //            if (blendIndex == i + 1)
            //            {
            //                usages += $"[{x}, {y}], ";
            //            }
            //        }
            //    }
            //    usages = usages.TrimEnd(' ', ',');
            //    _output.WriteLine($"{(i + 1).ToString().PadLeft(2, ' ')} = {binary} {usages}");
            //}
        }

        private static MapFile GetMapFile([CallerMemberName] string testName = null)
        {
            var fileName = Path.Combine("Map", "Assets", testName + ".map");

            using (var entryStream = File.OpenRead(fileName))
            using (var binaryReader = new BinaryReader(entryStream))
            {
                return MapFile.Parse(binaryReader);
            }
        }
    }
}
