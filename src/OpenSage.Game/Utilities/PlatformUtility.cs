using System;

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
    }
}
