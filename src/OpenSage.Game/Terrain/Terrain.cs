using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.Data.Map;
using OpenSage.Data.Tga;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering.Water;
using OpenSage.Graphics.Shaders;
using OpenSage.IO;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Veldrid;
using Veldrid.ImageSharp;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Terrain
{
    public sealed class Terrain : DisposableBase
    {
        private readonly AssetLoadContext _loadContext;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly ConstantBuffer<TerrainShaderResources.TerrainMaterialConstants> _materialConstantsBuffer;

        private readonly Material _material;
        private readonly TerrainPatchIndexBufferCache _indexBufferCache;

        private readonly RenderScene _renderScene;

        private readonly List<TerrainPatch> _patches = new();

        internal const int PatchSize = 17;

        public readonly MapFile Map;

        public HeightMap HeightMap { get; }

        private float _causticsIndex;

        private const uint NumOfCausticsAnimation = 32;

        internal readonly Texture CloudTexture;

        internal RadiusCursorDecals RadiusCursorDecals => _loadContext.ShaderResources.Global.RadiusCursorDecals;

        internal Terrain(MapFile mapFile, AssetLoadContext loadContext, RenderScene scene)
        {
            Map = mapFile;

            HeightMap = new HeightMap(mapFile.HeightMapData);

            _loadContext = loadContext;
            _graphicsDevice = loadContext.GraphicsDevice;

            _renderScene = scene;

            _indexBufferCache = AddDisposable(new TerrainPatchIndexBufferCache(loadContext.GraphicsDevice));

            var tileDataTexture = AddDisposable(CreateTileDataTexture(
                loadContext.GraphicsDevice,
                mapFile,
                HeightMap));

            var cliffDetailsBuffer = AddDisposable(CreateCliffDetails(
                loadContext.GraphicsDevice,
                mapFile));

            CreateTextures(
                loadContext,
                mapFile.BlendTileData,
                out var textureArray,
                out var textureDetails);

            var textureDetailsBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticStructuredBuffer(textureDetails));

            _materialConstantsBuffer = AddDisposable(
                new ConstantBuffer<TerrainShaderResources.TerrainMaterialConstants>(
                    loadContext.GraphicsDevice, "TerrainMaterialConstants"));
            _materialConstantsBuffer.Value = new TerrainShaderResources.TerrainMaterialConstants
            {
                MapBorderWidth = new Vector2(mapFile.HeightMapData.BorderWidth, mapFile.HeightMapData.BorderWidth) * HeightMap.HorizontalScale,
                MapSize = new Vector2(mapFile.HeightMapData.Width, mapFile.HeightMapData.Height) * HeightMap.HorizontalScale,
                IsMacroTextureStretched = mapFile.EnvironmentData?.IsMacroTextureStretched ?? false
            };
            _materialConstantsBuffer.Update(loadContext.GraphicsDevice);

            var macroTexture = loadContext.AssetStore.Textures.GetByName(mapFile.EnvironmentData?.MacroTexture ?? "tsnoiseurb.dds");

            var casuticsTextures = BuildCausticsTextureArray(loadContext.AssetStore);

            var terrainShaderResources = loadContext.ShaderSetStore.GetShaderSet(() => new TerrainShaderResources(loadContext.ShaderSetStore));

            var materialResourceSet = AddDisposable(terrainShaderResources.CreateMaterialResourceSet(
                _materialConstantsBuffer.Buffer,
                tileDataTexture,
                cliffDetailsBuffer ?? loadContext.GraphicsDeviceManager.GetNullStructuredBuffer(TerrainShaderResources.CliffInfo.Size),
                textureDetailsBuffer,
                textureArray,
                macroTexture,
                casuticsTextures));

            _material = AddDisposable(
                new Material(
                    terrainShaderResources,
                    terrainShaderResources.Pipeline,
                    materialResourceSet,
                    SurfaceType.Opaque));

            CloudTexture = loadContext.AssetStore.Textures.GetByName(mapFile.EnvironmentData?.CloudTexture ?? "tscloudmed.dds");

            OnHeightMapChanged();
        }

        private int GetCausticsTextureIndex(in TimeInterval time)
        {
            var deltaTime = (float) time.DeltaTime.TotalSeconds;
            _causticsIndex += 10f * deltaTime;
            if (_causticsIndex >= NumOfCausticsAnimation)
            {
                _causticsIndex = 0;
            }
            return (int) _causticsIndex;
        }

        private Texture BuildCausticsTextureArray(AssetStore assetStore)
        {
            var textures = new List<Texture>();

            for (var i = 0; i < NumOfCausticsAnimation; i++)
            {
                var name = $"causts{i:D2}";
                var texture = assetStore.Textures.GetByName(name);
                textures.Add(texture);
            }

            var texture0 = textures[0];

            var result = AddDisposable(_graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    texture0.Width,
                    texture0.Height,
                    texture0.MipLevels,
                    (uint)textures.Count,
                    texture0.Format,
                    TextureUsage.Sampled)));

            var commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
            commandList.Begin();

            for (var i = 0; i < textures.Count; i++)
            {
                for (var mipLevel = 0u; mipLevel < texture0.MipLevels; mipLevel++)
                {
                    var mipSize = TextureMipMapData.CalculateMipSize(mipLevel, texture0.Width);

                    commandList.CopyTexture(
                        textures[i],
                        0, 0, 0,
                        mipLevel,
                        0,
                        result,
                        0, 0, 0,
                        mipLevel,
                        (uint)i,
                        mipSize,
                        mipSize,
                        1,
                        1);
                }
            }

            commandList.End();

            _graphicsDevice.SubmitCommands(commandList);
            _graphicsDevice.DisposeWhenIdle(commandList);
            _graphicsDevice.WaitForIdle();

            return result;
        }

        internal void OnHeightMapChanged()
        {
            foreach (var patch in _patches)
            {
                patch.Dispose();
                RemoveToDispose(patch);

                _renderScene.Objects.Remove(patch);
            }

            _patches.Clear();

            CreatePatches();
        }

        private void CreatePatches()
        {
            const int numTilesPerPatch = PatchSize - 1;

            var heightMapWidthMinusOne = HeightMap.Width - 1;
            var numPatchesX = heightMapWidthMinusOne / numTilesPerPatch;
            if (heightMapWidthMinusOne % numTilesPerPatch != 0)
            {
                numPatchesX += 1;
            }

            var heightMapHeightMinusOne = HeightMap.Height - 1;
            var numPatchesY = heightMapHeightMinusOne / numTilesPerPatch;
            if (heightMapHeightMinusOne % numTilesPerPatch != 0)
            {
                numPatchesY += 1;
            }

            for (var y = 0; y < numPatchesY; y++)
            {
                for (var x = 0; x < numPatchesX; x++)
                {
                    var patchX = x * numTilesPerPatch;
                    var patchY = y * numTilesPerPatch;

                    var patchBounds = new Rectangle(
                        patchX,
                        patchY,
                        Math.Min(PatchSize, HeightMap.Width - patchX),
                        Math.Min(PatchSize, HeightMap.Height - patchY));

                    var patch = AddDisposable(
                        new TerrainPatch(
                            HeightMap,
                            patchBounds,
                            _graphicsDevice,
                            _indexBufferCache,
                            _material));

                    _renderScene.Objects.Add(patch);

                    _patches.Add(patch);
                }
            }
        }

        private static Texture CreateTileDataTexture(
            GraphicsDevice graphicsDevice,
            MapFile mapFile,
            HeightMap heightMap)
        {
            var tileData = new uint[heightMap.Width * heightMap.Height * 4];

            var tileDataIndex = 0;
            for (var y = 0; y < heightMap.Height; y++)
            {
                for (var x = 0; x < heightMap.Width; x++)
                {
                    var baseTextureIndex = (byte) mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex;

                    var blendData1 = GetBlendData(mapFile, mapFile.BlendTileData.Blends[x, y], baseTextureIndex);
                    var blendData2 = GetBlendData(mapFile, mapFile.BlendTileData.ThreeWayBlends[x, y], baseTextureIndex);

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

            var textureIDsByteArray = new byte[tileData.Length * sizeof(float)];
            Buffer.BlockCopy(tileData, 0, textureIDsByteArray, 0, tileData.Length * sizeof(float));

            var rowPitch = (uint) heightMap.Width * sizeof(float) * 4;

            return graphicsDevice.CreateStaticTexture2D(
                (uint) heightMap.Width,
                (uint) heightMap.Height,
                1u,
                new TextureMipMapData(
                    textureIDsByteArray,
                    rowPitch,
                    rowPitch * (uint) heightMap.Height,
                    (uint) heightMap.Width,
                    (uint) heightMap.Height),
                PixelFormat.R32_G32_B32_A32_UInt);
        }

        private static BlendData GetBlendData(
            MapFile mapFile,
            uint blendIndex,
            byte baseTextureIndex)
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

        private static DeviceBuffer CreateCliffDetails(
            GraphicsDevice graphicsDevice,
            MapFile mapFile)
        {
            var cliffDetails = new TerrainShaderResources.CliffInfo[mapFile.BlendTileData.CliffTextureMappings.Length];

            const int cliffScalingFactor = 64;
            for (var i = 0; i < cliffDetails.Length; i++)
            {
                var cliffMapping = mapFile.BlendTileData.CliffTextureMappings[i];
                cliffDetails[i] = new TerrainShaderResources.CliffInfo
                {
                    BottomLeftUV = cliffMapping.BottomLeftCoords * cliffScalingFactor,
                    BottomRightUV = cliffMapping.BottomRightCoords * cliffScalingFactor,
                    TopLeftUV = cliffMapping.TopLeftCoords * cliffScalingFactor,
                    TopRightUV = cliffMapping.TopRightCoords * cliffScalingFactor
                };
            }

            return cliffDetails.Length > 0
                ? graphicsDevice.CreateStaticStructuredBuffer(cliffDetails)
                : null;
        }

        private void CreateTextures(
            AssetLoadContext loadContext,
            BlendTileData blendTileData,
            out Texture textureArray,
            out TerrainShaderResources.TextureInfo[] textureDetails)
        {
            var graphicsDevice = loadContext.GraphicsDevice;

            var numTextures = (uint) blendTileData.Textures.Length;

            var textureInfo = new (uint size, FileSystemEntry entry)[numTextures];
            var largestTextureSize = uint.MinValue;

            textureDetails = new TerrainShaderResources.TextureInfo[numTextures];

            for (var i = 0; i < numTextures; i++)
            {
                var mapTexture = blendTileData.Textures[i];

                var terrainType = loadContext.AssetStore.TerrainTextures.GetByName(mapTexture.Name);
                var texturePath = Path.Combine("Art", "Terrain", terrainType.Texture);
                var entry = loadContext.FileSystem.GetFile(texturePath);

                var size = (uint) TgaFile.GetSquareTextureSize(entry);

                textureInfo[i] = (size, entry);

                if (size > largestTextureSize)
                {
                    largestTextureSize = size;
                }

                textureDetails[i] = new TerrainShaderResources.TextureInfo
                {
                    TextureIndex = (uint) i,
                    CellSize = mapTexture.CellSize * 2
                };
            }

            textureArray = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    largestTextureSize,
                    largestTextureSize,
                    TextureMipMapData.CalculateMipMapCount(largestTextureSize, largestTextureSize),
                    numTextures,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled)));

            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            commandList.Begin();

            var texturesToDispose = new List<Texture>();

            for (var i = 0u; i < numTextures; i++)
            {
                var tgaFile = TgaFile.FromFileSystemEntry(textureInfo[i].entry);
                var originalData = TgaFile.ConvertPixelsToRgba8(tgaFile, true);

                using (var tgaImage = Image.LoadPixelData<Rgba32>(
                    originalData,
                    tgaFile.Header.Width,
                    tgaFile.Header.Height))
                {
                    if (tgaFile.Header.Width != largestTextureSize)
                    {
                        tgaImage.Mutate(x => x.Resize((int) largestTextureSize, (int) largestTextureSize, LanczosResampler.Lanczos3));
                    }

                    var imageSharpTexture = new ImageSharpTexture(tgaImage);

                    var sourceTexture = CreateTextureViaStaging(
                        imageSharpTexture,
                        graphicsDevice,
                        graphicsDevice.ResourceFactory);

                    texturesToDispose.Add(sourceTexture);

                    for (var mipLevel = 0u; mipLevel < imageSharpTexture.MipLevels; mipLevel++)
                    {
                        commandList.CopyTexture(
                            sourceTexture,
                            0, 0, 0,
                            mipLevel,
                            0,
                            textureArray,
                            0, 0, 0,
                            mipLevel,
                            i,
                            (uint) imageSharpTexture.Images[mipLevel].Width,
                            (uint) imageSharpTexture.Images[mipLevel].Height,
                            1,
                            1);
                    }
                }
            }

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            foreach (var texture in texturesToDispose)
            {
                graphicsDevice.DisposeWhenIdle(texture);
            }

            graphicsDevice.DisposeWhenIdle(commandList);

            graphicsDevice.WaitForIdle();
        }

        private unsafe Texture CreateTextureViaStaging(ImageSharpTexture texture, GraphicsDevice gd, ResourceFactory factory)
        {
            var staging = factory.CreateTexture(
                TextureDescription.Texture2D(
                    texture.Width,
                    texture.Height,
                    texture.MipLevels,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Staging));

            var cl = gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            for (uint level = 0; level < texture.MipLevels; level++)
            {
                var image = texture.Images[level];
                if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixelSpan))
                {
                    throw new InvalidOperationException("Unable to get image pixelspan.");
                }
                fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan))
                {
                    var map = gd.Map(staging, MapMode.Write, level);
                    var rowWidth = (uint) (image.Width * 4);
                    if (rowWidth == map.RowPitch)
                    {
                        Unsafe.CopyBlock(map.Data.ToPointer(), pin, (uint) (image.Width * image.Height * 4));
                    }
                    else
                    {
                        for (uint y = 0; y < image.Height; y++)
                        {
                            var dstStart = (byte*) map.Data.ToPointer() + y * map.RowPitch;
                            var srcStart = (byte*) pin + y * rowWidth;
                            Unsafe.CopyBlock(dstStart, srcStart, rowWidth);
                        }
                    }
                    gd.Unmap(staging, level);
                }
            }
            cl.End();

            gd.SubmitCommands(cl);
            gd.DisposeWhenIdle(cl);

            return staging;
        }

        public Vector3? Intersect(Ray ray)
        {
            float? closestIntersection = null;

            foreach (var patch in _patches)
            {
                patch.Intersect(ray, ref closestIntersection);
            }

            if (closestIntersection == null)
            {
                return null;
            }

            return ray.Position + (ray.Direction * closestIntersection.Value);
        }

        internal void Update(WaterSettings waterSettings, in TimeInterval time)
        {
            RadiusCursorDecals.Update(time);

            _materialConstantsBuffer.Value.CausticTextureIndex = waterSettings.IsRenderCaustics
                ? GetCausticsTextureIndex(time)
                : -1;

            _materialConstantsBuffer.Update(_graphicsDevice);
        }
    }
}
