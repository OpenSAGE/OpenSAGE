using Eto.Forms;
using OpenSage.LowLevel;

namespace OpenSage.DataViewer
{
    public sealed class DataViewerApplication : Application
    {
        public DataViewerApplication(Eto.Platform platform)
            : base(platform)
        {
            HostPlatform.Start();

            OpenSage.Platform.CurrentPlatform = new Sdl2Platform();
            OpenSage.Platform.CurrentPlatform.Start();
        }

        protected override void Dispose(bool disposing)
        {
            OpenSage.Platform.CurrentPlatform.Stop();
            HostPlatform.Stop();

            base.Dispose(disposing);
        }
    }
}
