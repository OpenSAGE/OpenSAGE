using OpenSage.Graphics.Effects;
using Veldrid;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleMaterial : EffectMaterial
    {
        public override uint? SlotGlobalConstantsShared => 0;
        public override uint? SlotGlobalConstantsVS => 1;
        public override uint? SlotRenderItemConstantsVS => 2;

        public ParticleMaterial(Effect effect)
            : base(effect)
        {
            SetProperty(3, effect.GraphicsDevice.LinearSampler);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty(4, texture);
        }
    }
}
