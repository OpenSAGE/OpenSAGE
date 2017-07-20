using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class GraphicsDevice
    {
        internal Device Device { get; private set; }

        private void PlatformConstruct()
        {
#if DEBUG
            DebugInterface.Get().EnableDebugLayer();
#endif

            Device = AddDisposable(new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0));
        }
    }
}
