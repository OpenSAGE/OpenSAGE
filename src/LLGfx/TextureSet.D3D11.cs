using SharpDX.Direct3D11;

namespace LLGfx
{
    partial class TextureSet
    {
        internal ShaderResourceView[] DeviceShaderResourceViews { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, Texture[] textures)
        {
            DeviceShaderResourceViews = new ShaderResourceView[textures.Length];

            for (var i = 0; i < textures.Length; i++)
            {
                DeviceShaderResourceViews[i] = textures[i]?.DeviceShaderResourceView;
            }
        }
    }
}
