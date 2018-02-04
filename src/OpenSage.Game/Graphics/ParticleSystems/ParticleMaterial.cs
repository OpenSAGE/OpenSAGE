using OpenSage.Graphics.Effects;
using Veldrid;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleMaterial : EffectMaterial
    {
        public ParticleMaterial(Effect effect)
            : base(effect)
        {
            SetProperty("LinearSampler", effect.GraphicsDevice.LinearSampler);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("ParticleTexture", texture);
        }
    }
}
