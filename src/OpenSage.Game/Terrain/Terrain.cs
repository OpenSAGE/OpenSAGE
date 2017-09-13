using System.Linq;
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

        public Terrain(
            MapFile mapFile,
            GraphicsDevice graphicsDevice,
            FileSystem fileSystem,
            TextureCache textureCache,
            DescriptorSetLayout terrainDescriptorSetLayout,
            DescriptorSetLayout terrainPatchDescriptorSetLayout)
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

                    _patches[x, y] = AddDisposable(new TerrainPatch(
                        HeightMap,
                        mapFile.BlendTileData,
                        patchBounds,
                        graphicsDevice,
                        uploadBatch,
                        indexBufferCache,
                        terrainPatchDescriptorSetLayout));
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

            _terrainDescriptorSet.SetStructuredBuffer(0, textureDetailsBuffer);
            _terrainDescriptorSet.SetTextures(1, textures);

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
                    CellSize = mapTexture.CellSize
                };
            }
            return (textures, textureDetails);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextureInfo
        {
            public uint CellSize;
        }

        public void Draw(CommandEncoder commandEncoder)
        {
            commandEncoder.SetDescriptorSet(3, _terrainDescriptorSet);

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
