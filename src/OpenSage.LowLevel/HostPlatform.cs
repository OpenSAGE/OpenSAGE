using LL.Graphics3D;

namespace OpenSage.LowLevel
{
    public static class HostPlatform
    {
        public static GraphicsDevice GraphicsDevice { get; private set; }

        public static void Start()
        {
            GraphicsDevice = new GraphicsDevice();
        }

        public static void Stop()
        {
            GraphicsDevice.Dispose();
            GraphicsDevice = null;
        }
    }
}
