using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class RenderTarget
    {
        internal IMTLTexture DeviceTexture { get; private set; }

        internal override string PlatformGetDebugName() => DeviceTexture.Label;
        internal override void PlatformSetDebugName(string value) => DeviceTexture.Label = value;

        internal RenderTarget(GraphicsDevice graphicsDevice, IMTLTexture deviceTexture)
            : base(graphicsDevice)
        {
            DeviceTexture = deviceTexture;
        }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, Texture texture)
        {
            DeviceTexture = texture.DeviceTexture;
        }
    }
}
