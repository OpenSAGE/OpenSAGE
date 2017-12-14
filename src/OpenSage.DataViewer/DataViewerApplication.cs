using Eto;
using Eto.Forms;
using OpenSage.LowLevel;

namespace OpenSage.DataViewer
{
    public sealed class DataViewerApplication : Application
    {
        public DataViewerApplication(Platform platform)
            : base(platform)
        {
            HostPlatform.Start();
        }

        protected override void Dispose(bool disposing)
        {
            HostPlatform.Stop();

            base.Dispose(disposing);
        }
    }
}
