using System;
using System.Runtime.InteropServices;
using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteEffect : Effect
    {
        private readonly DynamicBuffer<TextureConstants> _textureConstantBuffer;
        private TextureConstants _textureConstants;

        private ShaderResourceView _texture;

        private SpriteEffectDirtyFlags _dirtyFlags;

        [Flags]
        private enum SpriteEffectDirtyFlags
        {
            None = 0,

            TextureConstants = 0x1,

            Texture = 0x2,

            All = TextureConstants | Texture
        }

        public SpriteEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "SpriteVS", 
                  "SpritePS", 
                  null,
                  CreatePipelineLayoutDescription())
        {
            _textureConstantBuffer = DynamicBuffer<TextureConstants>.Create(graphicsDevice);
        }

        private static PipelineLayoutDescription CreatePipelineLayoutDescription()
        {
            return new PipelineLayoutDescription
            {
                Entries = new[]
                {
                    // TextureCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Pixel,
                        ResourceType.ConstantBuffer,
                        0),

                    // Texture
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.Texture,
                        0, 1)
                },

                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription(
                        ShaderStageVisibility.Pixel,
                        0,
                        new SamplerStateDescription(SamplerFilter.MinMagMipPoint))
                }
            };
        }

        protected override void OnBegin()
        {
            _dirtyFlags = SpriteEffectDirtyFlags.All;
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(SpriteEffectDirtyFlags.TextureConstants))
            {
                _textureConstantBuffer.UpdateData(_textureConstants);

                commandEncoder.SetInlineConstantBuffer(0, _textureConstantBuffer);

                _dirtyFlags &= ~SpriteEffectDirtyFlags.TextureConstants;
            }

            if (_dirtyFlags.HasFlag(SpriteEffectDirtyFlags.Texture))
            {
                commandEncoder.SetShaderResourceView(1, _texture);
                _dirtyFlags &= ~SpriteEffectDirtyFlags.Texture;
            }
        }

        public void SetMipMapLevel(uint mipMapLevel)
        {
            _textureConstants.MipMapLevel = mipMapLevel;
            _dirtyFlags |= SpriteEffectDirtyFlags.TextureConstants;
        }

        public void SetTexture(ShaderResourceView texture)
        {
            _texture = texture;
            _dirtyFlags |= SpriteEffectDirtyFlags.Texture;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextureConstants
        {
            public uint MipMapLevel;
        }
    }
}
