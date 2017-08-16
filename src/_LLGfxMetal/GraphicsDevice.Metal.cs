using Metal;

namespace OpenZH.Graphics.LowLevel
{
    partial class GraphicsDevice
    {
        internal IMTLDevice Device { get; private set; }

        private void PlatformConstruct()
        {
            Device = MTLDevice.SystemDefault;
        }
    }
}