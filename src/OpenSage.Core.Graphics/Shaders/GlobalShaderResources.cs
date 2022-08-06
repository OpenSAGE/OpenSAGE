using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Core.Graphics;
using OpenSage.Graphics.Mathematics;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    public sealed class GlobalShaderResources : DisposableBase
    {
        public readonly ResourceLayout GlobalConstantsResourceLayout;
        public readonly ResourceLayout ForwardPassResourceLayout;

        public readonly Sampler ShadowSampler;

        public readonly RadiusCursorDecalResourceData RadiusCursorDecals;

        public GlobalShaderResources(GraphicsDevice graphicsDevice)
        {
            GlobalConstantsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("GlobalConstants", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment))));

            ForwardPassResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("GlobalLightingConstantsVS", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("GlobalLightingConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Global_CloudTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Global_CloudSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Global_ShadowConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Global_ShadowMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Global_ShadowSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecalTextures", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecalSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecalConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RadiusCursorDecals", ResourceKind.StructuredBufferReadOnly, ShaderStages.Fragment))));

            ShadowSampler = AddDisposable(graphicsDevice.ResourceFactory.CreateSampler(
                new SamplerDescription(
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerFilter.MinLinear_MagLinear_MipLinear,
                    ComparisonKind.LessEqual,
                    0,
                    0,
                    0,
                    0,
                    SamplerBorderColor.OpaqueBlack)));
            ShadowSampler.Name = "Shadow Sampler";

            RadiusCursorDecals = AddDisposable(new RadiusCursorDecalResourceData(graphicsDevice));
        }

        public struct GlobalConstants
        {
            public Vector3 CameraPosition;
            public float TimeInSeconds;

            public Matrix4x4 ViewProjection;
            public Vector4 ClippingPlane1;
            public Vector4 ClippingPlane2;
            public Bool32 HasClippingPlane1;
            public Bool32 HasClippingPlane2;

            public Vector2 ViewportSize;
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        public struct Light
        {
            public const int SizeInBytes = 48;

            [FieldOffset(0)]
            public Vector3 Ambient;

            [FieldOffset(16)]
            public Vector3 Color;

            [FieldOffset(32)]
            public Vector3 Direction;
        }

        [StructLayout(LayoutKind.Sequential, Size = SizeInBytes)]
        public struct LightingConstantsVS
        {
            public const int SizeInBytes = 64;

            public Matrix4x4 CloudShadowMatrix;
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        public struct LightingConstantsPS
        {
            public const int SizeInBytes = LightingConfiguration.SizeInBytes * 2;

            [FieldOffset(0)]
            public LightingConfiguration Terrain;

            [FieldOffset(LightingConfiguration.SizeInBytes)]
            public LightingConfiguration Object;
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        public struct LightingConfiguration
        {
            public const int SizeInBytes = Light.SizeInBytes * 3;

            [FieldOffset(0)]
            public Light Light0;

            [FieldOffset(48)]
            public Light Light1;

            [FieldOffset(96)]
            public Light Light2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ShadowConstantsPS
        {
            public Matrix4x4 ShadowMatrix;
            public float CascadeSplit0;
            public float CascadeSplit1;
            public float CascadeSplit2;
            public float CascadeSplit3;
            public Vector4 CascadeOffset0;
            public Vector4 CascadeOffset1;
            public Vector4 CascadeOffset2;
            public Vector4 CascadeOffset3;
            public Vector4 CascadeScale0;
            public Vector4 CascadeScale1;
            public Vector4 CascadeScale2;
            public Vector4 CascadeScale3;
            public float Bias;
            public float OffsetScale;
            public uint VisualizeCascades;
            public uint FilterAcrossCascades;
            public float ShadowDistance;
            public ShadowsType ShadowsType;
            public uint NumSplits;
#pragma warning disable IDE1006, CS0169
            private readonly float _padding;
#pragma warning restore IDE1006, CS0169

            public void Set(uint numCascades, ShadowSettings settings, ShadowData data)
            {
                ShadowMatrix = data.ShadowMatrix;

                SetArrayData(
                    numCascades,
                    data.CascadeSplits,
                    out CascadeSplit0,
                    out CascadeSplit1,
                    out CascadeSplit2,
                    out CascadeSplit3);

                SetArrayData(
                    numCascades,
                    data.CascadeOffsets,
                    out CascadeOffset0,
                    out CascadeOffset1,
                    out CascadeOffset2,
                    out CascadeOffset3);

                SetArrayData(
                    numCascades,
                    data.CascadeScales,
                    out CascadeScale0,
                    out CascadeScale1,
                    out CascadeScale2,
                    out CascadeScale3);

                Bias = settings.Bias;
                OffsetScale = settings.NormalOffset;
                VisualizeCascades = settings.VisualizeCascades ? 1u : 0u;
                FilterAcrossCascades = 1u;
                ShadowDistance = settings.ShadowDistance;
                ShadowsType = settings.ShadowsType;
                NumSplits = numCascades;
            }

            private static void SetArrayData<T>(
                uint numValues,
                in T[] values,
                out T value1,
                out T value2,
                out T value3,
                out T value4)
                where T : struct
            {
                value1 = values[0];
                value2 = (numValues > 1) ? values[1] : default;
                value3 = (numValues > 2) ? values[2] : default;
                value4 = (numValues > 3) ? values[3] : default;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = SizeInBytes)]
        public struct WaterConstantsPS
        {
            public const int SizeInBytes = 16 * 4;

            public Vector2 UVOffset;

            public float FarPlaneDistance;
            public float NearPlaneDistance;

            public uint IsRenderReflection;
            public uint IsRenderRefraction;

            public float TransparentWaterMinOpacity;
            public float TransparentWaterDepth;
            // Figure out when how to use those correctly
            public ColorRgbaF DiffuseColor;
            public ColorRgbaF TransparentDiffuseColor;
        }

        public struct RadiusCursorDecalConstants
        {
            public Vector3 _Padding;
            public uint NumRadiusCursorDecals;
        }
    }

    public sealed class RadiusCursorDecalResourceData : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly ConstantBuffer<GlobalShaderResources.RadiusCursorDecalConstants> _decalConstantBuffer;

        private readonly Dictionary<Texture, uint> _nameToTextureIndex;

        // TODO: Support non-512px texture sizes.
        private const uint TextureSize = 512;
        private static readonly uint TextureMipLevels = TextureMipMapData.CalculateMipMapCount(TextureSize, TextureSize);

        private const uint MaxTextures = 20; // This can be increased if necessary.
        private uint _nextTextureIndex;

        public const uint MaxDecals = 8;
        private readonly RadiusCursorDecal[] _decals;

        public readonly DeviceBuffer DecalConstants;

        public readonly DeviceBuffer DecalsBuffer;

        public readonly Texture TextureArray;

        public RadiusCursorDecalResourceData(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            _decalConstantBuffer = AddDisposable(new ConstantBuffer<GlobalShaderResources.RadiusCursorDecalConstants>(
                graphicsDevice,
                "RadiusCursorDecalConstants"));

            DecalConstants = _decalConstantBuffer.Buffer;

            DecalsBuffer = AddDisposable(graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    RadiusCursorDecal.SizeInBytes * MaxDecals,
                    BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic,
                    RadiusCursorDecal.SizeInBytes,
                    true)));

            _decals = new RadiusCursorDecal[MaxDecals];

            _nameToTextureIndex = new Dictionary<Texture, uint>();

            TextureArray = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    TextureSize,
                    TextureSize,
                    TextureMipLevels,
                    MaxTextures,
                    PixelFormat.BC3_UNorm, // TODO: Allow other types
                    TextureUsage.Sampled)));
        }

        public void SetNumRadiusCursorDecals(uint value)
        {
            _decalConstantBuffer.Value.NumRadiusCursorDecals = value;
            _decalConstantBuffer.Update(_graphicsDevice);
        }

        public void SetDecal(int index, in RadiusCursorDecal decal)
        {
            _decals[index] = decal;
        }

        public void UpdateDecalsBuffer()
        {
            _graphicsDevice.UpdateBuffer(DecalsBuffer, 0, _decals);
        }

        public uint GetTextureIndex(Texture texture)
        {
            if (!_nameToTextureIndex.TryGetValue(texture, out var result))
            {
                if (_nextTextureIndex == MaxTextures ||
                    texture.Width != texture.Height ||
                    texture.Width != TextureSize)
                {
                    throw new InvalidOperationException();
                }

                result = _nextTextureIndex;

                _nameToTextureIndex.Add(texture, _nextTextureIndex);

                var commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
                commandList.Begin();

                for (var mipLevel = 0u; mipLevel < TextureMipLevels; mipLevel++)
                {
                    var mipSize = TextureMipMapData.CalculateMipSize(mipLevel, TextureSize);

                    commandList.CopyTexture(
                        texture,
                        0, 0, 0,
                        mipLevel,
                        0,
                        TextureArray,
                        0, 0, 0,
                        mipLevel,
                        _nextTextureIndex,
                        mipSize,
                        mipSize,
                        1,
                        1);
                }

                commandList.End();

                _graphicsDevice.SubmitCommands(commandList);

                _graphicsDevice.DisposeWhenIdle(commandList);

                _graphicsDevice.WaitForIdle();

                _nextTextureIndex++;
            }

            return result;
        }
    }
}
