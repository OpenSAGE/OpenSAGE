using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class Terrain : GraphicsObject
    {
        public const int PatchSize = 17;

        private readonly List<TerrainTexture> _terrainTextures;

        private readonly TerrainEffect _terrainEffect;

        private readonly EffectPipelineStateHandle _pipelineStateSolid;
        private readonly EffectPipelineStateHandle _pipelineStateWireframe;

        private readonly int _numPatchesX, _numPatchesY;
        private readonly TerrainPatch[,] _patches;

        public HeightMap HeightMap { get; }

        public BoundingBox BoundingBox { get; }

        public bool RenderWireframeOverlay { get; set; }

        public Terrain(
            MapFile mapFile,
            GraphicsDevice graphicsDevice,
            FileSystem fileSystem,
            IniDataContext iniDataContext,
            ContentManager contentManager)
        {
            _terrainEffect = AddDisposable(new TerrainEffect(graphicsDevice, mapFile.BlendTileData.Textures.Length));

            _pipelineStateSolid = new EffectPipelineState(
                RasterizerStateDescription.CullBackSolid,
                DepthStencilStateDescription.Default,
                BlendStateDescription.Opaque)
                .GetHandle();

            _pipelineStateWireframe = new EffectPipelineState(
                RasterizerStateDescription.CullBackWireframe,
                DepthStencilStateDescription.DepthRead,
                BlendStateDescription.Opaque)
                .GetHandle();

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
                contentManager);

            var textureDetailsBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                textureDetails));

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
                    cliffDetails))
                : null;

            uploadBatch.End();

            var tileDataTextureView = AddDisposable(ShaderResourceView.Create(graphicsDevice, tileDataTexture));
            var cliffDetailsBufferView = AddDisposable(ShaderResourceView.Create(graphicsDevice, cliffDetailsBuffer));
            var textureDetailsBufferView = AddDisposable(ShaderResourceView.Create(graphicsDevice, textureDetailsBuffer));
            var texturesView = AddDisposable(ShaderResourceView.Create(graphicsDevice, textures));

            _terrainEffect.SetTileData(tileDataTextureView);
            _terrainEffect.SetCliffDetails(cliffDetailsBufferView);
            _terrainEffect.SetTextureDetails(textureDetailsBufferView);
            _terrainEffect.SetTextures(texturesView);
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
            ContentManager contentManager)
        {
            var numTextures = blendTileData.Textures.Length;

            var textures = new Texture[numTextures];
            var textureDetails = new TextureInfo[numTextures];
            for (var i = 0; i < numTextures; i++)
            {
                var mapTexture = blendTileData.Textures[i];

                var terrainType = terrainTextures.First(x => x.Name == mapTexture.Name);

                var texturePath = Path.Combine("Art", "Terrain", terrainType.Texture);
                textures[i] = contentManager.Load<Texture>(texturePath, uploadBatch);

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

        public void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 view,
            ref Matrix4x4 projection,
            ref Lights lights)
        {
            _terrainEffect.Begin(commandEncoder);

            _terrainEffect.SetView(ref view);
            _terrainEffect.SetProjection(ref projection);

            Draw(
                commandEncoder,
                _pipelineStateSolid,
                ref lights,
                ref view,
                ref projection);

            if (RenderWireframeOverlay)
            {
                var blackLights = new Lights();
                Draw(
                    commandEncoder,
                    _pipelineStateWireframe,
                    ref blackLights,
                    ref view,
                    ref projection);
            }
        }

        private void Draw(
            CommandEncoder commandEncoder,
            EffectPipelineStateHandle pipelineStateHandle,
            ref Lights lights,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            _terrainEffect.SetPipelineState(pipelineStateHandle);

            _terrainEffect.SetLights(ref lights);

            _terrainEffect.Apply(commandEncoder);

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
