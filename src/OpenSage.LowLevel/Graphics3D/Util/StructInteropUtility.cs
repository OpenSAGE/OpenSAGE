using System.Runtime.InteropServices;

namespace LL.Graphics3D.Util
{
    public static class StructInteropUtility
    {
        public static byte[] ToBytes<T>(ref T value)
            where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var result = new byte[size];

            var handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(result, GCHandleType.Pinned);

                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            return result;
        }

        public static byte[] ToBytes<T>(T[] values)
        {
            var size = Marshal.SizeOf<T>();
            var result = new byte[values.Length * size];

            var handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(result, GCHandleType.Pinned);
                var baseAddress = handle.AddrOfPinnedObject();

                for (var i = 0; i < values.Length; i++)
                {
                    Marshal.StructureToPtr(values[i], baseAddress + i * size, false);
                }
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            return result;
        }
    }
}
