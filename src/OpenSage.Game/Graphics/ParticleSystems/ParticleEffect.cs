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
        }

        protected override void OnApply()
        {
            if (_dirtyFlags.HasFlag(ParticleEffectDirtyFlags.TransformConstants))
            {
                SetConstantBufferField("TransformConstants", "World", ref _world);
                _dirtyFlags &= ~ParticleEffectDirtyFlags.TransformConstants;
            }
        }

        void IEffectMatrices.SetWorld(Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= ParticleEffectDirtyFlags.TransformConstants;
        }
    }

    public sealed class ParticleMaterial : EffectMaterial
    {
        public ParticleMaterial(ParticleEffect effect)
            : base(effect)
        {
            SetProperty("LinearSampler", effect.GraphicsDevice.SamplerLinearWrap);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("ParticleTexture", texture);
        }
    }
}
