using Metal;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class GraphicsDevice
    {
        internal IMTLDevice Device { get; private set; }

        private PixelFormat PlatformBackBufferFormat => PixelFormat.Bgra8UNorm;

        private void PlatformConstruct()
        {
            Device = MTLDevice.SystemDefault;
        }
    }
}
