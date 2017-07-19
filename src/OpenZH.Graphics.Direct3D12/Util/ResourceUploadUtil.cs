using System;
using SharpDX;

namespace OpenZH.Graphics.Direct3D12.Util
{
    internal static class ResourceUploadUtil
    {
        public static void MemcpySubresource(
            IntPtr destinationDataPtr,
            int destinationRowPitch,
            byte[] sourceData,
            int sourceRowPitch,
            int rowSizeInBytes,
            int numRows)
        {
            for (var y = 0; y < numRows; y++)
            {
                Utilities.Write(
                    destinationDataPtr + (destinationRowPitch * y),
                    sourceData,
                    sourceRowPitch * y,
                    rowSizeInBytes);
            }
        }
    }
}
