using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Data.Tga;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using Veldrid.ImageSharp;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Terrain
{
    public sealed class Terrain : DisposableBase
    {
        private readonly ShaderSet _shaderSet;
        private readonly Pipeline _pipeline;

        internal const int PatchSize = 17;

        public HeightMap HeightMap { get; }

        public IReadOnlyList<TerrainPatch> Patches { get; }

        public ResourceSet CloudResourceSet { get; }

        private float _causticsIndex;
        private DeltaTimer _deltaTimer;

        private List<Texture> _causticsTextureList;
        private readonly uint _numOfCausticsAnimation = 32;

        internal Terrain(MapFile mapFile, AssetLoadContext loadContext)
        {
            HeightMap = new HeightMap(mapFile.HeightMapData);

            var indexBufferCache = AddDisposable(new TerrainPatchIndexBufferCache(loadContext.GraphicsDevice));

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

            var terrainPipeline = loadContext.ShaderResources.Terrain.Pipeline;

            var materialConstantsBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                new TerrainShaderResources.TerrainMaterialConstants
                {
                    MapBorderWidth = new Vector2(mapFile.HeightMapData.BorderWidth, mapFile.HeightMapData.BorderWidth) * HeightMap.HorizontalScale,
                    MapSize = new Vector2(mapFile.HeightMapData.Width, mapFile.HeightMapData.Height) * HeightMap.HorizontalScale,
                    IsMacroTextureStretched = false // TODO: This must be one of the EnvironmentData unknown values.
                },
                BufferUsage.UniformBuffer));

            var macroTexture = loadContext.AssetStore.Textures.GetByName(mapFile.EnvironmentData?.MacroTexture ?? "tsnoiseurb.dds");

            BuildCausticsTextureArray(loadContext.AssetStore);
            Func<ResourceSet> causticsRenderer = () =>
            {
                UpdateTimer();
                var casuticsTexture = GetCausticsTexture();

                var materialResourceSet = AddDisposable(loadContext.ShaderResources.Terrain.CreateMaterialResourceSet(
                    materialConstantsBuffer,
                    tileDataTexture,
                    cliffDetailsBuffer ?? loadContext.StandardGraphicsResources.GetNullStructuredBuffer(TerrainShaderResources.CliffInfo.Size),
                    textureDetailsBuffer,
                    textureArray,
                    macroTexture,
                    casuticsTexture));
                return materialResourceSet;
            };

            var casuticsTexture = loadContext.StandardGraphicsResources.SolidBlackTexture;
            var materialResourceSet = AddDisposable(loadContext.ShaderResources.Terrain.CreateMaterialResourceSet(
                materialConstantsBuffer,
                tileDataTexture,
                cliffDetailsBuffer ?? loadContext.StandardGraphicsResources.GetNullStructuredBuffer(TerrainShaderResources.CliffInfo.Size),
                textureDetailsBuffer,
                textureArray,
                macroTexture,
                casuticsTexture));

            Patches = CreatePatches(
                loadContext.GraphicsDevice,
                HeightMap,
                indexBufferCache,
                materialResourceSet,
                causticsRenderer);

            var cloudTexture = loadContext.AssetStore.Textures.GetByName(mapFile.EnvironmentData?.CloudTexture ?? "tscloudmed.dds");

            var cloudResourceLayout = AddDisposable(loadContext.GraphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Global_CloudTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment))));

            CloudResourceSet = AddDisposable(loadContext.GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    cloudResourceLayout,
                    cloudTexture.Texture)));
            CloudResourceSet.Name = "Cloud resource set";

            _shaderSet = loadContext.ShaderResources.Terrain.ShaderSet;
            _pipeline = terrainPipeline;
        }

        private void UpdateTimer()
        {
            if (_deltaTimer == null)
            {
                _deltaTimer = new DeltaTimer();
                _deltaTimer.Start();
            }
            else
            {
                _deltaTimer.Update();
            }
        }

        private Texture GetCausticsTexture()
        {
            var deltaTime = (float) _deltaTimer.CurrentGameTime.DeltaTime.TotalSeconds;
            _causticsIndex += 10f * deltaTime;
            if (_causticsIndex >= _numOfCausticsAnimation)
                _causticsIndex = 0;
            return _causticsTextureList[(int) _causticsIndex];
        }

        private void BuildCausticsTextureArray(AssetStore assetStore)
        {
            _causticsTextureList = new List<Texture>();
            var baseTextureName = "causts";
            var textureFormat = ".tga";

            for (int i = 0; i < _numOfCausticsAnimation; i++)
            {
                var numString = i < 10 ? ("0" + i.ToString()) : i.ToString();
                var texture = assetStore.Textures.GetByName(baseTextureName + numString + textureFormat);
                _causticsTextureList.Add(texture);
            }
        }

        private List<TerrainPatch> CreatePatches(
            GraphicsDevice graphicsDevice,
            HeightMap heightMap,
            TerrainPatchIndexBufferCache indexBufferCache,
            ResourceSet materialResourceSet,
            Func<ResourceSet> causticsRendererCallback)
        {
            const int numTilesPerPatch = PatchSize - 1;

            var heightMapWidthMinusOne = heightMap.Width - 1;
            var numPatchesX = heightMapWidthMinusOne / numTilesPerPatch;
            if (heightMapWidthMinusOne % numTilesPerPatch != 0)
            {
                numPatchesX += 1;
            }

            var heightMapHeightMinusOne = heightMap.Height - 1;
            var numPatchesY = heightMapHeightMinusOne / numTilesPerPatch;
            if (heightMapHeightMinusOne % numTilesPerPatch != 0)
            {
                numPatchesY += 1;
            }

            var patches = new List<TerrainPatch>();

            for (var y = 0; y < numPatchesY; y++)
            {
                for (var x = 0; x < numPatchesX; x++)
                {
                    var patchX = x * numTilesPerPatch;
                    var patchY = y * numTilesPerPatch;

                    var patchBounds = new Rectangle(
                        patchX,
                        patchY,
                        Math.Min(PatchSize, heightMap.Width - patchX),
                        Math.Min(PatchSize, heightMap.Height - patchY));

                    patches.Add(AddDisposable(new TerrainPatch(
                        heightMap,
                        patchBounds,
                        graphicsDevice,
                        indexBufferCache,
                        materialResourceSet,
                        causticsRendererCallback)));
                }
            }

            return patches;
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
                    CalculateMipMapCount(largestTextureSize, largestTextureSize),
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
                        tgaImage.Mutate(x => x.Resize((int) largestTextureSize, (int) largestTextureSize));
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

        private static uint CalculateMipMapCount(uint width, uint height)
        {
            return 1u + (uint) MathF.Floor(MathF.Log(Math.Max(width, height), 2));
        }

        public Vector3? Intersect(Ray ray)
        {
            float? closestIntersection = null;

            foreach (var patch in Patches)
            {
                patch.Intersect(ray, ref closestIntersection);
            }

            if (closestIntersection == null)
            {
                return null;
            }

            return ray.Position + (ray.Direction * closestIntersection.Value);
        }

        internal void BuildRenderList(RenderList renderList)
        {
            foreach (var patch in Patches)
            {
                patch.BuildRenderList(
                    renderList,
                    _shaderSet,
                    _pipeline);
            }
        }
    }
}
