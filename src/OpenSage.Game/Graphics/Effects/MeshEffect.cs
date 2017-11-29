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

        private readonly Buffer<MeshTransformConstants> _transformConstantBuffer;
        private MeshTransformConstants _transformConstants;

        private readonly Buffer<LightingConstants> _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        private readonly Buffer<PerDrawConstants> _perDrawConstantBuffer;
        private PerDrawConstants _perDrawConstants;

        private MeshEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        [Flags]
        private enum MeshEffectDirtyFlags
        {
            None = 0,

            PerDrawConstants = 0x1,

            SkinningConstants = 0x2,

            TransformConstants = 0x4,

            LightingConstants = 0x8,

            All = PerDrawConstants
                | SkinningConstants
                | TransformConstants
                | LightingConstants
        }

        public MeshEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "FixedFunctionVS", 
                  "FixedFunctionPS",
                  MeshVertex.VertexDescriptor)
        {
            _transformConstantBuffer = AddDisposable(Buffer<MeshTransformConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer));

            _lightingConstantBuffer = AddDisposable(Buffer<LightingConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer));

            _perDrawConstantBuffer = AddDisposable(Buffer<PerDrawConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer));
        }
        
        protected override void OnBegin(CommandEncoder commandEncoder)
        {
            _dirtyFlags = MeshEffectDirtyFlags.All;

            SetValue("Sampler", GraphicsDevice.SamplerAnisotropicWrap);
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.ViewProjection = _view * _projection;

                _transformConstantBuffer.SetData(ref _transformConstants);

                commandEncoder.SetVertexShaderConstantBuffer(0, _transformConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.LightingConstants))
            {
                Matrix4x4.Invert(_view, out var viewInverse);
                _lightingConstants.CameraPosition = viewInverse.Translation;

                _lightingConstantBuffer.SetData(ref _lightingConstants);

                commandEncoder.SetPixelShaderConstantBuffer(0, _lightingConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.LightingConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.PerDrawConstants))
            {
                _perDrawConstantBuffer.SetData(ref _perDrawConstants);

                commandEncoder.SetPixelShaderConstantBuffer(1, _perDrawConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.PerDrawConstants;
            }
        }

        public void SetSkinningBuffer(Buffer<Matrix4x3> skinningBuffer)
        {
            SetValue("SkinningBuffer", skinningBuffer);
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

        public void SetMaterials(Buffer<VertexMaterial> materialsBuffer)
        {
            SetValue("Materials", materialsBuffer);
        }

        public void SetTexture0(Texture texture)
        {
            SetValue("Texture0", texture);
        }

        public void SetTexture1(Texture texture)
        {
            SetValue("Texture1", texture);
        }

        public void SetMaterialIndices(Buffer<uint> materialIndicesBuffer)
        {
            SetValue("MaterialIndices", materialIndicesBuffer);
        }

        public void SetShadingConfigurations(Buffer<ShadingConfiguration> shadingConfigurationsBuffer)
        {
            SetValue("ShadingConfigurations", shadingConfigurationsBuffer);
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
