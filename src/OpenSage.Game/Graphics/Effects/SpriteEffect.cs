using System;
using System.Runtime.InteropServices;
using LLGfx;
using LLGfx.Effects;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteEffect : Effect
    {
        private readonly Buffer<TextureConstants> _textureConstantBuffer;
        private TextureConstants _textureConstants;

        private SpriteEffectDirtyFlags _dirtyFlags;

        [Flags]
        private enum SpriteEffectDirtyFlags
        {
            None = 0,

            TextureConstants = 0x1,

            All = TextureConstants
        }

        public SpriteEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "SpriteVS", 
                  "SpritePS", 
                  null)
        {
            _textureConstantBuffer = Buffer<TextureConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer);
        }

        protected override void OnBegin(CommandEncoder commandEncoder)
        {
            _dirtyFlags = SpriteEffectDirtyFlags.All;

            SetValue("Sampler", GraphicsDevice.SamplerPointWrap);
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(SpriteEffectDirtyFlags.TextureConstants))
            {
                _textureConstantBuffer.SetData(_textureConstants);

                commandEncoder.SetPixelShaderConstantBuffer(0, _textureConstantBuffer);

                _dirtyFlags &= ~SpriteEffectDirtyFlags.TextureConstants;
            }
        }

        public void SetMipMapLevel(uint mipMapLevel)
        {
            _textureConstants.MipMapLevel = mipMapLevel;
            _dirtyFlags |= SpriteEffectDirtyFlags.TextureConstants;
        }

        public void SetTexture(Texture texture)
        {
            SetValue("BaseTexture", texture);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextureConstants
        {
            public uint MipMapLevel;
        }
    }
}
