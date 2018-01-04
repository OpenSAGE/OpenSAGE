using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.LowLevel
{
    public static class HostPlatform
    {
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static GraphicsDevice2D GraphicsDevice2D { get; private set; }

        public static void Start()
        {
            GraphicsDevice = new GraphicsDevice();
            GraphicsDevice2D = new GraphicsDevice2D(GraphicsDevice);
        }

        public static void Stop()
        {
            GraphicsDevice2D.Dispose();
            GraphicsDevice2D = null;

            GraphicsDevice.Dispose();
            GraphicsDevice = null;
        }
    }
}
