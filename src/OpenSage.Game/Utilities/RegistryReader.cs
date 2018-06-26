using System.IO;
using Microsoft.Win32;
using OpenSage.Data;

namespace OpenSage.Utilities
{
    public static class RegistryReader
    {
        public static string GetRegistryValue(RegistryKeyPath keyPath)
        {
            // 64-bit Windows uses a separate registry for 32-bit and 64-bit applications.
            // On a 64-bit system Registry.GetValue uses the 64-bit registry by default, which is why we have to read the value the "long way".
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = baseKey.OpenSubKey(keyPath.Key, false))
                {
                    var value = key?.GetValue(keyPath.ValueName, null) as string;

                    if (value != null && keyPath.Append != null)
                    {
                        value = Path.Combine(value, keyPath.Append);
                    }

                    return value;
                }
            }
        }
    }
}
