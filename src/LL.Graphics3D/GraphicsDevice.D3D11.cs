using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace LL.Graphics3D
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

            Device = new Device(DriverType.Hardware, flags, FeatureLevel.Level_11_0);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            Device.ImmediateContext.ClearState();

            base.Dispose(disposeManagedResources);

            Device.ImmediateContext.Dispose();

#if DEBUG
            using (var deviceDebug = AddDisposable(new DeviceDebug(Device)))
            {
                deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
            }
#endif

            Device.Dispose();
        }
    }
}
