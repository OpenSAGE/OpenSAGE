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

        protected override void OnBegin()
        {
            SetValue("Sampler", GraphicsDevice.SamplerPointWrap);
        }

        public void SetMipMapLevel(uint mipMapLevel)
        {
            SetConstantBufferField("TextureCB", "MipMapLevel", mipMapLevel);
        }

        public void SetTexture(Texture texture)
        {
            SetValue("BaseTexture", texture);
        }
    }
}
