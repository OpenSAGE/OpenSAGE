using System;
using System.Numerics;
using LLGfx;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Effects
{
    public sealed class MeshEffect : Effect, IEffectMatrices, IEffectLights, IEffectTime, IEffectViewport
    {
        private MeshEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        // Derived data.
        private Matrix4x4 _viewProjection;

        [Flags]
        private enum MeshEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x1,

            All = TransformConstants
        }

        public MeshEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "FixedFunctionVS", 
                  "FixedFunctionPS",
                  MeshVertex.VertexDescriptor)
        {
        }
        
        protected override void OnBegin()
        {
            _dirtyFlags = MeshEffectDirtyFlags.All;

            SetValue("Sampler", GraphicsDevice.SamplerAnisotropicWrap);
        }

        protected override void OnApply()
        {
            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.TransformConstants))
            {
                _viewProjection = _view * _projection;
                SetConstantBufferField("MeshTransformCB", "ViewProjection", ref _viewProjection);

                Matrix4x4.Invert(_view, out var viewInverse);
                SetConstantBufferField("LightingCB", "CameraPosition", viewInverse.Translation);

                _dirtyFlags &= ~MeshEffectDirtyFlags.TransformConstants;
            }
        }

        public void SetSkinningBuffer(Buffer<Matrix4x3> skinningBuffer)
        {
            SetValue("SkinningBuffer", skinningBuffer);
        }

        public void SetSkinningEnabled(bool enabled)
        {
            SetConstantBufferField("MeshTransformCB", "SkinningEnabled", enabled);
        }

        public void SetNumBones(uint numBones)
        {
            SetConstantBufferField("MeshTransformCB", "NumBones", numBones);
        }

        public void SetWorld(Matrix4x4 matrix) { }

        public void SetView(Matrix4x4 matrix)
        {
            _view = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetProjection(Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetLights(ref Lights lights)
        {
            SetConstantBufferField("LightingCB", "Lights", ref lights);
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
            SetConstantBufferField("PerDrawCB", "ShadingConfigurationID", shadingConfigurationID);
        }

        public void SetNumTextureStages(uint numTextureStages)
        {
            SetConstantBufferField("PerDrawCB", "NumTextureStages", numTextureStages);
        }

        public void SetTimeInSeconds(float time)
        {
            SetConstantBufferField("PerDrawCB", "TimeInSeconds", time);
        }

        public void SetViewportSize(Vector2 viewportSize)
        {
            SetConstantBufferField("PerDrawCB", "ViewportSize", ref viewportSize);
        }
    }
}
