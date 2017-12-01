using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteEffect : Effect
    {
        public SpriteEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "SpriteVS", 
                  "SpritePS", 
                  null)
        {
        }
    }

    public sealed class SpriteMaterial : EffectMaterial
    {
        public SpriteMaterial(SpriteEffect effect) 
            : base(effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.SamplerPointWrap);
        }

        public void SetMipMapLevel(uint mipMapLevel)
        {
            SetProperty("MaterialConstants", "MipMapLevel", mipMapLevel);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("BaseTexture", texture);
        }
    }
}
