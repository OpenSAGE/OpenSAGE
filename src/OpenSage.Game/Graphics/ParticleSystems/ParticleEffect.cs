using System;
using System.Numerics;
using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleEffect : Effect, IEffectMatrices
    {
        private ParticleEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        [Flags]
        private enum ParticleEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x1,

            All = TransformConstants
        }

        public ParticleEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "ParticleVS", 
                  "ParticlePS",
                  ParticleVertex.VertexDescriptor)
        {
        }

        protected override void OnBegin()
        {
            _dirtyFlags = ParticleEffectDirtyFlags.All;

            SetValue("LinearSampler", GraphicsDevice.SamplerLinearWrap);
        }

        protected override void OnApply()
        {
            if (_dirtyFlags.HasFlag(ParticleEffectDirtyFlags.TransformConstants))
            {
                SetConstantBufferField("ParticleTransformCB", "World", ref _world);
                SetConstantBufferField("ParticleTransformCB", "ViewProjection", _view * _projection);

                var result = Matrix4x4.Invert(_view, out var viewInverse);
                SetConstantBufferField("ParticleTransformCB", "CameraPosition", viewInverse.Translation);

                _dirtyFlags &= ~ParticleEffectDirtyFlags.TransformConstants;
            }
        }

        public void SetWorld(Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= ParticleEffectDirtyFlags.TransformConstants;
        }

        public void SetView(Matrix4x4 matrix)
        {
            _view = matrix;
            _dirtyFlags |= ParticleEffectDirtyFlags.TransformConstants;
        }

        public void SetProjection(Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= ParticleEffectDirtyFlags.TransformConstants;
        }

        public void SetTexture(Texture texture)
        {
            SetValue("ParticleTexture", texture);
        }
    }
}
