using System;
using System.Windows.Forms;

namespace OpenSage.LowLevel
{
    partial class HostCursor
    {
        internal Cursor Cursor { get; private set; }

        private void PlatformConstruct(string filePath)
        {
            var cursorHandle = LoadCursorFromFile(filePath);
            Cursor = new Cursor(cursorHandle);
        }

        private void PlatformDispose()
        {
            DestroyCursor(Cursor.Handle);
            Cursor = null;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr LoadCursorFromFile(string fileName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool DestroyCursor(IntPtr handle);
    }
}
