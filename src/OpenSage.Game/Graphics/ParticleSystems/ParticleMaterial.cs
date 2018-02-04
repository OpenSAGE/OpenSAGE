using OpenSage.Content;
using OpenSage.Graphics.Effects;
using Veldrid;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleMaterial : EffectMaterial
    {
        public ParticleMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            SetProperty("LinearSampler", effect.GraphicsDevice.LinearSampler);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("ParticleTexture", texture);
        }
    }
}
