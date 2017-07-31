using System;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics.Platforms.Direct3D12
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
