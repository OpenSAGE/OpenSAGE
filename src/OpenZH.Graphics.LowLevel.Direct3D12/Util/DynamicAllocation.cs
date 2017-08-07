using System;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics.LowLevel.Util
{
    internal struct DynamicAllocation
    {
        public Resource Buffer;
        public uint Offset;
        public uint Size;
        public IntPtr CpuAddress;
        public long GpuAddress;
    }
}
