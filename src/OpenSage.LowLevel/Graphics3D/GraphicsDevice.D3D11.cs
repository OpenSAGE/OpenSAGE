using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class GraphicsDevice
    {
        internal Device Device { get; private set; }

        private PixelFormat PlatformBackBufferFormat => PixelFormat.Bgra8UNorm;

        private void PlatformConstruct()
        {
#if DEBUG
            var flags = DeviceCreationFlags.Debug;
#else
            var flags = DeviceCreationFlags.None;
#endif

            // Required for D2D compatibility.
            flags |= DeviceCreationFlags.BgraSupport;

            Device = new Device(
                DriverType.Hardware, 
                flags, 
                FeatureLevel.Level_11_0);
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
