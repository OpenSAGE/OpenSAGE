using Eto.Forms;

namespace OpenSage.DataViewer
{
    public sealed class DataViewerApplication : Application
    {
        public DataViewerApplication(Eto.Platform platform)
            : base(platform)
        {
            OpenSage.Platform.CurrentPlatform = new Sdl2Platform();
            OpenSage.Platform.CurrentPlatform.Start();
        }

        protected override void Dispose(bool disposing)
        {
            OpenSage.Platform.CurrentPlatform.Stop();

            base.Dispose(disposing);
        }
    }
}
