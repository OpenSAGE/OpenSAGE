using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleMaterial : EffectMaterial
    {
        public ParticleMaterial(Effect effect)
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