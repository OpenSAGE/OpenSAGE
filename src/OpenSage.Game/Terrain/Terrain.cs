using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class Terrain : GraphicsObject
    {
        public const int PatchSize = 17;

        // TODO: Don't keep this in memory.
        private readonly MapFile _mapFile;

        private readonly List<TerrainTexture> _terrainTextures;

        private readonly int _numPatchesX, _numPatchesY;
        private readonly TerrainPatch[,] _patches;

        private readonly DescriptorSet _terrainDescriptorSet;

        public HeightMap HeightMap { get; }

        public BoundingBox BoundingBox { get; }

        public Terrain(
            MapFile mapFile,
            GraphicsDevice graphicsDevice,
            FileSystem fileSystem,
            TextureCache textureCache,
            DescriptorSetLayout terrainDescriptorSetLayout)
        {
            _mapFile = mapFile;

            var iniDataContext = new IniDataContext();
            iniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\Terrain.ini"));
            _terrainTextures = iniDataContext.TerrainTextures;

            var indexBufferCache = AddDisposable(new TerrainPatchIndexBufferCache(graphicsDevice));

            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            const int numTilesPerPatch = PatchSize - 1;

            HeightMap = new HeightMap(mapFile.HeightMapData);

            _numPatchesX = HeightMap.Width / numTilesPerPatch;
            if (HeightMap.Width % numTilesPerPatch != 0)
            {
                _numPatchesX += 1;
            }

            _numPatchesY = HeightMap.Height / numTilesPerPatch;
            if (HeightMap.Height % numTilesPerPatch != 0)
            {
                _numPatchesY += 1;
            }

            _patches = new TerrainPatch[_numPatchesX, _numPatchesY];

            for (var y = 0; y < _numPatchesY; y++)
            {
                for (var x = 0; x < _numPatchesX; x++)
                {
                    var patchX = x * numTilesPerPatch;
                    var patchY = y * numTilesPerPatch;

                    var patchBounds = new Int32Rect
                    {
                        X = patchX,
                        Y = patchY,
                        Width = System.Math.Min(PatchSize, HeightMap.Width - patchX),
                        Height = System.Math.Min(PatchSize, HeightMap.Height - patchY)
                    };

                    var terrainPatch = AddDisposable(new TerrainPatch(
                        HeightMap,
                        mapFile.BlendTileData,
                        patchBounds,
                        graphicsDevice,
                        uploadBatch,
                        indexBufferCache));

                    if (x == 0 && y == 0)
                    {
                        BoundingBox = terrainPatch.BoundingBox;
                    }
                    else
                    {
                        BoundingBox = BoundingBox.CreateMerged(BoundingBox, terrainPatch.BoundingBox);
                    }

                    _patches[x, y] = terrainPatch;
                }
            }

            var (textures, textureDetails) = CreateTextures(
                graphicsDevice,
                uploadBatch,
                mapFile.BlendTileData,
                fileSystem,
                _terrainTextures,
                textureCache);

            _terrainDescriptorSet = AddDisposable(new DescriptorSet(
                graphicsDevice,
                terrainDescriptorSetLayout));

            var textureDetailsBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                textureDetails,
                false));

            var tileData = new uint[HeightMap.Width * HeightMap.Height * 4];

            var tileDataIndex = 0;
            for (var y = 0; y < HeightMap.Height; y++)
            {
                for (var x = 0; x < HeightMap.Width; x++)
                {
                    var baseTextureIndex = (byte) mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex;

                    BlendData blendData1 = GetBlendData(mapFile, x, y, mapFile.BlendTileData.Blends[x, y], baseTextureIndex);
                    BlendData blendData2 = GetBlendData(mapFile, x, y, mapFile.BlendTileData.ThreeWayBlends[x, y], baseTextureIndex);

                    uint packedTextureIndices = 0;
                    packedTextureIndices |= baseTextureIndex;
                    packedTextureIndices |= (uint) (blendData1.TextureIndex << 8);
                    packedTextureIndices |= (uint) (blendData2.TextureIndex << 16);

                    tileData[tileDataIndex++] = packedTextureIndices;

                    var packedBlendInfo = 0u;
                    packedBlendInfo |= blendData1.BlendDirection;
                    packedBlendInfo |= (uint) (blendData1.Flags << 8);
                    packedBlendInfo |= (uint) (blendData2.BlendDirection << 16);
                    packedBlendInfo |= (uint) (blendData2.Flags << 24);

                    tileData[tileDataIndex++] = packedBlendInfo;

                    tileData[tileDataIndex++] = mapFile.BlendTileData.CliffTextures[x, y];

                    tileData[tileDataIndex++] = 0;
                }
            }

            byte[] textureIDsByteArray = new byte[tileData.Length * sizeof(uint)];
            System.Buffer.BlockCopy(tileData, 0, textureIDsByteArray, 0, tileData.Length * sizeof(uint));
            var tileDataTexture = AddDisposable(Texture.CreateTexture2D(
                graphicsDevice,
                uploadBatch,
                PixelFormat.Rgba32UInt,
                HeightMap.Width,
                HeightMap.Height,
                new[]
                {
                    new TextureMipMapData
                    {
                        BytesPerRow = HeightMap.Width * sizeof(uint) * 4,
                        Data = textureIDsByteArray
                    }
                }));

            var cliffDetails = new CliffInfo[mapFile.BlendTileData.CliffTextureMappings.Length];

            const int cliffScalingFactor = 64;
            for (var i = 0; i < cliffDetails.Length; i++)
            {
                var cliffMapping = mapFile.BlendTileData.CliffTextureMappings[i];
                cliffDetails[i] = new CliffInfo
                {
                    BottomLeftUV = cliffMapping.BottomLeftCoords.ToVector2() * cliffScalingFactor,
                    BottomRightUV = cliffMapping.BottomRightCoords.ToVector2() * cliffScalingFactor,
                    TopLeftUV = cliffMapping.TopLeftCoords.ToVector2() * cliffScalingFactor,
                    TopRightUV = cliffMapping.TopRightCoords.ToVector2() * cliffScalingFactor
                };
            }

            var cliffDetailsBuffer = cliffDetails.Length > 0
                ? AddDisposable(StaticBuffer.Create(
                    graphicsDevice,
                    uploadBatch,
                    cliffDetails,
                    false))
                : null;

            _terrainDescriptorSet.SetTexture(0, tileDataTexture);
            _terrainDescriptorSet.SetStructuredBuffer(1, cliffDetailsBuffer);
            _terrainDescriptorSet.SetStructuredBuffer(2, textureDetailsBuffer);
            _terrainDescriptorSet.SetTextures(3, textures);

            uploadBatch.End();
        }

        private BlendData GetBlendData(MapFile mapFile, int x, int y, ushort blendIndex, byte baseTextureIndex)
        {
            if (blendIndex > 0)
            {
                var blendDescription = mapFile.BlendTileData.BlendDescriptions[blendIndex - 1];
                var flipped = blendDescription.Flags.HasFlag(BlendFlags.Flipped);
                var flags = (byte) (flipped ? 1 : 0);
                if (blendDescription.TwoSided)
                {
                    flags |= 2;
                }
                return new BlendData
                {
                    TextureIndex = (byte) mapFile.BlendTileData.TextureIndices[(int) blendDescription.SecondaryTextureTile].TextureIndex,
                    BlendDirection = (byte) blendDescription.BlendDirection,
                    Flags = flags
                };
            }
            else
            {
                return new BlendData
                {
                    TextureIndex = baseTextureIndex
                };
            }
        }

        private struct BlendData
        {
            public byte TextureIndex;
            public byte BlendDirection;
            public byte Flags;
        }

        private static (Texture[], TextureInfo[]) CreateTextures(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            BlendTileData blendTileData,
            FileSystem fileSystem,
            List<TerrainTexture> terrainTextures,
            TextureCache textureCache)
        {
            var numTextures = blendTileData.Textures.Length;

            if (numTextures > Map.MaxTextures)
            {
                throw new System.NotSupportedException();
            }

            var textures = new Texture[numTextures];
            var textureDetails = new TextureInfo[numTextures];
            for (var i = 0; i < numTextures; i++)
            {
                var mapTexture = blendTileData.Textures[i];

                var terrainType = terrainTextures.FirstOrDefault(x => x.Name == mapTexture.Name);

                FileSystemEntry textureFileSystemEntry;
                if (terrainType != null)
                {
                    var texturePath = $@"Art\Terrain\{terrainType.Texture}";
                    textureFileSystemEntry = fileSystem.GetFile(texturePath);
                }
                else
                {
                    textureFileSystemEntry = null;
                }

                if (textureFileSystemEntry != null)
                {
                    textures[i] = textureCache.GetTexture(
                        textureFileSystemEntry,
                        uploadBatch);
                }
                else
                {
                    textures[i] = textureCache.GetPlaceholderTexture(uploadBatch);
                }

                textureDetails[i] = new TextureInfo
                {
                    CellSize = mapTexture.CellSize * 2
                };
            }
            return (textures, textureDetails);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextureInfo
        {
            public uint CellSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CliffInfo
        {
            public Vector2 BottomLeftUV;
            public Vector2 BottomRightUV;
            public Vector2 TopRightUV;
            public Vector2 TopLeftUV;
        }

        public string GetTileDescription(int x, int y)
        {
            var blendTileData = _mapFile.BlendTileData;

            string getTextureName(int textureTile)
            {
                var textureIndex = (uint) blendTileData.TextureIndices[textureTile].TextureIndex;
                return blendTileData.Textures[textureIndex].Name;
            }

            var result = new StringBuilder();

            result.Append($"Base texture: {getTextureName(blendTileData.Tiles[x, y])}");

            var blend = blendTileData.Blends[x, y];
            if (blend > 0)
            {
                var blendDescription = blendTileData.BlendDescriptions[blend - 1];
                result.Append($"; Blend <Texture: {getTextureName((int) blendDescription.SecondaryTextureTile)}");
                result.Append($", Dir: {blendDescription.BlendDirection}");
                result.Append($", Flags: {blendDescription.Flags}");
                result.Append($", Adjacent sides: {blendDescription.TwoSided}>");
            }

            return result.ToString();
        }

        public Vector3? Intersect(Ray ray)
        {
            if (ray.Intersects(BoundingBox) == null)
            {
                return null;
            }

            float? closestIntersection = null;

            for (var y = 0; y < _numPatchesY; y++)
            {
                for (var x = 0; x < _numPatchesX; x++)
                {
                    _patches[x, y].Intersect(ray, ref closestIntersection);
                }
            }

            if (closestIntersection == null)
            {
                return null;
            }

            return ray.Position + (ray.Direction * closestIntersection.Value);
        }

        public void Draw(CommandEncoder commandEncoder)
        {
            commandEncoder.SetDescriptorSet(2, _terrainDescriptorSet);

            for (var y = 0; y < _numPatchesY; y++)
            {
                for (var x = 0; x < _numPatchesX; x++)
                {
                    // TODO: Frustum culling.
                    _patches[x, y].Draw(commandEncoder);
                }
            }
        }
    }
}
