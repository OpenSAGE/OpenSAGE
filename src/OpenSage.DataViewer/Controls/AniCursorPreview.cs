using Eto;
using Eto.Forms;
using OpenSage.Data;

namespace OpenSage.DataViewer.Controls
{
    [Handler(typeof(IAniCursorPreview))]
    public class AniCursorPreview : Control
    {
        private new IAniCursorPreview Handler => (IAniCursorPreview) base.Handler;

        public FileSystemEntry AniFile
        {
            get { return Handler.AniFile; }
            set { Handler.AniFile = value; }
        }

        public interface IAniCursorPreview : IHandler
        {
            FileSystemEntry AniFile { get; set; }
        }
    }
}
