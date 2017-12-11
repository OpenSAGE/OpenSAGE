using Eto;
using Eto.Forms;
using LL.Graphics3D;

namespace OpenSage.DataViewer
{
    public sealed class DataViewerApplication : Application
    {
        public new static DataViewerApplication Instance { get; private set; }

        public GraphicsDevice GraphicsDevice { get; }

        public DataViewerApplication(Platform platform)
            : base(platform)
        {
            Instance = this;

            GraphicsDevice = new GraphicsDevice();
        }

        protected override void Dispose(bool disposing)
        {
            GraphicsDevice.Dispose();

            base.Dispose(disposing);
        }
    }
}
