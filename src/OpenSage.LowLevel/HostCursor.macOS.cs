using System;
using AppKit;

namespace OpenSage.LowLevel
{
    partial class HostCursor
    {
        internal NSCursor Cursor { get; private set; }

        private void PlatformConstruct(string filePath)
        {
            throw new NotImplementedException();
            //new NSCursor()
        }

        private void PlatformDispose()
        {
            Cursor.Dispose();
            Cursor = null;
        }
    }
}
