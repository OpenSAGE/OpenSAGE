﻿using System.Runtime.InteropServices;

namespace OpenSage.Utilities
{
    internal static class StructInteropUtility
    {
        public static byte[] ToBytes<T>(ref T value)
            where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var result = new byte[size];

            MemoryMarshal.Write(result, in value);

            return result;
        }
    }
}
