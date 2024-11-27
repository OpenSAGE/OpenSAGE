using System.Runtime.InteropServices;
using System;
using Xunit;

namespace OpenSage.Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WindowsOnlyFact : FactAttribute
    {
        public WindowsOnlyFact() {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Skip = "Ignore on non-Windows platforms because there is an issue with loading NuGet packaged SDL2 library in tests";
            }
        }
    }
}
