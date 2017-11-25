using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using LLGfx.Effects;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Effects
{
    public sealed class MeshEffect : Effect, IEffectMatrices, IEffectLights, IEffectTime, IEffectViewport
    {
        public const int MaxTextures = 32;

        private DynamicBuffer<Matrix4x3> _skinningBuffer;

        private readonly DynamicBuffer<MeshTransformConstants> _transformConstantBuffer;
        private MeshTransformConstants _transformConstants;

        private readonly DynamicBuffer<LightingConstants> _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        private readonly DynamicBuffer<PerDrawConstants> _perDrawConstantBuffer;
        private PerDrawConstants _perDrawConstants;

        private MeshEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        private StaticBuffer<VertexMaterial> _materialsBuffer;
        private Texture _texture0;
        private Texture _texture1;
        private StaticBuffer<uint> _materialIndicesBuffer;
        private StaticBuffer<ShadingConfiguration> _shadingConfigurationsBuffer;

        [Flags]
        private enum MeshEffectDirtyFlags
        {
            None = 0,

            PerDrawConstants = 0x1,

            SkinningConstants = 0x2,

            TransformConstants = 0x4,

            LightingConstants = 0x8,

            MaterialsBuffer = 0x10,
            Texture0 = 0x20,
            Texture1 = 0x40,
            MaterialIndicesBuffer = 0x80,
            ShadingConfigurationsBuffer = 0x100,

            All = PerDrawConstants
                | SkinningConstants
                | TransformConstants
                | LightingConstants
                | MaterialsBuffer
                | Texture0
                | Texture1
                | MaterialIndicesBuffer
                | ShadingConfigurationsBuffer
        }

        public MeshEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "FixedFunctionVS", 
                  "FixedFunctionPS",
                  CreateVertexDescriptor(),
                  CreatePipelineLayoutDescription())
        {
            _transformConstantBuffer = AddDisposable(DynamicBuffer<MeshTransformConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer<LightingConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer));

            _perDrawConstantBuffer = AddDisposable(DynamicBuffer<PerDrawConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer));
        }

        private static VertexDescriptor CreateVertexDescriptor()
        {
            return new VertexDescriptor(
                 new[]
                 {
                    new VertexAttributeDescription(InputClassification.PerVertexData, "POSITION", 0, VertexFormat.Float3, 0, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "NORMAL", 0, VertexFormat.Float3, 12, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "BLENDINDICES", 0, VertexFormat.UInt, 24, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 0, VertexFormat.Float2, 0, 1),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 1, VertexFormat.Float2, 8, 1),
                    new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 2, VertexFormat.Float4, 0, 2),
                    new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 3, VertexFormat.Float4, 16, 2),
                    new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 4, VertexFormat.Float4, 32, 2),
                    new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 5, VertexFormat.Float4, 48, 2),
                 },
                 new[]
                 {
                    new VertexLayoutDescription(28),
                    new VertexLayoutDescription(16),
                    new VertexLayoutDescription(64)
                 });
        }

        private static PipelineLayoutDescription CreatePipelineLayoutDescription()
        {
            return new PipelineLayoutDescription
            {
                Entries = new[]
                {
                    // PerDrawCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Pixel,
                        ResourceType.ConstantBuffer,
                        1),

                    // MeshTransformCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Vertex,
                        ResourceType.ConstantBuffer,
                        0),

                    // SkinningBuffer
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Vertex,
                        ResourceType.StructuredBuffer,
                        0),

                    // LightingCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Pixel,
                        ResourceType.ConstantBuffer,
                        0),

                    // Sorted in descending frequency of updating

                    // MaterialIndices
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Vertex,
                        ResourceType.StructuredBuffer,
                        1, 1),

                    // ShadingConfigurations
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.StructuredBuffer,
                        1, 1),

                    // Materials
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.StructuredBuffer,
                        0, 1),

                    // Texture0
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.Texture,
                        2, 1),

                    // Texture1
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.Texture,
                        3, 1),
                },

                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription(
                        ShaderStageVisibility.Pixel,
                        0,
                        new SamplerStateDescription(SamplerFilter.Anisotropic))
                }
            };
        }

        protected override void OnBegin()
        {
            _dirtyFlags = MeshEffectDirtyFlags.All;
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.SkinningConstants))
            {
                if (_skinningBuffer != null)
                {
                    commandEncoder.SetInlineStructuredBuffer(2, _skinningBuffer);
                }

                _dirtyFlags &= ~MeshEffectDirtyFlags.SkinningConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.ViewProjection = _view * _projection;

                _transformConstantBuffer.UpdateData(ref _transformConstants);

                commandEncoder.SetInlineConstantBuffer(1, _transformConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.LightingConstants))
            {
                Matrix4x4.Invert(_view, out var viewInverse);
                _lightingConstants.CameraPosition = viewInverse.Translation;

                _lightingConstantBuffer.UpdateData(ref _lightingConstants);

                commandEncoder.SetInlineConstantBuffer(3, _lightingConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.LightingConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.PerDrawConstants))
            {
                _perDrawConstantBuffer.UpdateData(ref _perDrawConstants);

                commandEncoder.SetInlineConstantBuffer(0, _perDrawConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.PerDrawConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.ShadingConfigurationsBuffer))
            {
                commandEncoder.SetStaticBuffer(5, _shadingConfigurationsBuffer);
                _dirtyFlags &= ~MeshEffectDirtyFlags.ShadingConfigurationsBuffer;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.MaterialsBuffer))
            {
                commandEncoder.SetStaticBuffer(6, _materialsBuffer);
                _dirtyFlags &= ~MeshEffectDirtyFlags.MaterialsBuffer;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.Texture0))
            {
                commandEncoder.SetTexture(7, _texture0);
                _dirtyFlags &= ~MeshEffectDirtyFlags.Texture0;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.Texture1))
            {
                commandEncoder.SetTexture(8, _texture1);
                _dirtyFlags &= ~MeshEffectDirtyFlags.Texture1;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.MaterialIndicesBuffer))
            {
                commandEncoder.SetStaticBuffer(4, _materialIndicesBuffer);
                _dirtyFlags &= ~MeshEffectDirtyFlags.MaterialIndicesBuffer;
            }
        }

        public void SetSkinningBuffer(DynamicBuffer<Matrix4x3> skinningBuffer)
        {
            if (skinningBuffer == _skinningBuffer)
            {
                return;
            }
            _skinningBuffer = skinningBuffer;
            _dirtyFlags |= MeshEffectDirtyFlags.SkinningConstants;
        }

        public void SetSkinningEnabled(bool enabled)
        {
            _transformConstants.SkinningEnabled = enabled;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetNumBones(uint numBones)
        {
            _transformConstants.NumBones = numBones;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetWorld(Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetView(Matrix4x4 matrix)
        {
            _view = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
            _dirtyFlags |= MeshEffectDirtyFlags.LightingConstants;
        }

        public void SetProjection(Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetLights(ref Lights lights)
        {
            _lightingConstants.Lights = lights;
            _dirtyFlags |= MeshEffectDirtyFlags.LightingConstants;
        }

        public void SetMaterials(StaticBuffer<VertexMaterial> materialsBuffer)
        {
            _materialsBuffer = materialsBuffer;
            _dirtyFlags |= MeshEffectDirtyFlags.MaterialsBuffer;
        }

        public void SetTexture0(Texture texture)
        {
            _texture0 = texture;
            _dirtyFlags |= MeshEffectDirtyFlags.Texture0;
        }

        public void SetTexture1(Texture texture)
        {
            _texture1 = texture;
            _dirtyFlags |= MeshEffectDirtyFlags.Texture1;
        }

        public void SetMaterialIndices(StaticBuffer<uint> materialIndicesBuffer)
        {
            _materialIndicesBuffer = materialIndicesBuffer;
            _dirtyFlags |= MeshEffectDirtyFlags.MaterialIndicesBuffer;
        }

        public void SetShadingConfigurations(StaticBuffer<ShadingConfiguration> shadingConfigurationsBuffer)
        {
            _shadingConfigurationsBuffer = shadingConfigurationsBuffer;
            _dirtyFlags |= MeshEffectDirtyFlags.ShadingConfigurationsBuffer;
        }

        public void SetShadingConfigurationID(uint shadingConfigurationID)
        {
            _perDrawConstants.ShadingConfigurationID = shadingConfigurationID;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        public void SetNumTextureStages(uint numTextureStages)
        {
            _perDrawConstants.NumTextureStages = numTextureStages;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        public void SetTimeInSeconds(float time)
        {
            _perDrawConstants.TimeInSeconds = time;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        public void SetViewportSize(Vector2 viewportSize)
        {
            _perDrawConstants.ViewportSize = viewportSize;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshTransformConstants
        {
            public Matrix4x4 ViewProjection;
            public bool SkinningEnabled;
            public uint NumBones;
        }

        [StructLayout(LayoutKind.Explicit, Size = 20)]
        private struct PerDrawConstants
        {
            [FieldOffset(0)]
            public uint ShadingConfigurationID;

            // Not actually per-draw, but we don't have a per-mesh CB.
            [FieldOffset(4)]
            public uint NumTextureStages;

            [FieldOffset(8)]
            public float TimeInSeconds;

            // Not actually per-draw
            [FieldOffset(12)]
            public Vector2 ViewportSize;
        }
    }
}
