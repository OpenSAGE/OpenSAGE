using Eto.Forms;

namespace OpenSage.DataViewer
{
    public sealed class DataViewerApplication : Application
    {
        public DataViewerApplication(Eto.Platform platform)
            : base(platform)
        {
            OpenSage.Platform.Start();
        }

        protected override void Dispose(bool disposing)
        {
            OpenSage.Platform.Stop();

            base.Dispose(disposing);
        }
    }
}
