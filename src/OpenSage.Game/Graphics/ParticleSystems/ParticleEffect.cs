using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleEffect : Effect
    {
        public ParticleEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "ParticleVS", 
                  "ParticlePS",
                  ParticleVertex.VertexDescriptor)
        {
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
