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
            using (var dxgiDevice = graphicsDevice.Device.QueryInterface<SharpDX.DXGI.Device>())
            {
                var debugLevel = DebugLevel.None;
#if DEBUG
                debugLevel = DebugLevel.Warning;
#endif

                var factory = AddDisposable(new Factory1(FactoryType.SingleThreaded, debugLevel));
                var device = AddDisposable(new Device(factory, dxgiDevice));

                DeviceContext = AddDisposable(new DeviceContext(device, DeviceContextOptions.None));
            }

            DirectWriteFactory = AddDisposable(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated));
        }
    }
}
