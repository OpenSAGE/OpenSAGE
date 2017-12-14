using System;

namespace OpenSage.LowLevel
{
    public sealed partial class HostCursor : IDisposable
    {
        public HostCursor(string filePath)
        {
            PlatformConstruct(filePath);
        }

        public void Dispose()
        {
            PlatformDispose();
        }
    }
}
