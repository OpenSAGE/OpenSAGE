#nullable enable

using System.IO;
using Microsoft.Win32;
using OpenSage.Data;

namespace OpenSage.Utilities;

#pragma warning disable CA1416 // Validate platform compatibility
public static class RegistryUtility
{
    public static string? GetRegistryValue(RegistryKeyPath keyPath, RegistryView registryView = RegistryView.Registry32)
    {
        // 64-bit Windows uses a separate registry for 32-bit and 64-bit applications.
        // On a 64-bit system Registry.GetValue uses the 64-bit registry by default, which is why we have to read the value the "long way".
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView);
        using var key = baseKey.OpenSubKey(keyPath.Key, false);

        var value = key?.GetValue(keyPath.ValueName, null) as string;

        if (value != null && keyPath.Append != null)
        {
            value = Path.Combine(value, keyPath.Append);
        }

        return value;
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
