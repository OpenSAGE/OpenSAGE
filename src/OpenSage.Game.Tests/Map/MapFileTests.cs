using System.IO;
using System.Runtime.CompilerServices;
using OpenSage.Data.Map;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Map
{
    public class MapFileTests
    {
        private readonly ITestOutputHelper _output;

        public MapFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanRoundtripMaps()
        {
            InstalledFilesTestData.ReadFiles(".map", _output, entry =>
            {
                // These maps have false positive differences, so ignore them.
                switch (entry.FilePath)
                {
                    // Differences in passability data, because the original file appears to have
                    // un-initialized (random) values for partial passability bytes beyond the map width.
                    case @"Maps\USA07-TaskForces\USA07-TaskForces.map":
                        return;
                }

                using (var entryStream = entry.Open())
                {
                    TestUtility.DoRoundtripTest(
                        () => MapFile.Decompress(entryStream),
                        stream => MapFile.FromStream(stream),
                        (mapFile, stream) => mapFile.WriteTo(stream));
                }
            });
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

            Assert.Equal(9, mapFile.BlendTileData.Textures.Length);

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

                    Assert.Equal(0, mapFile.BlendTileData.CliffTextures[x, y]);
                }
            }

            void assertBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction, bool reversed = false)
            {
                var blendIndex = mapFile.BlendTileData.Blends[x, y];

                var blend = mapFile.BlendTileData.BlendDescriptions[blendIndex - 1];

                Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[(int) blend.SecondaryTextureTile].TextureIndex);

                Assert.Equal(direction, blend.BlendDirection);

                Assert.Equal(reversed, blend.Flags.HasFlag(BlendFlags.Flipped));
            }

            void assertBlends(int startY, int textureIndex, bool includeBottom)
            {
                if (includeBottom)
                {
                    assertBlend(0, startY + 0, textureIndex, BlendDirection.BlendTowardsTopRight);
                    assertBlend(1, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(2, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(3, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(4, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(5, startY + 0, textureIndex, BlendDirection.BlendTowardsTopLeft);
                }

                assertBlend(0, startY + 1, textureIndex, BlendDirection.BlendTowardsRight);
                assertBlend(5, startY + 1, textureIndex, BlendDirection.BlendTowardsRight, true);

                assertBlend(0, startY + 2, textureIndex, BlendDirection.BlendTowardsTopRight, true);
                assertBlend(1, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(2, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(3, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(4, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(5, startY + 2, textureIndex, BlendDirection.BlendTowardsTopLeft, true);
            }

            assertBlends(0, 1, true);
            assertBlends(2, 2, false);

            void assertThreeWayBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction)
            {
                var threeWayBlend = mapFile.BlendTileData.ThreeWayBlends[x, y];

                var blendDescription = mapFile.BlendTileData.BlendDescriptions[threeWayBlend - 1];

                Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[(int) blendDescription.SecondaryTextureTile].TextureIndex);

                Assert.Equal(direction, blendDescription.BlendDirection);
            }

            assertThreeWayBlend(1, 2, 2, BlendDirection.BlendTowardsTop);
            assertThreeWayBlend(2, 2, 2, BlendDirection.BlendTowardsTop);
            assertThreeWayBlend(3, 2, 2, BlendDirection.BlendTowardsTop);
            assertThreeWayBlend(4, 2, 2, BlendDirection.BlendTowardsTop);
        }

        [Fact]
        public void BlendTileData_CliffTextures()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(2, mapFile.BlendTileData.Textures.Length);
        }

        [Fact]
        public void WorldInfo()
        {
            var mapFile = GetMapFile();

            //Assert.Equal(MapCompressionType.RefPack, mapFile.WorldInfo.Compression);
            //Assert.Equal("FooBar", mapFile.WorldInfo.MapName);
            //Assert.Equal(MapWeatherType.Snowy, mapFile.WorldInfo.Weather);
        }

        [Fact]
        public void SidesList()
        {
            var mapFile = GetMapFile();

            Assert.Equal(5, mapFile.SidesList.Players.Length);

            //var player0 = mapFile.SidesList.Players[0];
            //Assert.Equal("", player0.Name);
            //Assert.Equal(false, player0.IsHuman);
            //Assert.Equal("Neutral", player0.DisplayName);
            //Assert.Equal("", player0.Faction);
            //Assert.Equal("", player0.Allies);
            //Assert.Equal("", player0.Enemies);
            //Assert.Equal(null, player0.Color);

            //var player1 = mapFile.SidesList.Players[1];
            //Assert.Equal("Human_Player", player1.Name);
            //Assert.Equal(true, player1.IsHuman);
            //Assert.Equal("Human Player", player1.DisplayName);
            //Assert.Equal("FactionAmerica", player1.Faction);
            //Assert.Equal("", player1.Allies);
            //Assert.Equal("", player1.Enemies);
            //Assert.Equal(new MapColorArgb(0xFF, 0x96, 0, 0xC8), player1.Color);

            //var player2 = mapFile.SidesList.Players[2];
            //Assert.Equal("Computer_Player_1", player2.Name);
            //Assert.Equal(false, player2.IsHuman);
            //Assert.Equal("Computer Player 1", player2.DisplayName);
            //Assert.Equal("FactionChina", player2.Faction);
            //Assert.Equal("Human_Player", player2.Allies);
            //Assert.Equal("Computer_Player_2 Computer_Player_3", player2.Enemies);
            //Assert.Equal(null, player2.Color);

            //var player3 = mapFile.SidesList.Players[3];
            //Assert.Equal("Computer_Player_2", player3.Name);
            //Assert.Equal(false, player3.IsHuman);
            //Assert.Equal("Computer Player 2", player3.DisplayName);
            //Assert.Equal("FactionGLA", player3.Faction);
            //Assert.Equal("", player3.Allies);
            //Assert.Equal("", player3.Enemies);
            //Assert.Equal(null, player3.Color);

            //var player4 = mapFile.SidesList.Players[4];
            //Assert.Equal("Computer_Player_3", player4.Name);
            //Assert.Equal(false, player4.IsHuman);
            //Assert.Equal("Computer Player 3", player4.DisplayName);
            //Assert.Equal("FactionGLADemolitionGeneral", player4.Faction);
            //Assert.Equal("", player4.Allies);
            //Assert.Equal("", player4.Enemies);
            //Assert.Equal(new MapColorArgb(0xFF, 0xFF, 0x96, 0xFF), player4.Color);

            Assert.Equal(11, mapFile.SidesList.Teams.Length);
        }

        [Fact]
        public void SidesList_Scripts()
        {
            GetMapFile();
        }

        [Fact]
        public void BlendTileData_Passability()
        {
            var mapFile = GetMapFile();

            Assert.Equal(false, mapFile.BlendTileData.Impassability[0, 0]);
            Assert.Equal(false, mapFile.BlendTileData.Impassability[0, 1]);
            Assert.Equal(true, mapFile.BlendTileData.Impassability[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.Impassability[1, 1]);

            Assert.Equal(false, mapFile.BlendTileData.ImpassabilityToPlayers[0, 0]);
            Assert.Equal(true, mapFile.BlendTileData.ImpassabilityToPlayers[0, 1]);
            Assert.Equal(false, mapFile.BlendTileData.ImpassabilityToPlayers[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.ImpassabilityToPlayers[1, 1]);

            Assert.Equal(true, mapFile.BlendTileData.PassageWidths[0, 0]);
            Assert.Equal(false, mapFile.BlendTileData.PassageWidths[0, 1]);
            Assert.Equal(false, mapFile.BlendTileData.PassageWidths[1, 0]);
            Assert.Equal(true, mapFile.BlendTileData.PassageWidths[1, 1]);

            Assert.Equal(false, mapFile.BlendTileData.Taintability[0, 0]);
            Assert.Equal(true, mapFile.BlendTileData.Taintability[0, 1]);
            Assert.Equal(true, mapFile.BlendTileData.Taintability[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.Taintability[1, 1]);

            Assert.Equal(true, mapFile.BlendTileData.ExtraPassability[0, 0]);
            Assert.Equal(false, mapFile.BlendTileData.ExtraPassability[0, 1]);
            Assert.Equal(false, mapFile.BlendTileData.ExtraPassability[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.ExtraPassability[1, 1]);
        }

        private static MapFile GetMapFile([CallerMemberName] string testName = null)
        {
            var fileName = Path.Combine("Map", "Assets", testName + ".map");

            using (var entryStream = File.OpenRead(fileName))
            {
                return MapFile.FromStream(entryStream);
            }
        }
    }
}
