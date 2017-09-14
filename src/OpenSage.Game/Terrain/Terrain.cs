using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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
                    var textureIndex = (uint) mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex;

                    uint secondaryTextureIndex;
                    uint blendDirection;
                    uint reverseDirection;

                    var blend = mapFile.BlendTileData.Blends[x, y];
                    if (blend > 0)
                    {
                        var blendDescription = mapFile.BlendTileData.BlendDescriptions[blend - 1];
                        secondaryTextureIndex = (uint) mapFile.BlendTileData.TextureIndices[(int) blendDescription.SecondaryTextureTile].TextureIndex;
                        blendDirection = (uint) blendDescription.BlendDirection;
                        reverseDirection = blendDescription.Flags.HasFlag(BlendFlags.ReverseDirection) 
                            ? 1u 
                            : 0u;
                    }
                    else
                    {
                        secondaryTextureIndex = textureIndex;
                        blendDirection = 0;
                        reverseDirection = 0;
                    }

                    tileData[tileDataIndex++] = textureIndex;
                    tileData[tileDataIndex++] = secondaryTextureIndex;
                    tileData[tileDataIndex++] = blendDirection;
                    tileData[tileDataIndex++] = reverseDirection;
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

            _terrainDescriptorSet.SetTexture(0, tileDataTexture);
            _terrainDescriptorSet.SetStructuredBuffer(1, textureDetailsBuffer);
            _terrainDescriptorSet.SetTextures(2, textures);

            uploadBatch.End();
        }

        private static (Texture[], TextureInfo[]) CreateTextures(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            BlendTileData blendTileData,
            FileSystem fileSystem,
            TextureCache textureCache)
        {
            var iniDataContext = new IniDataContext();
            iniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\Terrain.ini"));

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

                var terrainType = iniDataContext.Terrains.First(x => x.Name == mapTexture.Name);

                var texturePath = $@"Art\Terrain\{terrainType.Texture}";
                var textureFileSystemEntry = fileSystem.GetFile(texturePath);

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
