using System;
using SharpDX.Direct3D11;
using LLGfx.Util;
using SharpDX.Direct3D;

namespace LLGfx
{
    partial class GraphicsDevice
    {
        internal Device Device { get; private set; }

        private PixelFormat PlatformBackBufferFormat => PixelFormat.Bgra8UNorm;

        private void PlatformConstruct()
        {
#if DEBUG
            const DeviceCreationFlags flags = DeviceCreationFlags.Debug;
#else
            const DeviceCreationFlags flags = DeviceCreationFlags.None;
#endif

            Device = AddDisposable(new Device(DriverType.Hardware, flags, FeatureLevel.Level_11_0));
        }
    }
}
