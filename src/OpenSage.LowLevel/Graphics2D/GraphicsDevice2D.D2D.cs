using OpenSage.LowLevel.Graphics3D;
using SharpDX.Direct2D1;

namespace OpenSage.LowLevel.Graphics2D
{
    partial class GraphicsDevice2D
    {
        internal DeviceContext DeviceContext { get; private set; }

        internal SharpDX.DirectWrite.Factory DirectWriteFactory { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice)
        {
            var debugLevel = DebugLevel.None;
#if DEBUG
            debugLevel = DebugLevel.Information;
#endif

            using (var dxgiDevice = graphicsDevice.Device.QueryInterface<SharpDX.DXGI.Device>())
            using (var factory = new Factory1(FactoryType.SingleThreaded, debugLevel))
            using (var device = new Device(factory, dxgiDevice))
            {
                DeviceContext = AddDisposable(new DeviceContext(device, DeviceContextOptions.None));
            }

            DirectWriteFactory = AddDisposable(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated));
        }
    }
}
