using System.Windows.Controls;
using Eto.Forms;
using Eto.Wpf.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.Windows.Controls
{
    internal sealed class AniCursorPreviewHandler : WpfControl<UserControl, AniCursorPreview, Eto.Forms.Control.ICallback>, AniCursorPreview.IAniCursorPreview
    {
        public AniCursorPreviewHandler()
        {
            Control = new UserControl();
        }

        private FileSystemEntry _aniFile;
        public FileSystemEntry AniFile
        {
            get => _aniFile;
            set
            {
                _aniFile = value;

                using (var aniStream = value.Open())
                {
                    Control.Cursor = new System.Windows.Input.Cursor(aniStream);
                }
            }
        }

        public override Cursor Cursor
        {
            get => base.Cursor;
            set { } // Don't do anything, since we override cursor above.
        }
    }
}
