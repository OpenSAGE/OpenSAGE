using LL.Graphics3D;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using D2D1RenderTarget = SharpDX.Direct2D1.RenderTarget;

namespace LL.Graphics2D
{
    partial class RenderTarget
    {
        internal D2D1RenderTarget DeviceRenderTarget { get; private set; }

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            GraphicsDevice2D graphicsDevice2D,
            int width,
            int height,
            out Texture texture)
        {
            var textureDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Height = height,
                Width = width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.Direct3D11.ResourceUsage.Default
            };

            texture = AddDisposable(new Texture(
                graphicsDevice,
                textureDescription));

            var dxgiSurface = texture.DeviceResource.QueryInterface<Surface>();

            DeviceRenderTarget = new D2D1RenderTarget(
                graphicsDevice2D.DeviceFactory,
                dxgiSurface,
                new RenderTargetProperties
                {
                    MinLevel = FeatureLevel.Level_DEFAULT,
                    PixelFormat = new SharpDX.Direct2D1.PixelFormat(
                        textureDescription.Format, 
                        SharpDX.Direct2D1.AlphaMode.Ignore),
                    Type = RenderTargetType.Default,
                    Usage = RenderTargetUsage.None
                });
        }
    }
}
