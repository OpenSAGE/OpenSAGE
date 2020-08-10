using System;
using System.Runtime.InteropServices;

namespace OpenSage.Utilities
{
    public static class PlatformUtility
    {
        /// <summary>
        /// Check if current platform is windows
        /// </summary>
        /// <returns></returns>
        public static bool IsWindowsPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                case PlatformID.Win32S:
                    return true;
                default:
                    return false;
            }
        }

        public static float GetDefaultDpi()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return 96.0f;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return 72.0f;
            }
            else
            {
                return 1.0f; // TODO: What happens on Linux?
            }
        }
    }
}
