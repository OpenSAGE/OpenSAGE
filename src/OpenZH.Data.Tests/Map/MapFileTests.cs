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
                if (!entry.Contains("BUG_"))
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

                    Assert.Equal(0, mapFile.BlendTileData.CliffBlends[x, y]);
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
        public void BlendTileData_ThreeWayBlending()
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

                    Assert.Equal(0, mapFile.BlendTileData.CliffBlends[x, y]);
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

        [Fact]
        public void BlendTileData_CliffTextures()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(4, mapFile.BlendTileData.Textures.Length);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    //if (x >= 3 && x <= 5 && y == 3)
                    //{
                    //    Assert.NotEqual(0, mapFile.BlendTileData.CliffBlends[x, y]);
                    //}
                    //else
                    //{
                    //    Assert.Equal(0, mapFile.BlendTileData.CliffBlends[x, y]);
                    //}
                }
            }

            //void assertCliffBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction)
            //{
            //    var blendIndex = mapFile.BlendTileData.Blends[x, y];

            //    var blend = mapFile.BlendTileData.BlendDescriptions[blendIndex - 1];

            //    Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[blend.SecondaryTextureTile].TextureIndex);

            //    Assert.Equal(direction, blend.BlendDirection);
            //}
            

            //assertBlends(0, 1, true);
            //assertBlends(2, 2, false);

            for (var i = 0; i < mapFile.BlendTileData.CliffBlendDescriptions.Length; i++)
            {
                var cliffBlendDescription = mapFile.BlendTileData.CliffBlendDescriptions[i];

                var unknown1Binary = string.Empty;
                foreach (var value in BitConverter.GetBytes(cliffBlendDescription.Unknown1))
                {
                    unknown1Binary += Convert.ToString(value, 2).PadLeft(8, '0') + $" ({value.ToString().PadLeft(3, ' ')}) ";
                }
                unknown1Binary = unknown1Binary.TrimEnd(' ');

                var binary = string.Empty;
                for (int j = 0; j < cliffBlendDescription.Unknown2.Length; j++)
                {
                    if (j % 8 == 0)
                        binary += Environment.NewLine;
                    binary += Convert.ToString(cliffBlendDescription.Unknown2[j], 2).PadLeft(8, '0') + $" ({cliffBlendDescription.Unknown2[j].ToString().PadLeft(3, ' ')}) ";
                    //if (j % 4 == 0 && j != 0)
                    //    binary += BitConverter. cliffBlendDescription
                }
                binary = binary.TrimEnd(' ');
                //var binary = string.Empty;
                //for (int j = 0; j < cliffBlendDescription.Unknown2.Length - 2; j += 4)
                //{
                //    if (j % 8 == 0)
                //        binary += Environment.NewLine;
                //    var temp = cliffBlendDescription.Unknown2.Skip(j).Take(4).ToArray();
                //    binary += BitConverter.ToSingle(temp, 0).ToString() + " ";
                //}
                //binary = binary.TrimEnd(' ');

                var usages = string.Empty;
                for (var y = 0; y < mapFile.HeightMapData.Height; y++)
                {
                    for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                    {
                        var blendIndex = mapFile.BlendTileData.CliffBlends[x, y];
                        if (blendIndex == i + 1)
                        {
                            usages += $"[{x}, {y}], ";
                        }
                    }
                }
                usages = usages.TrimEnd(' ', ',');
                if (usages == string.Empty)
                {
                    continue;
                }

                _output.WriteLine($"{i + 1}{Environment.NewLine}{unknown1Binary}{binary} {usages}");
                _output.WriteLine(string.Empty);
            }
        }

        [Fact]
        public void WorldInfo()
        {
            var mapFile = GetMapFile();

            Assert.Equal(Data.Map.WorldInfo.CompressionType.RefPack, mapFile.WorldInfo.Compression);
            Assert.Equal("FooBar", mapFile.WorldInfo.MapName);
            Assert.Equal(Data.Map.WorldInfo.WeatherType.Snowy, mapFile.WorldInfo.Weather);
        }

        [Fact]
        public void SidesList()
        {
            var mapFile = GetMapFile();

            Assert.Equal(5, mapFile.SidesList.Players.Length);

            var player0 = mapFile.SidesList.Players[0];
            Assert.Equal("", player0.Name);
            Assert.Equal(false, player0.IsHuman);
            Assert.Equal("Neutral", player0.DisplayName);
            Assert.Equal("", player0.Faction);
            Assert.Equal("", player0.Allies);
            Assert.Equal("", player0.Enemies);
            Assert.Equal(null, player0.Color);

            var player1 = mapFile.SidesList.Players[1];
            Assert.Equal("Human_Player", player1.Name);
            Assert.Equal(true, player1.IsHuman);
            Assert.Equal("Human Player", player1.DisplayName);
            Assert.Equal("FactionAmerica", player1.Faction);
            Assert.Equal("", player1.Allies);
            Assert.Equal("", player1.Enemies);
            Assert.Equal(new ColorArgb(0xFF, 0x96, 0, 0xC8), player1.Color);

            var player2 = mapFile.SidesList.Players[2];
            Assert.Equal("Computer_Player_1", player2.Name);
            Assert.Equal(false, player2.IsHuman);
            Assert.Equal("Computer Player 1", player2.DisplayName);
            Assert.Equal("FactionChina", player2.Faction);
            Assert.Equal("Human_Player", player2.Allies);
            Assert.Equal("Computer_Player_2 Computer_Player_3", player2.Enemies);
            Assert.Equal(null, player2.Color);

            var player3 = mapFile.SidesList.Players[3];
            Assert.Equal("Computer_Player_2", player3.Name);
            Assert.Equal(false, player3.IsHuman);
            Assert.Equal("Computer Player 2", player3.DisplayName);
            Assert.Equal("FactionGLA", player3.Faction);
            Assert.Equal("", player3.Allies);
            Assert.Equal("", player3.Enemies);
            Assert.Equal(null, player3.Color);

            var player4 = mapFile.SidesList.Players[4];
            Assert.Equal("Computer_Player_3", player4.Name);
            Assert.Equal(false, player4.IsHuman);
            Assert.Equal("Computer Player 3", player4.DisplayName);
            Assert.Equal("FactionGLADemolitionGeneral", player4.Faction);
            Assert.Equal("", player4.Allies);
            Assert.Equal("", player4.Enemies);
            Assert.Equal(new ColorArgb(0xFF, 0xFF, 0x96, 0xFF), player4.Color);

            Assert.Equal(11, mapFile.SidesList.Teams.Length);
        }

        [Fact]
        public void SidesList_Scripts()
        {
            var mapFile = GetMapFile();

            Assert.Equal(5, mapFile.SidesList.PlayerScripts.ScriptLists.Length);

            var scriptList0 = mapFile.SidesList.PlayerScripts.ScriptLists[0];
            //Assert.Equal(scriptList0.ChildNodes)

            var player0 = mapFile.SidesList.Players[0];
            Assert.Equal("", player0.Name);
            Assert.Equal(false, player0.IsHuman);
            Assert.Equal("Neutral", player0.DisplayName);
            Assert.Equal("", player0.Faction);
            Assert.Equal("", player0.Allies);
            Assert.Equal("", player0.Enemies);
            Assert.Equal(null, player0.Color);

            var player1 = mapFile.SidesList.Players[1];
            Assert.Equal("Human_Player", player1.Name);
            Assert.Equal(true, player1.IsHuman);
            Assert.Equal("Human Player", player1.DisplayName);
            Assert.Equal("FactionAmerica", player1.Faction);
            Assert.Equal("", player1.Allies);
            Assert.Equal("", player1.Enemies);
            Assert.Equal(new ColorArgb(0xFF, 0x96, 0, 0xC8), player1.Color);

            var player2 = mapFile.SidesList.Players[2];
            Assert.Equal("Computer_Player_1", player2.Name);
            Assert.Equal(false, player2.IsHuman);
            Assert.Equal("Computer Player 1", player2.DisplayName);
            Assert.Equal("FactionChina", player2.Faction);
            Assert.Equal("Human_Player", player2.Allies);
            Assert.Equal("Computer_Player_2 Computer_Player_3", player2.Enemies);
            Assert.Equal(null, player2.Color);

            var player3 = mapFile.SidesList.Players[3];
            Assert.Equal("Computer_Player_2", player3.Name);
            Assert.Equal(false, player3.IsHuman);
            Assert.Equal("Computer Player 2", player3.DisplayName);
            Assert.Equal("FactionGLA", player3.Faction);
            Assert.Equal("", player3.Allies);
            Assert.Equal("", player3.Enemies);
            Assert.Equal(null, player3.Color);

            var player4 = mapFile.SidesList.Players[4];
            Assert.Equal("Computer_Player_3", player4.Name);
            Assert.Equal(false, player4.IsHuman);
            Assert.Equal("Computer Player 3", player4.DisplayName);
            Assert.Equal("FactionGLADemolitionGeneral", player4.Faction);
            Assert.Equal("", player4.Allies);
            Assert.Equal("", player4.Enemies);
            Assert.Equal(new ColorArgb(0xFF, 0xFF, 0x96, 0xFF), player4.Color);

            Assert.Equal(11, mapFile.SidesList.Teams.Length);
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
