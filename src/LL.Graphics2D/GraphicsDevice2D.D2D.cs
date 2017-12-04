using SharpDX.Direct2D1;

namespace LL.Graphics2D
{
    partial class GraphicsDevice2D
    {
        internal Factory DeviceFactory { get; private set; }

        private void PlatformConstruct()
        {
#if DEBUG
            var debugLevel = DebugLevel.Warning;
#else
            var debugLevel = DebugLevel.None;
#endif

            DeviceFactory = AddDisposable(new Factory(
                FactoryType.SingleThreaded,
                debugLevel));
        }
    }
}
